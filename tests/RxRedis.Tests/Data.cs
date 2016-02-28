using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxRedis.Tests
{
    public class Data
    {
        public string Foo { get; set; }
        public long Bar { get; set; }

        public Data()
        {

        }

        public Data(string foo, long bar)
        {
            Foo = foo;
            Bar = bar;
        }
    }
}
