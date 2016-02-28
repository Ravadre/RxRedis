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
            var preSub = new Subscriber("Pre complete subscriber");
            var postSub = new Subscriber("Post complete subscriber");

            preSub.Run();
            pub.Run();
           

            Console.WriteLine("Press enter to mark stream completed");
            Console.ReadLine();
            complete.Set();
            Thread.Sleep(1000);
            Console.WriteLine();
            Console.WriteLine("Press enter to subscribe with another subscriber");
            Console.ReadLine();
            postSub.Run();
            Console.ReadLine();
        }
    }

    class Subscriber
    {
        private readonly string name;
        private readonly IObservable<long> obs;

        public Subscriber(string name)
        {
            this.name = name;
            var redis = ConnectionMultiplexer.Connect("localhost");
            obs = RedisObservable.Create<long>("async", redis);
        }

        public void Run()
        {
            obs.Subscribe(idx =>
            {
                Console.WriteLine(name + " notified with idx: " + idx);
            }, () =>
            {
                Console.WriteLine(name + " completed");
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
