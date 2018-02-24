using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFRecording
{
    abstract class AbstractSink
    {
        public abstract void setOptions(string aOptions);

        public abstract object getOutputNode(object aUpStreamMediaType);
    }
}
