using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterProcessRendererAsync
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

        #region Methods

        private void create()
        {
            var lSinkControl = Viewer.CaptureManager.createSinkControlAsync();

            lSinkControl.Wait();

            var lSinkFactory = lSinkControl.Result.createEVRMultiSinkFactoryAsync(Guid.Empty);

            lSinkFactory.Wait();
            
            var ltemp = lSinkFactory.Result.createOutputNodesAsync(
                    _handler,
                    1);

            ltemp.Wait();

            _Node = ltemp.Result[0];
        }

        #endregion

        #region Properties

        public object Node
        {
            get
            {
                if (_Node == null)
                {
                    create();
                }

                return _Node;
            }
            set { _Node = value; }
        }

        public Program Viewer { get; private set; }

        #endregion
    }
}
