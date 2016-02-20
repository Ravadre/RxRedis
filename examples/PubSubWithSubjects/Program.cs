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
            var sbj = new RedisSubject<long>("s1", redis);
            //var sbj = new AsyncSubject<long>();

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

            sbj.OnNext(5);

            Console.ReadLine();
        }
    }
}
