using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
            });

            Observable.Interval(TimeSpan.FromSeconds(0.5)).Subscribe(idx => sbj.OnNext(idx));
            Console.ReadLine();
        }
    }
}
