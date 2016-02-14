using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;

namespace RxRedis
{
    public class RedisSubject<T> : ISubject<T>, IDisposable
    {
        private readonly string redisChannel;
        private readonly IConnectionMultiplexer redisConnection;
        private readonly ISubscriber sub;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly List<IObserver<T>> observers;
        private readonly object gate;
        private bool isStopped;
        private bool isDisposed;

        public RedisSubject(string redisChannel, IConnectionMultiplexer redisConnection)
        {
            this.redisChannel = redisChannel;
            this.redisConnection = redisConnection;

            observers = new List<IObserver<T>>();
            gate = new object();

            sub = this.redisConnection.GetSubscriber();

            jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            sub.Subscribe(redisChannel, OnMessage);
        }

        private void OnMessage(RedisChannel redisChannel, RedisValue redisValue)
        {
            var msg = JsonConvert.DeserializeObject<Message<T>>(redisValue);

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
                            o.OnError(new Exception(msg.Value.ToString()));
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

        public void OnNext(T value)
        {
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    sub.Publish(redisChannel,
                        JsonConvert.SerializeObject(
                            new Message<T>(value, null, MessageType.Simple), jsonSerializerSettings));
                }
            }
        }

        public void OnError(Exception error)
        {
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    isStopped = true;
                    sub.Publish(redisChannel,
                        JsonConvert.SerializeObject(
                            new Message<T>(default(T), error.Message, MessageType.Error), jsonSerializerSettings));
                }
            }
        }

        public void OnCompleted()
        {
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    isStopped = true;
                    sub.Publish(redisChannel,
                        JsonConvert.SerializeObject(
                            new Message<T>(default(T), null, MessageType.Completed), jsonSerializerSettings));
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

        private void CheckDisposed()
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