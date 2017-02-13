using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using wormbrain.client;

namespace wormbrain.client
{
    public class BrainCommand
    {
        public double Delta { get; private set; }

        public double RadInMs { get; private set; }

        public sbyte Direction { get; private set; }

        public BrainCommand(double delta, double speed, sbyte direction)
        {
            if (speed <= 0)           
                throw new ArgumentException("Speed cannot be less than 0.", "speed");
                       
            Delta = delta;
            RadInMs = speed;
            Direction = direction;
        }
    }

    public class WormBrain
    {
        private readonly ConcurrentQueue<Dictionary<short, byte>> Packets;       
        private readonly double _fullRotate = 360.ToRadian();    

        public int MaxX { get; private set; }
        public int MaxY { get; private set; }
        public int MouseRad { get; private set; }
        public double StartAngle  { get; private set; }
        public double CurrentAngle { get; private set; }
        public double TargetAngle { get; private set; }
        public double Direction { get; private set; }       
        public double PacketsCount { get { return Packets.Count; } }
        public Dot Centre { get; private set; }
        public Dot Current { get { return Centre.Rotate(MouseRad, CurrentAngle); } }
        public Dot Start { get { return Centre.Rotate(MouseRad, StartAngle); } }
        public Dot Target { get { return Centre.Rotate(MouseRad, TargetAngle); } }
        public IStrategy Strategy { get; set; }
        public bool Freeze { get; set; }        
        public WormBrain()
        {
            MaxX = 1000;
            MaxY = 1000;
            MouseRad = 300;
            Centre = new Dot(500, 500);

            Packets = new ConcurrentQueue<Dictionary<short, byte>>();           

            StartAngle = 0;
            CurrentAngle = 0;
            TargetAngle = 0;

            Strategy = new Default();
            Task.Run(() => { while (true) PacketsToCommand(); });
           
        }
        public void SetViewPort(Dot viewPort)
        {
            MaxX = viewPort.X;
            MaxY = viewPort.Y;
            MouseRad = (MaxX /100) * 10;
            Centre = new Dot(MaxY / 2, MaxX / 2);
        }

        public void UpdateInput(Dictionary<short, byte> data)
        {
            Packets.Enqueue(data);         
        }
       

        private void PacketsToCommand()
        {
            Dictionary<short, byte> packet;            
            if (Packets.TryDequeue(out packet))
            {
                var cmd = Strategy.NextCommand(packet, CurrentAngle);
                ExecuteCommand(cmd);           
            }
            else
            {
                Thread.Sleep(15);
            }
        }
        private void ExecuteCommand(BrainCommand command)
        {            
            double target = CurrentAngle + (command.Delta * command.Direction);
            var path = CurrentAngle - target;

            TargetAngle = target % _fullRotate;
            CurrentAngle = CurrentAngle % _fullRotate;
            StartAngle = CurrentAngle;
            Direction = command.Direction;

            long freq = TimeSpan.TicksPerMillisecond / 10;           
            double step = (command.RadInMs * command.Direction) / freq;
            uint frames = (uint)Math.Abs(path / step);      

            for (uint frame = 0; frame < frames; frame++)
            {
                while (Freeze) {Thread.Sleep(100);}
                CurrentAngle = CurrentAngle + step;
                Thread.Sleep(TimeSpan.FromTicks(freq));
            }            
        }          

    }
}
