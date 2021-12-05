using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFMultiSourceRecorderAsync
{
    public interface ISource
    {
        Task<object> getCompressedMediaType();

        Task<object> getSourceNode(object aOutputNode);

        void access(bool aState);
    }
}
