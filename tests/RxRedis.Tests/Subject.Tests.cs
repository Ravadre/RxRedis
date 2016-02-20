using System;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using StackExchange.Redis;
using Xunit;

namespace RxRedis.Tests
{
    public class RedisSubjectTests
    {
        private RedisSubject<Data> redisRx;
        private Subject<Data> rx;

        public RedisSubjectTests()
        {
            rx = new Subject<Data>();
            redisRx = new RedisSubject<Data>("redis-subject-tests", ConnectionMultiplexer.Connect("localhost"));
        }

        private IDisposable Subcribe(ISubject<Data> subject, StringBuilder buffer)
        {
            return subject.Subscribe(d => { buffer.AppendLine($"OnNext: {d.Foo} {d.Bar}"); },
                error => { buffer.AppendLine($"OnError: {error.Message}"); },
                () => buffer.AppendLine("OnCompleted"));
        }

        private void OnCompleted()
        {
            redisRx.OnCompleted();
            rx.OnCompleted();
        }

        private void OnNext(Data data)
        {
            redisRx.OnNext(data);
            rx.OnNext(data);
        }

        private void OnError(Exception exn)
        {
            redisRx.OnError(exn);
            rx.OnError(exn);
        }

        private void AssertBuilders(StringBuilder expected, StringBuilder actual)
        {
            var i = 2;
            while (--i >= 0)
            {
                try
                {
                    Assert.Equal(expected.ToString(), actual.ToString());
                    return;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }

            Assert.Equal(expected.ToString(), actual.ToString());
        }

        [Fact]
        public void Subscribe_after_completed_should_behave_like_rx()
        {
            var rxResult = new StringBuilder();
            var redisResult = new StringBuilder();

            OnCompleted();

            Subcribe(rx, rxResult);
            Subcribe(redisRx, redisResult);

            AssertBuilders(rxResult, redisResult);
        }

        [Fact]
        public void Completed_after_subscribe_should_behave_like_rx()
        {
            var rxResult = new StringBuilder();
            var redisResult = new StringBuilder();

            Subcribe(rx, rxResult);
            Subcribe(redisRx, redisResult);

            OnCompleted();
            Thread.Sleep(100);

            AssertBuilders(rxResult, redisResult);
        }
    }

    public class Data
    {
        public string Foo { get; set; }
        public long Bar { get; set; }
    }
}