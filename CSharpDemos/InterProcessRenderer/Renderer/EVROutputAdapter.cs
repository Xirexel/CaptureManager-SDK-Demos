using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterProcessRenderer
{
    class EVROutputAdapter
    {
        #region Fields

        private object _Node = null;

        private IntPtr _handler = IntPtr.Zero;

        #endregion

        #region Constructors

        public EVROutputAdapter(Program viewer, IntPtr handler)
        {
            Viewer = viewer;

            _handler = handler;
        }

        #endregion

        #region Properties

        public object Node
        {
            get
            {
                if (_Node == null)
                {
                    IEVRMultiSinkFactory lSinkFactory;

                    var lSinkControl = Viewer.CaptureManager.createSinkControl();

                    lSinkControl.createSinkFactory(Guid.Empty, out lSinkFactory);

                    List<object> ltemp = new List<object>();

                    lSinkFactory.createOutputNodes(
                            _handler,
                            1,
                            out ltemp);

                    _Node = ltemp[0];
                }

                return _Node;
            }
            set { _Node = value; }
        }

        public Program Viewer { get; private set; }

        #endregion
    }
}
