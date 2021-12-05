using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using InterProcessRenderer.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InterProcessRenderer
{
    class Program
    {
        PipeProcessor mPipeProcessor;
        private System.Timers.Timer _Timer = null;
        private bool _close = true;
        private CaptureManager _CaptureManager = null;
        private object _CaptureManagerLock = new object();
        private EVROutputAdapter _EVROutputAdapter = null;
        private IEVRStreamControl _IEVRStreamControl = null;

        public CaptureManager CaptureManager
        {
            get
            {
                lock (_CaptureManagerLock)
                {
                    if (_CaptureManager == null)
                    {

                        try
                        {
                            _CaptureManager = new CaptureManager("CaptureManager.dll");
                        }
                        catch (System.Exception exc)
                        {
                            try
                            {
                                _CaptureManager = new CaptureManager();
                            }
                            catch (System.Exception exc1)
                            {

                            }
                        }
                    }
                }

                return _CaptureManager;
            }
        }

        private System.Timers.Timer Timer
        {
            get
            {
                if (_Timer == null)
                {
                    _Timer = new System.Timers.Timer(4000);
                    _Timer.Elapsed += _Timer_Elapsed;
                }

                return _Timer;
            }
        }

        void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_close)
                System.Diagnostics.Process.GetCurrentProcess().Kill();

            _close = true;
        }


        private Program() { }

        private static Program _Instance = null;

        public static Program Instance { get { if (_Instance == null) _Instance = new Program(); return _Instance; } }

        private ISession _Session = null;

        private string _SymbolicLink = "";

        private uint _StreamIndex = 0;

        private uint _MediaTypeIndex = 0;

        private Guid _ConnectionSignature = Guid.Empty;

        public uint FrameHeight = 0;

        public uint FrameWidth = 0;

        private IntPtr _HWND = IntPtr.Zero;

        private AutoResetEvent mStartEvent = new AutoResetEvent(false);

        private AutoResetEvent mCloseEvent = new AutoResetEvent(false);

        private ISession Session
        {
            get { return _Session; }
            set
            {
                if (_Session != value)
                {
                    if (_Session != null)
                    {
                        _Session.stopSession();
                        _Session.closeSession();
                        //  Panel.Invalidate();
                    }

                    _Session = value;
                }
            }
        }

        [System.MTAThread]
        static void Main(string[] args)
        {            
            Instance.Timer.Start();
            
            foreach (var item in args)
            {
                if (item.Contains("SymbolicLink="))
                {
                    Instance._SymbolicLink = item.Replace("SymbolicLink=", "");
                }
                else if (item.Contains("StreamIndex="))
                {
                    Instance._StreamIndex = uint.Parse(item.Replace("StreamIndex=", ""));
                }
                else if (item.Contains("MediaTypeIndex="))
                {
                    Instance._MediaTypeIndex = uint.Parse(item.Replace("MediaTypeIndex=", ""));
                }
                else if (item.Contains("ConnectionSignature="))
                {
                    Instance._ConnectionSignature = Guid.Parse(item.Replace("ConnectionSignature=", ""));
                }
                else if (item.Contains("WindowHandler="))
                {
                    Instance._HWND = new IntPtr(int.Parse(item.Replace("WindowHandler=", "")));
                }
                else if (item.Contains("FrameHeight="))
                {
                    Instance.FrameHeight = uint.Parse(item.Replace("FrameHeight=", ""));
                }
                else if (item.Contains("FrameWidth="))
                {
                    Instance.FrameWidth = uint.Parse(item.Replace("FrameWidth=", ""));
                }
                else if (item.Contains("ConnectionCount="))
                {
                    Instance.FrameWidth = uint.Parse(item.Replace("ConnectionCount=", ""));
                }


                
            }

            Instance.start();
        }

        public void start()
        {
            mPipeProcessor = new PipeProcessor(
            "Client"+ _ConnectionSignature,
            "Server"+ _ConnectionSignature);

            _IEVRStreamControl = CaptureManager.createEVRStreamControl();

            mPipeProcessor.MessageDelegateEvent += mPipeProcessor_MessageDelegateEvent;

            mPipeProcessor.send("Initilized");

            mStartEvent.WaitOne();

            StartSession();

            mCloseEvent.WaitOne();

            if (Session != null)
            {
                Session.stopSession();

                Session.closeSession();
            }

            mPipeProcessor.closeConnection();
        }

        void mPipeProcessor_MessageDelegateEvent(string aMessage)
        {
            switch (aMessage)
            {
                case "Start":
                    mStartEvent.Set();
                    break;

                case "Close":
                    mCloseEvent.Set();
                    break;

                case "HeartBeat":
                    _close = false;
                    break;

                case "Get_EVRStreamFilters":
                    getEVRStreamFilters();
                    break;

                case "Get_EVRStreamOutputFeatures":
                    getEVRStreamOutputFeatures();
                    break;

                    

                default:
                    break;
            }


            if (aMessage.Contains("Set_EVRStreamOutputFeatures="))
            {
                var lparts = aMessage.Replace("Set_EVRStreamOutputFeatures=", "").Split(new char[] { '_' });

                if(lparts != null && lparts.Length == 2)
                {
                    uint lIndex = uint.Parse(lparts[0]);

                    int lvalue = int.Parse(lparts[1]);

                    setEVRStreamOutputFeatures(lIndex, lvalue);
                }
            }


            if (aMessage.Contains("Set_EVRStreamFilters="))
            {
                var lparts = aMessage.Replace("Set_EVRStreamFilters=", "").Split(new char[] { '_' });

                if (lparts != null && lparts.Length == 3)
                {
                    uint lIndex = uint.Parse(lparts[0]);

                    int lvalue = int.Parse(lparts[1]);

                    bool lIsEnable = bool.Parse(lparts[2]);

                    setFilterParametr(lIndex, lvalue, lIsEnable);
                }
            }

        }
        
        private void StartSession()
        {
            object lPtrSourceNode = null;
            ISourceControl sourceControl = CaptureManager.createSourceControl();
            ISessionControl sessionControl = CaptureManager.createSessionControl();

            _EVROutputAdapter = new EVROutputAdapter(Instance, _HWND);

            if (sessionControl != null && sessionControl != null && _EVROutputAdapter.Node != null)
            {
                Session = null;

                sourceControl.createSourceNode(_SymbolicLink,
                                               _StreamIndex,
                                               _MediaTypeIndex,
                                               _EVROutputAdapter.Node,
                                               out lPtrSourceNode);

                Session = sessionControl.createSession(new object[]
                                                       {
                                                           lPtrSourceNode
                                                       });

                if (Session != null)
                {
                    Session.registerUpdateStateDelegate(HandleSessionStateChanged);
                    Session.startSession(0, Guid.Empty);
                }
            }
        }

        void HandleSessionStateChanged(uint aCallbackEventCode, uint aSessionDescriptor)
        {
            SessionCallbackEventCode k = (SessionCallbackEventCode)aCallbackEventCode;

            switch (k)
            {
                case SessionCallbackEventCode.Unknown:
                    break;
                case SessionCallbackEventCode.Error:
                    break;
                case SessionCallbackEventCode.Status_Error:
                    break;
                case SessionCallbackEventCode.Execution_Error:
                    break;
                case SessionCallbackEventCode.ItIsReadyToStart:
                    break;
                case SessionCallbackEventCode.ItIsStarted:
                    break;
                case SessionCallbackEventCode.ItIsPaused:
                    break;
                case SessionCallbackEventCode.ItIsStopped:
                    break;
                case SessionCallbackEventCode.ItIsEnded:
                    break;
                case SessionCallbackEventCode.ItIsClosed:
                    break;
                case SessionCallbackEventCode.VideoCaptureDeviceRemoved:
                    {



                    }
                    break;
                default:
                    break;
            }
        }

        private void getEVRStreamFilters()
        {
            if (_EVROutputAdapter == null || _IEVRStreamControl == null)
                return;

            string lxmldoc = "";

            _IEVRStreamControl.getCollectionOfFilters(
                _EVROutputAdapter.Node,
                out lxmldoc);

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            mPipeProcessor.send("Get_EVRStreamFilters=" + lxmldoc);
        }

        private void getEVRStreamOutputFeatures()
        {
            if (_EVROutputAdapter == null || _IEVRStreamControl == null)
                return;

            string lxmldoc = "";

            _IEVRStreamControl.getCollectionOfOutputFeatures(
                _EVROutputAdapter.Node,
                out lxmldoc);

            if (string.IsNullOrEmpty(lxmldoc))
                return;

            mPipeProcessor.send("Get_EVRStreamOutputFeatures=" + lxmldoc);
        }

        private void setEVRStreamOutputFeatures(uint aIndex, int aValue)
        {
            if (_EVROutputAdapter == null || _IEVRStreamControl == null)
                return;

            _IEVRStreamControl.setOutputFeatureParametr(
                _EVROutputAdapter.Node,
                aIndex,
                aValue);
        }

        private void setFilterParametr(uint aParametrIndex, int aNewValue, bool aIsEnabled)
        {
            if (_EVROutputAdapter == null || _IEVRStreamControl == null)
                return;

            _IEVRStreamControl.setFilterParametr(
                _EVROutputAdapter.Node,
                aParametrIndex,
                aNewValue,
                aIsEnabled);
        }
    }
}
