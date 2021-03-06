﻿using System;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using StackExchange.Redis;
using Xunit;

namespace RxRedis.Tests
{
    public class RedisSubjectTests : SubjectTestsCommons
    {
        public RedisSubjectTests()
        {
            rx = new Subject<Data>();
            redisRx = new RedisSubject<Data>("redis-subject-tests", ConnectionMultiplexer.Connect("localhost"));
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

            AssertBuilders(rxResult, redisResult);
        }

        [Fact]
        public void All_values_should_be_received()
        {
            var rxResult = new StringBuilder();
            var redisResult = new StringBuilder();

            Subcribe(rx, rxResult);
            Subcribe(redisRx, redisResult);

            OnNext(new Data("foo", 5));
            OnNext(new Data("foo 2", 57));
            OnNext(new Data("foo 3", 88885));

            AssertBuilders(rxResult, redisResult);
        }

        [Fact]
        public void All_values_before_completed_should_be_received()
        {
            var rxResult = new StringBuilder();
            var redisResult = new StringBuilder();

            Subcribe(rx, rxResult);
            Subcribe(redisRx, redisResult);

            OnNext(new Data("foo", 5));
            OnNext(new Data("foo 2", 57));
            OnCompleted();
            OnNext(new Data("foo 3", 88885));

            AssertBuilders(rxResult, redisResult);
        }

        [Fact]
        public void All_values_before_error_should_be_received()
        {
            var rxResult = new StringBuilder();
            var redisResult = new StringBuilder();

            Subcribe(rx, rxResult);
            Subcribe(redisRx, redisResult);

            OnNext(new Data("foo", 5));
            OnNext(new Data("foo 2", 57));
            OnError(new Exception("test exn"));
            OnNext(new Data("foo 3", 88885));

            AssertBuilders(rxResult, redisResult);
        }

        [Fact]
        public void All_methods_after_first_OnError_should_be_ignored()
        {
            var rxResult = new StringBuilder();
            var redisResult = new StringBuilder();

            Subcribe(rx, rxResult);
            Subcribe(redisRx, redisResult);

            OnNext(new Data("foo", 5));
            OnNext(new Data("foo 2", 57));
            OnError(new Exception("test exn"));
            OnError(new Exception("test exn"));
            OnCompleted();
            OnCompleted();
            OnCompleted();

            AssertBuilders(rxResult, redisResult);
        }

        [Fact]
        public void Subscribing_to_faulted_subject_should_propagate_error()
        {
            var rxResult = new StringBuilder();
            var redisResult = new StringBuilder();

            OnError(new Exception("test exn"));
            OnError(new Exception("test exn"));
            OnCompleted();
            OnCompleted();
            OnCompleted();

            Subcribe(rx, rxResult);
            Subcribe(redisRx, redisResult);

            AssertBuilders(rxResult, redisResult);
        }
    }
}