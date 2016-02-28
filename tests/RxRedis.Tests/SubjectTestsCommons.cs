using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RxRedis.Tests
{
    public class SubjectTestsCommons
    {
        protected ISubject<Data> redisRx;
        protected ISubject<Data> rx;
        
        protected IDisposable Subcribe(ISubject<Data> subject, StringBuilder buffer)
        {
            return subject.Subscribe(d => { buffer.AppendLine($"OnNext: {d.Foo} {d.Bar}"); },
                error => { buffer.AppendLine($"OnError: {error.Message}"); },
                () => buffer.AppendLine("OnCompleted"));
        }

        protected void OnCompleted()
        {
            redisRx.OnCompleted();
            rx.OnCompleted();
        }

        protected void OnNext(Data data)
        {
            redisRx.OnNext(data);
            rx.OnNext(data);
        }

        protected void OnError(Exception exn)
        {
            redisRx.OnError(exn);
            rx.OnError(exn);
        }

        protected void AssertBuilders(StringBuilder expected, StringBuilder actual)
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
    }
}
