using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RxRedis;
using StackExchange.Redis;

namespace TwoWayCommunication
{
    class Program
    {
        static void Main(string[] args)
        {
            var sink = new Sink();
            var client = new Client();
            var service = new Service();

            sink.Run();
            client.Run();
            service.Run();

            Console.ReadKey();
        }
    }

    class Sink
    {
        private readonly IObservable<string> commands;
        private readonly IObservable<string> events; 

        public Sink()
        {
            var redis = ConnectionMultiplexer.Connect("localhost");
            commands = RedisObservable.Create<string>("commands", redis);
            events = RedisObservable.Create<string>("events", redis);
        }

        public void Run()
        {
            Task.Run(() =>
            {
                commands.Subscribe(cmd => Console.WriteLine("\tSink received command: " + cmd));
                events.Subscribe(ev => Console.WriteLine("\tSink received event: " + ev));
            });
        }
        
    }

    class Client
    {
        private readonly ISubject<string> eventsSubject;
        private readonly IObservable<string> commands;
        private readonly Random rng;

        public Client()
        {
            var redis = ConnectionMultiplexer.Connect("localhost");
            eventsSubject = new RedisSubject<string>("events", redis);
            commands = RedisObservable.Create<string>("commands", redis);

            rng = new Random();
        }

        public void Run()
        {
            Task.Run(() =>
            {
                var events = new[]
                {
                    "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta",
                    "theta", "iota", "kappa", "lambda", "mu", "nu", "xi", "omicron",
                    "pi", "rho", "sigma", "tau", "upsilon", "phi", "chi", "psi", "omega"
                };

                commands.Subscribe(ev => Console.WriteLine("Client received command: " + ev));

                while (true)
                {
                    eventsSubject.OnNext(events[rng.Next(events.Length)]);
                    Thread.Sleep(500);
                }
            });
        }
    }

    class Service
    {
        private readonly ISubject<string> commandSubject;
        private readonly IObservable<string> events;

        public Service()
        {
            var redis = ConnectionMultiplexer.Connect("localhost");
            commandSubject = new RedisSubject<string>("commands", redis);
            events = RedisObservable.Create<string>("events", redis);
        }

        public void Run()
        {
            Task.Run(() =>
            {
                events.Subscribe(ev =>
                {
                    if (ev == "beta")
                    {
                        commandSubject.OnNext("Beta command");
                    }
                });
            });
        }
    }
}
