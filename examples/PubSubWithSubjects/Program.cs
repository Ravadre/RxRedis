using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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
            var sbj = new RedisSubject<long>("s1", redis);

            sbj.Subscribe(x =>
            {
                Console.WriteLine(x);
            },
            () =>
            {
                Console.WriteLine("Done");
            });

            Observable.Interval(TimeSpan.FromSeconds(0.2)).Subscribe(idx =>
            {
                if (idx < 5)
                {
                    sbj.OnNext(idx);
                }
                else
                {
                    sbj.OnCompleted();
                }
            });
                
            Console.ReadLine();
        }
    }
}
