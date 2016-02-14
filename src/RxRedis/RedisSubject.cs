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
    public class RedisSubject<T> : RedisObservable<T>, ISubject<T>
    { 
        public RedisSubject(string redisChannel, IConnectionMultiplexer redisConnection)
            : base(redisChannel, redisConnection)
        {
           
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
    }
}