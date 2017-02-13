using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wormbrain.client
{
    public class BitmapOutput
    {
        private Timer _drawTimer;
        private WormBrain _brain;
        public WriteableBitmap Out { get; private set; }        
        public BitmapOutput(WormBrain brain)
        {
            _brain = brain;
            Out = BitmapFactory.New(_brain.MaxY, _brain.MaxX);
            _drawTimer = new Timer(20);
            _drawTimer.Elapsed += Draw;
            _drawTimer.Start();
        }

        private void Draw(object sender, ElapsedEventArgs e)
        { 
            Do(b => b.Clear(Brushes.LightGray.Color));
            Do(b => b.DrawEllipseCentered(_brain.Centre.X, _brain.Centre.Y, _brain.MouseRad, _brain.MouseRad, Brushes.Azure.Color));
            Do(b => b.DrawLine(_brain.Centre.X, _brain.Centre.Y, _brain.Start.X, _brain.Start.Y, Brushes.Green.Color));
            Do(b => b.DrawLine(_brain.Centre.X, _brain.Centre.Y, _brain.Current.X, _brain.Current.Y, Brushes.Red.Color));
            Do(b => b.DrawLine(_brain.Centre.X, _brain.Centre.Y, _brain.Target.X, _brain.Target.Y, Brushes.Blue.Color));           
        }

        private void Do(Action<WriteableBitmap> action)
        {
            try
            {
                Out.Dispatcher.Invoke(() => action(Out));
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Error {0}", e.ToString());
            }
        }
    }
}
