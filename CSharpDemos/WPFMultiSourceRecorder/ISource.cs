using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMultiSourceRecorder
{
    public interface ISource
    {
        object getCompressedMediaType();

        object getSourceNode(object aOutputNode);

        void access(bool aState);
    }
}
