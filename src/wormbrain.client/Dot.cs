using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wormbrain.client
{
    public class Dot
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Dot(int x, int y)
        {
            X = x;
            Y = y;
        }

      
    }
}
