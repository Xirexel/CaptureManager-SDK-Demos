using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRecording
{
    abstract class AbstractSink
    {
        public abstract void setOptions(string aOptions);

        public abstract Task<object> getOutputNodeAsync(object aUpStreamMediaType);
    }
}
