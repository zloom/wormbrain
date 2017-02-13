using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using wormbrain.client;
using System.Collections.ObjectModel;

namespace wormbrain.ui.ViewModels
{
    public class Main : BaseViewModel, IDisposable
    {       

        private bool _gameStarted;

        private bool _driverStarted;

        private bool _transmitterInjected;

        private ObservableCollection<string> _traceLogs;

        private GameController _controller;               
      
        private BitmapOutput _output;

        private Timer _propertyRefresher;
        
        public WriteableBitmap OutputSource
        {
            get { return _output.Out; }          
        }

        public Func<double> GetLeft { get; set; }

        public Func<double> GetTop { get; set; }

        public string BrainOutput
        {
            get
            {
                return String.Format("Start: {0} Target: {1} Direction: {2} Current: {3} BrainQueue: {4}",
                    _controller.Brain.StartAngle,
                    _controller.Brain.TargetAngle,
                    _controller.Brain.Direction,
                    _controller.Brain.CurrentAngle,
                    _controller.Brain.PacketsCount);
            }
            set { }
        }

        public bool Freeze
        {
            get { return _controller.Brain.Freeze; }
            set
            {
                _controller.Brain.Freeze = value;
                Notify(() => Freeze);
            }
        }

        public bool InputСohesion
        {
            get { return _controller.InputСohesion; }
            set
            {
                _controller.InputСohesion = value;
                Notify(() => InputСohesion);
            }
        }

        public bool OutputСohesion
        {
            get { return _controller.OutputСohesion; }
            set
            {
                _controller.OutputСohesion = value;
                Notify(() => OutputСohesion);
            }
        }

        public bool TransmitterInjected
        {
            get { return _transmitterInjected; }
            set
            {
                _transmitterInjected = value;
                Notify(() => TransmitterInjected);
            }
        }

        public bool DriverStarted
        {
            get { return _driverStarted; }
            set
            {
                _driverStarted = value;
                Notify(() => DriverStarted);
            }
        }

        public bool GameStarted
        {
            get { return _gameStarted; }
            set
            {
                _gameStarted = value;
                Notify(() => GameStarted);
            }
        }

       
        public DelegateCommand StartDriverCommand => new DelegateCommand(StartDriver);
        public DelegateCommand StopDriverCommand => new DelegateCommand(StopDriver);
        public DelegateCommand StartGameCommand => new DelegateCommand(StartGame);
        public DelegateCommand StopGameCommand => new DelegateCommand(StopGame);
        public DelegateCommand StartTestCommand => new DelegateCommand((_) => { });
        public DelegateCommand ShowTraceLogCommand => new DelegateCommand(ShowTraceLog);
        public DelegateCommand ShowBrowserLogCommand => new DelegateCommand(ShowBrowserLog);



        public Main()
        {
            _traceLogs = new ObservableCollection<string>();
            _controller = new GameController();
            _propertyRefresher = new Timer(new TimerCallback(_ => Notify(() => BrainOutput)), null, 1000, 100);
                  
            _controller.DriverClosed += (s, e) => DriverStarted = false;
            _controller.GameInterrupted += (s, e) => GameStarted = false;
            _controller.TransmitterDisconnected += (s, e) => TransmitterInjected = false;

            var listener = new ActionTraceListener(null, msg =>
            {
                _traceLogs.Dispatch(l => l.Insert(0, $"{DateTime.Now:HH:mm:ss.fff} : {msg}"));
            });

            Trace.Listeners.Add(listener);
            _output = new BitmapOutput(_controller.Brain);
        }      

        private void StartDriver(object obj)
        {
            Task.Run(() =>
            {
                _controller.StartDriver();
                DriverStarted = true;
            });
        }

        private void StopDriver(object obj)
        {
            _controller.StopDriver();            
        }
        

        private void StartGame(object obj)
        {
            Task.Run(() =>
            {
                _controller.StartGame();
                GameStarted = true;
            });           
        }

        private void StopGame(object obj)
        {
            _controller.StopGame();            
        }
   

        private void ShowTraceLog(object obj)
        {   
            var logWindow = new Views.Log(new Log(_traceLogs));
            logWindow.Left = (GetLeft?.Invoke() ?? 0) + logWindow.Width;
            logWindow.Top = GetTop?.Invoke() ?? 0;
            logWindow.Show();
        }

        private void ShowBrowserLog(object obj)
        {            
            var logWindow = new Views.Log(new Log(_controller.GetBrowserLogs()));
            logWindow.Left = (GetLeft?.Invoke() ?? 0) + logWindow.Width;
            logWindow.Top = (GetTop?.Invoke() ?? 0) + logWindow.Height;
            logWindow.Show();
        }

        public void Dispose()
        {
            _controller.Dispose();
        }
    }   
}
