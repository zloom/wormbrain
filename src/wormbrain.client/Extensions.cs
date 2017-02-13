using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wormbrain.client
{
    public static class Extensions
    {       

        public static Dot Rotate(this Dot centre, double r, double angle)
        {
            var x = (int)Math.Round(Math.Cos(angle) * r + centre.X);
            var y = (int)Math.Round(Math.Sin(angle) * r + centre.Y);
            return new Dot(x, y);
        }

        public static double ToRadian(this double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static double ToRadian(this int angle)
        {
            return (Math.PI / 180) * angle;
        }

        public static int ToMs(this int seconds)
        {
            return (int)TimeSpan.FromSeconds(7).TotalMilliseconds;
        }

        private static double RandomNumberBetween(this Random random, double maxValue)
        {
            var next = random.NextDouble();

            return (next * (maxValue));
        }
    }
}
