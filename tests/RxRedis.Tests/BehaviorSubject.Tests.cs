using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Xunit;

namespace RxRedis.Tests
{
    public class RedisBehaviorSubjectTests : SubjectTestsCommons
    {
        public RedisBehaviorSubjectTests()
        {
            var startData = new Data("start", 0);
            rx = new BehaviorSubject<Data>(startData);
            redisRx = new RedisBehaviorSubject<Data>("redis-behavior-subject-tests", ConnectionMultiplexer.Connect("localhost"),
                startData);
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
//
//        [Fact]
//        public void Completed_after_subscribe_should_behave_like_rx()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            OnCompleted();
//
//            AssertBuilders(rxResult, redisResult);
//        }
//
//        [Fact]
//        public void Non_completed_subject_should_not_emit_any_values()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            OnNext(new Data("foo", 5));
//            OnNext(new Data("foo 2", 57));
//            OnNext(new Data("foo 3", 88885));
//
//            AssertBuilders(rxResult, redisResult);
//        }
//
//        [Fact]
//        public void Last_value_before_completed_should_be_emitted()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            OnNext(new Data("foo", 5));
//            OnNext(new Data("foo 2", 57));
//            OnCompleted();
//            OnNext(new Data("foo 3", 88885));
//
//            AssertBuilders(rxResult, redisResult);
//        }
//
//        [Fact]
//        public void No_values_should_be_received_when_error_occured()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            OnNext(new Data("foo", 5));
//            OnNext(new Data("foo 2", 57));
//            OnError(new Exception("test exn"));
//            OnNext(new Data("foo 3", 88885));
//
//            AssertBuilders(rxResult, redisResult);
//        }
//
//        [Fact]
//        public void No_values_should_be_received_when_error_occured_even_if_completed_was_signaled_after()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            OnNext(new Data("foo", 5));
//            OnNext(new Data("foo 2", 57));
//            OnError(new Exception("test exn"));
//            OnNext(new Data("foo 3", 88885));
//            OnCompleted();
//
//            AssertBuilders(rxResult, redisResult);
//        }
//        [Fact]
//        public void All_methods_after_first_OnError_should_be_ignored()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            OnNext(new Data("foo", 5));
//            OnNext(new Data("foo 2", 57));
//            OnError(new Exception("test exn"));
//            OnError(new Exception("test exn"));
//            OnCompleted();
//            OnCompleted();
//            OnCompleted();
//
//            AssertBuilders(rxResult, redisResult);
//        }
//
//        [Fact]
//        public void Subscribing_to_faulted_subject_should_propagate_error()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            OnError(new Exception("test exn"));
//            OnError(new Exception("test exn"));
//            OnCompleted();
//            OnCompleted();
//            OnCompleted();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            AssertBuilders(rxResult, redisResult);
//        }
//
//
//        [Fact]
//        public void Subscribing_to_completed_subject_with_value_should_propagate_this_value()
//        {
//            var rxResult = new StringBuilder();
//            var redisResult = new StringBuilder();
//
//            OnNext(new Data("foo 2", 57));
//            OnCompleted();
//            OnCompleted();
//
//            Subcribe(rx, rxResult);
//            Subcribe(redisRx, redisResult);
//
//            AssertBuilders(rxResult, redisResult);
//        }
    }
}
