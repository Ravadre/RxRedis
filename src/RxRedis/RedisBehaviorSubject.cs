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
            T data)
            : base(subjectName, redisConnection)
        {
            db.KeyDelete(RedisKeyState);
            db.KeyDelete(RedisKeyValue);

            SetValueAsPublished(data);
        }

        private void SetValueAsPublished(T value)
        {
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    var msg = JSON.Serialize(value);
                    db.StringSet(RedisKeyValue, msg);
                    db.StringSet(RedisKeyState, (int)ValueState.Published);
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
                    var msg = JSON.Serialize(value);
                    db.StringSet(RedisKeyValue, msg);
                    db.StringSet(RedisKeyState, (int)ValueState.Published);

                    sub.Publish(subjectName,
                      JSON.Serialize(
                          new Message<T>(value, null, MessageType.Value)));
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

                    db.StringSet(RedisKeyValue, error.Message);
                    db.StringSet(RedisKeyState, (int)ValueState.Error);

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
                    db.StringSet(RedisKeyState, (int)(ValueState.Completed | ValueState.Unpublished));

                    sub.Publish(subjectName,
                        JSON.Serialize(
                            new Message<T>(default(T), null, MessageType.Completed)));
                }
            }
        }
    }
}