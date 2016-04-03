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
    public class RedisBehaviorSubject<T> : RedisObservable<T>, ISubject<T>
    {
        public RedisBehaviorSubject(string subjectName, IConnectionMultiplexer redisConnection,
            T originalData)
            : base(subjectName, redisConnection)
        {
            db.KeyDelete(RedisKeyState);
            db.KeyDelete(RedisKeyValue);
        }

        public void OnNext(T value)
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    isStopped = true;
                    db.StringSet(RedisKeyState, (int)(ValueState.Completed | ValueState.Published));

                    sub.Publish(subjectName,
                        JSON.Serialize(
                            new Message<T>(default(T), null, MessageType.Completed)));
                }
            }
        }
    }
}