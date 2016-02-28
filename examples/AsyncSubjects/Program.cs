using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RxRedis;
using StackExchange.Redis;

namespace AsyncSubjects
{
    class Program
    {
        static void Main(string[] args)
        {
            var complete = new ManualResetEventSlim();
            var pub = new Publisher(complete);
            var sub1 = new Subscriber();
            var sub2 = new Subscriber();

            sub1.Run();
            pub.Run();
            sub2.Run();

            Console.WriteLine("Press enter to mark stream completed");
            Console.ReadLine();
            complete.Set();
            Console.ReadLine();
        }
    }

    class Subscriber
    {
        private readonly IObservable<long> obs;

        public Subscriber()
        {
            var redis = ConnectionMultiplexer.Connect("localhost");
            obs = RedisObservable.Create<long>("async", redis);
        }

        public void Run()
        {
            obs.Subscribe(idx =>
            {
                Console.WriteLine("Subscriber notified with idx: " + idx);
            }, () =>
            {
                Console.WriteLine("Subscriber completed");
            });
        }
    }

    class Publisher
    {
        private readonly ManualResetEventSlim complete;
        private readonly ISubject<long> subject;
         
        public Publisher(ManualResetEventSlim complete)
        {
            this.complete = complete;
            var redis = ConnectionMultiplexer.Connect("localhost");
            subject = new RedisAsyncSubject<long>("async", redis);

        }

        public void Run()
        {
            Task.Run(() =>
            {
                var sub = Observable.Interval(TimeSpan.FromSeconds(1))
                          .Subscribe(idx =>
                          {
                              Console.WriteLine("Published " + idx);
                              subject.OnNext(idx);
                          });
                complete.Wait();
                subject.OnCompleted();
                sub.Dispose();
            });
        }
    }
}
