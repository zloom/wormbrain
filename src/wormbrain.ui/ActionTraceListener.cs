using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wormbrain.ui
{
    public class ActionTraceListener : TraceListener
    {
        private readonly Action<string> WriteAction;

        private readonly Action<string> WriteLineAction;
        public ActionTraceListener(Action<string> write, Action<string> writeLine)
        {
            WriteAction = write;
            WriteLineAction = writeLine;
        }
        public override void Write(string message)
        {
            WriteAction?.Invoke(message);
        }

        public override void WriteLine(string message)
        {
            WriteLineAction?.Invoke(message);
        }
    }
}
