using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;

namespace RxRedis
{
    public class RedisSubject<T> : ISubject<T>
    {
        private readonly string redisChannel;
        private readonly IConnectionMultiplexer redisConnection;
        private readonly ISubscriber sub;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly List<IObserver<T>> observers;

        public RedisSubject(string redisChannel, IConnectionMultiplexer redisConnection)
        {
            this.redisChannel = redisChannel;
            this.redisConnection = redisConnection;

            observers = new List<IObserver<T>>();

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
            lock (observers)
            {
                obs = observers.ToArray();
            }

            switch (msg.MessageType)
            {
                case MessageType.Simple:
                {
                    foreach (var o in obs)
                    {
                        o.OnNext(msg.Value);
                    }
                    break;
                }

                case MessageType.Completed:
                {
                    foreach (var o in obs)
                    {
                        o.OnCompleted();
                    }
                    break;
                }

                case MessageType.Error:
                {
                    foreach (var o in obs)
                    {
                        o.OnError(new Exception(msg.Value.ToString()));
                    }
                    break;
                }
            }
        }

        public void OnNext(T value)
        {
            sub.Publish(redisChannel,
                JsonConvert.SerializeObject(
                    new Message<T>(value, MessageType.Simple), jsonSerializerSettings));
        }

        public void OnError(Exception error)
        {
            sub.Publish(redisChannel,
                JsonConvert.SerializeObject(
                    new Message<string>(error.Message, MessageType.Error), jsonSerializerSettings));
        }

        public void OnCompleted()
        {
            sub.Publish(redisChannel,
                JsonConvert.SerializeObject(
                    new Message<Unit>(Unit.Default, MessageType.Completed), jsonSerializerSettings));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (observers)
            {
                observers.Add(observer);
            }

            return Disposable.Create(() =>
            {
                lock (observers)
                {
                    observers.Remove(observer);
                }
            });
        }
    }
}