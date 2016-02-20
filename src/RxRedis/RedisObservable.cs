using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Jil;
using StackExchange.Redis;

namespace RxRedis
{
    public static class RedisObservable
    {
        public static IObservable<T> Create<T>(string subjectName, IConnectionMultiplexer redisConnection)
        {
            return new RedisObservable<T>(subjectName, redisConnection);
        } 
    }

    public class RedisObservable<T> : IObservable<T>, IDisposable
    {
        protected readonly string subjectName;
        protected readonly IConnectionMultiplexer redisConnection;
        protected readonly ISubscriber sub;
        protected readonly List<IObserver<T>> observers;
        protected readonly object gate;
        protected bool isStopped;
        protected bool isDisposed;

        internal RedisObservable(string subjectName, IConnectionMultiplexer redisConnection)
        {
            this.subjectName = subjectName;
            this.redisConnection = redisConnection;

            observers = new List<IObserver<T>>();
            gate = new object();

            sub = this.redisConnection.GetSubscriber();

            sub.Subscribe(subjectName, OnMessage);
        }

        private void OnMessage(RedisChannel redisChannel, RedisValue redisValue)
        {
            var msg = JSON.Deserialize<Message<T>>(redisValue);

            IObserver<T>[] obs;
            lock (gate)
            {
                obs = observers.ToArray();
            }

            switch (msg.MessageType)
            {
                case MessageType.Simple:
                    {
                        foreach (var o in obs)
                        {
                            try
                            {
                                o.OnNext(msg.Value);
                            }
                            catch
                            {
                                Unsubscribe(o);
                            }
                        }
                        break;
                    }
                case MessageType.Completed:
                    {
                        foreach (var o in obs)
                        {
                            try
                            {
                                o.OnCompleted();
                            }
                            catch
                            {
                                Unsubscribe(o);
                            }
                        }
                        break;
                    }
                case MessageType.Error:
                    {
                        foreach (var o in obs)
                        {
                            try
                            {
                                o.OnError(new Exception(msg.Error));
                            }
                            catch
                            {
                                Unsubscribe(o);
                            }
                        }
                        break;
                    }
            }
        }
        
        private void Unsubscribe(IObserver<T> observer)
        {
            lock (gate)
            {
                observers.Remove(observer);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    observers.Add(observer);
                }
            }

            return Disposable.Create(() =>
            {
                lock (gate)
                {
                    Unsubscribe(observer);
                }
            });
        }

        protected void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(string.Empty);
        }

        public void Dispose()
        {
            lock (gate)
            {
                isDisposed = true;
                observers.Clear();
            }
        }
    }
}
