using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RxRedis
{
    public class RedisAsyncSubject<T> : RedisObservable<T>, ISubject<T>
    {
        public RedisAsyncSubject(string subjectName, IConnectionMultiplexer redisConnection)
            : base(subjectName, redisConnection)
        {

        }

        public void OnNext(T value)
        {
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {

                    sub.Publish(subjectName,
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
                    sub.Publish(subjectName,
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
                    sub.Publish(subjectName,
                        JsonConvert.SerializeObject(
                            new Message<T>(default(T), null, MessageType.Completed), jsonSerializerSettings));
                }
            }
        }
    }
}
