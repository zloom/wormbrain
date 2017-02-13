using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WebSocketSharp.Server;
using wormbrain.client;
using WebSocketSharp;
using Newtonsoft.Json.Linq;

namespace wormbrain.client
{

    internal class Behavior : WebSocketBehavior
    {
        private readonly Action<MessageEventArgs> _onMessage;
        private readonly Action<CloseEventArgs> _onClose;
        private readonly Action<WebSocketSharp.ErrorEventArgs> _onError;

        public Behavior(Action<MessageEventArgs> onMessage, Action<CloseEventArgs> onClose, Action<WebSocketSharp.ErrorEventArgs> onError)
        {
            _onMessage = onMessage;
            _onClose = onClose;
            _onError = onError;
        }


        protected override void OnOpen()
        {
            Trace.TraceInformation("Transmitter connected.");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            _onMessage?.Invoke(e);           
        }

        protected override void OnClose(CloseEventArgs e)
        {
            _onClose?.Invoke(e);
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            _onError?.Invoke(e);
        }
    }
    public class GameController: IDisposable
    {          

        private readonly Timer DriverTimer;

        private readonly Timer GameTimer;       

        private ChromeDriver _driver;

        private WebSocketServer _wss;

        private IWebElement _loginButton;

        private IWebElement _gameField;

        public readonly WormBrain Brain; 

        public event EventHandler<EventArgs> DriverClosed;

        public event EventHandler<EventArgs> GameInterrupted;

        public event EventHandler<EventArgs> TransmitterDisconnected;

        public bool OutputСohesion { get; set; }

        public bool InputСohesion { get; set; }

        public GameController()
        {           
            Brain = new WormBrain();
            _wss = new WebSocketServer("ws://localhost:3535");
            _wss.AddWebSocketService("/", () => new Behavior(OnMessage, e => TransmitterDisconnected?.Invoke(this, EventArgs.Empty), e => TransmitterDisconnected?.Invoke(this, EventArgs.Empty)));
            _wss.Start();


            DriverTimer = new Timer(100);
            DriverTimer.Elapsed += (s, e) =>
            {
                if (_driver.SessionId == null)
                {
                    DriverTimer.Stop();
                    DriverClosed?.Invoke(this, EventArgs.Empty);
                };
            };

            GameTimer = new Timer(1000);
            GameTimer.Elapsed += (s, e) =>
            {
                var gameStarted = false;
                try
                {
                    gameStarted = !(_loginButton.Displayed && _loginButton.Enabled);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Game status error {0}", ex.ToString());
                }

                if (!gameStarted)
                {
                    GameTimer.Stop();
                    GameInterrupted?.Invoke(this, EventArgs.Empty);
                }                
            };
                     
            Task.Run(() => { while (true) BrainToGame(); });
        }
        public void StartDriver()
        {
            try
            {
                var chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = false;
                var options = new ChromeOptions();
                options.SetLoggingPreference(LogType.Browser, OpenQA.Selenium.LogLevel.All);

                _driver = new ChromeDriver(chromeDriverService, options);
                Trace.TraceInformation("Driver created sid {0}", _driver.SessionId);

                _driver.Navigate().GoToUrl("http://slither.io");

                var viewPort = GetViewPort();
                Trace.TraceInformation("Driver view port X:{0} Y:{1}", viewPort.X, viewPort.Y);             

                Brain.SetViewPort(viewPort);                
            }
            catch (Exception ex)
            {
                Trace.TraceError("Start driver error {0}", ex.ToString());
            }
            finally
            {
                DriverTimer.Start();
            }
        }

        public void StartGame()
        {
            try
            {                
                _loginButton = _driver?.FindElement(By.Id("playh"));
                _gameField = _driver?.FindElement(By.Id("nbg"));
                if (_loginButton != null && _loginButton.Displayed && _loginButton.Enabled)
                {
                    _loginButton.Click();                 
                }
                
                _driver.ExecuteScript(File.ReadAllText("Injection.js"));
            }
            catch (Exception ex)
            {
                Trace.TraceError("Start game error {0}", ex.ToString());
            }
            finally
            {
                GameTimer.Start();
            }            
        }

        public void StopGame()
        {
            _driver?.Navigate().Refresh();           
        }

        public void StopDriver()
        {
            _driver.Quit();
        }

       

        public IEnumerable<string> GetBrowserLogs()
        {
            try
            {
                return _driver.Manage().Logs.GetLog(LogType.Browser).Select(l => l.Message);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Get browser logs error {0}", ex.ToString());
                return new List<string>();
            }
        }
            
    

        private void OnMessage(MessageEventArgs e)
        {
            dynamic d = JObject.Parse(e.Data);          
            if (d.type == 0)
            {
                return;
            }

            if (InputСohesion && d.type == 1)
            {
                string payload = d.data.ToString();
                var data = JsonConvert.DeserializeObject<Dictionary<short, byte>>(payload);
                Brain.UpdateInput(data);                               
            }
        }

        private void BrainToGame()
        {
            if (OutputСohesion && _driver != null)
            {
                try
                {
                    new Actions(_driver).MoveToElement(_gameField, Brain.Current.X, Brain.Current.Y).Build().Perform();
                }
                catch (Exception e)
                {
                    Trace.TraceError("Brain output error {0}", e.ToString());
                }
            }
            else
            {
                System.Threading.Thread.Sleep(15);
            }
        }       

        private Dot GetViewPort()
        {
            var viewPort = (Dictionary<string, object>)_driver.ExecuteScript("return {w: document.documentElement.clientWidth, h: document.documentElement.clientHeight};");
            int w = (int)(long)viewPort["w"];
            int h = (int)(long)viewPort["h"];
            return new Dot(h, w);
        }

        private string GetHost()
        {
            return _driver.ExecuteScript("return window.bso.ip;").ToString();
        }    

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();            
        }
               
    }
}
