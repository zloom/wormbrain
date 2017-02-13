using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wormbrain.client
{
    public class EventArgs<T> : EventArgs
    {
        public T Payload { get; private set; }

        public EventArgs(T payload)
        {
            Payload = payload;
        }
    }
}
