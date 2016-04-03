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
        protected readonly IDatabase db;
        protected readonly ISubscriber sub;
        protected readonly List<IObserver<T>> observers;
        protected readonly object gate;
        protected bool isStopped;
        protected bool isDisposed;

        protected string RedisKeyValue => "rxRedis:" + subjectName + ":value";
        protected string RedisKeyState => "rxRedis:" + subjectName + ":state";

        protected ValueState RedisParseState()
        {
            var v = db.StringGet(RedisKeyState);
            return v.HasValue ? (ValueState)(int)v : 0;
        }

        internal RedisObservable(string subjectName, IConnectionMultiplexer redisConnection)
        {
            this.subjectName = subjectName;
            this.redisConnection = redisConnection;
            this.db = redisConnection.GetDatabase();

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
                case MessageType.Value:
                    {
                        foreach (var o in obs)
                        {
                            try { o.OnNext(msg.Value); }
                            catch { Unsubscribe(o); }
                        }
                        break;
                    }
                case MessageType.Completed:
                    {
                        foreach (var o in obs)
                        {
                            EmitIfPublished(o, RedisParseState());
                            try { o.OnCompleted(); }
                            catch { Unsubscribe(o); }
                        }
                        break;
                    }
                case MessageType.Error:
                    {
                        foreach (var o in obs)
                        {
                            try { o.OnError(new Exception(msg.Error)); }
                            catch { Unsubscribe(o); }
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

        private void EmitIfPublished(IObserver<T> o, ValueState state)
        {
            if ((state & ValueState.Published) != 0)
            {
                var val = db.StringGet(RedisKeyValue);

                if (val.HasValue)
                {
                    var msg = JSON.Deserialize<T>(val);
                    try { o.OnNext(msg); }
                    catch { Unsubscribe(o); }
                }
            }
        }

        private void EmitIfCompleted(IObserver<T> o, ValueState state)
        {
            if ((state & ValueState.Completed) != 0)
            {
                try { o.OnCompleted(); }
                catch { Unsubscribe(o); }
            }
        }


        private void EmitIfError(IObserver<T> o, ValueState state)
        {
            if ((state & ValueState.Error) != 0)
            {
                var val = db.StringGet(RedisKeyValue);

                if (val.HasValue)
                {
                    try { o.OnError(new Exception(val)); }
                    catch { Unsubscribe(o); }
                }
            }
        }


        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (gate)
            {
                CheckDisposed();

                var state = RedisParseState();
                EmitIfPublished(observer, state);
                EmitIfCompleted(observer, state);
                EmitIfError(observer, state);

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
