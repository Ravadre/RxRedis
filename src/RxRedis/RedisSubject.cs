using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jil;
using StackExchange.Redis;

namespace RxRedis
{
    public class RedisSubject<T> : RedisObservable<T>, ISubject<T>
    { 
        public RedisSubject(string subjectName, IConnectionMultiplexer redisConnection)
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

                db.StringSet(RedisKeyState, (int) ValueState.Completed);

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