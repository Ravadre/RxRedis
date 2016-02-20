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
            //var sbj = new RedisAsyncSubject<long>("s1", redis);
            //var sbj = new AsyncSubject<long>();
            
            var sbj = new RedisSubject<long>("s1", redis);
            //var sbj = new Subject<long>();

            sbj.OnNext(5);
            sbj.OnNext(6);
            sbj.OnNext(7);

            sbj.OnError(new Exception("test"));

            sbj.Subscribe(x =>
            {
                Console.WriteLine(x);
            },
            exn =>
            {
                Console.WriteLine(exn.Message);  
            },
            () =>
            {
                Console.WriteLine("Done");
            });

            sbj.OnNext(8);
            sbj.OnNext(9);
            Console.WriteLine("Before complete");
            sbj.OnError(new Exception("test"));

            Console.ReadLine();
        }
    }
}
