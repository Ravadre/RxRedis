using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Jil;
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
                    var msg = JSON.Serialize(value);
                    redisConnection.GetDatabase().StringSet("rxRedis:" + subjectName, msg);
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
                        JSON.Serialize(
                            new Message<T>(default(T), error.Message, MessageType.Error)));
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
                        JSON.Serialize(
                            new Message<T>(default(T), null, MessageType.Completed)));
                }
            }
        }
    }
}
