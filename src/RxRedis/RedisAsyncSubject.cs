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
            db.KeyDelete(RedisKeyState);
            db.KeyDelete(RedisKeyValue);
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
                    db.StringSet(RedisKeyState, (int)ValueState.Unpublished);
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
                    var state = RedisParseState();
                    var newState = ValueState.Completed;
                    if ((state & ValueState.Unpublished) > 0)
                        newState |= ValueState.Published;
                    db.StringSet(RedisKeyState, (int) newState);

                    isStopped = true;
                    sub.Publish(subjectName,
                        JSON.Serialize(
                            new Message<T>(default(T), null, MessageType.Completed)));
                }
            }
        }
    }
}
