using COMUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace WPFVirtualCameraServer
{
    [ComVisible(false)]
    public abstract class AVirtualCameraServer : ReferenceCountedObject, IVirtualCameraServer
    {
        public Guid ClassID
        {
            get { return this.GetType().GUID; }
        }

        public IntPtr get_DirectX11TextureHandler(out int retVal)
        {
            retVal = 0;

            return Tools.SharedTexture.Instance.SharedHadler;
        }
    }

    [ComVisible(false)]
    class ProxyObject : COMUtil.IObject
    {
        public Guid ClassId { get { return mAVirtualCameraServer != null ? mAVirtualCameraServer.ClassID : Guid.Empty; } }

        public Type InterfaceType { get { return typeof(IVirtualCameraServer); } }

        private AVirtualCameraServer mAVirtualCameraServer;

        private Action mWaitCallback;

        public ProxyObject(AVirtualCameraServer aAVirtualCameraServer, Action aWaitCallback = null)
        {
            mAVirtualCameraServer = aAVirtualCameraServer;

            mWaitCallback = aWaitCallback;
        }

        public object createInstance()
        {
            if (mWaitCallback != null)
                mWaitCallback();

            return mAVirtualCameraServer;
        }
    }
}
