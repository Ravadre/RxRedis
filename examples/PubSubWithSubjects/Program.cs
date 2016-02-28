using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RxRedis;
using StackExchange.Redis;

namespace PubSubWithSubjects
{
    class Program
    {
        static void Main(string[] args)
        {
            var redis = ConnectionMultiplexer.Connect("localhost");

            // Create a subject, the same way standard one would be created.
            // Provided name should be unique for single subject (publisher)
            // but can be used by multiple observables.
            var sbj = new RedisSubject<long>("s1", redis);

            // Subcribe an observable to a subject, this
            // could be done from different process.
            var obs = RedisObservable.Create<long>("s1", redis);

            // RedisObservable implements IObservable<T> and therefore
            // can be used with Reactive Extensions methods like 
            // .Where operator
            obs.Where(x => x % 2 == 0)
               .Subscribe(Console.WriteLine,
                    exn => Console.WriteLine("Error: " + exn.Message),
                    () => Console.WriteLine("Done"));

            sbj.OnNext(4);
            sbj.OnNext(5);
            sbj.OnNext(6);
            sbj.OnNext(7);
            sbj.OnNext(8);
            sbj.OnNext(9);
            sbj.OnCompleted();

            Console.ReadLine();
        }
    }
}
