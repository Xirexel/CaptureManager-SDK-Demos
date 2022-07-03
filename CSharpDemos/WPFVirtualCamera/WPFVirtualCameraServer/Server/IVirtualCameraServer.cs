using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFVirtualCameraServer
{
    [ComVisible(true)]
    [Guid("EEE2F595-722F-4279-B919-313189A72C36")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IVirtualCameraServer
    {
        IntPtr get_DirectX11TextureHandler(out int retVal);
    }
}
