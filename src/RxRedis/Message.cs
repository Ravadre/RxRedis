using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxRedis
{
    public class Message<T>
    {
        public T Value { get; set; }
        public MessageType MessageType { get; set; }
        public string Error { get; set; }

        public Message() { }

        public Message(T value, string error, MessageType messageType)
        {
            Value = value;
            MessageType = messageType;
            Error = error;
        }
    }

    public enum MessageType
    {
        Simple = 1,
        Error = 2,
        Completed = 3
    }
}
