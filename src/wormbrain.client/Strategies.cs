using System;
using System.Collections.Generic;

namespace wormbrain.client
{
    public interface IStrategy
    {
        BrainCommand NextCommand(Dictionary<short, byte> data, double currentAngle);
    }

    public class Default : IStrategy
    {
        private Random _random;
        public Default()
        {
            _random = new Random();          
        }
        public BrainCommand NextCommand(Dictionary<short, byte> data, double currentAngle)
        {         
            double speed = _random.Next(1, 10) / 100.0;
            int angle = _random.Next(0, 360);
            var direction = _random.Next(100) > 50 ? -1 : 1;
            return new BrainCommand(angle.ToRadian(), speed.ToRadian(), (sbyte)direction);
        }
    }

    public class Crawl : IStrategy
    {
        private readonly double _deviation;
        private readonly double _mutation;        
        private readonly Random _random;
        private sbyte _direction;
        public Crawl(double deviation, double mutation)
        {
            _direction = -1;
            _deviation = deviation;
            _mutation = mutation;
            _random = new Random();
        }
        public BrainCommand NextCommand(Dictionary<short, byte> data, double currentAngle)
        {
            _direction = (sbyte)(_direction * -1);
            var delta = (_random.NextDouble() * _mutation) + _deviation;
            var speed = 0.05.ToRadian();
            return new BrainCommand(delta, speed, _direction);
        }
    }
}
