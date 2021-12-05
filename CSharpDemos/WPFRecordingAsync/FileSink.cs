using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRecordingAsync
{
    class FileSink : AbstractSink
    {
        string mFilePath = null;

        IFileSinkFactoryAsync mSinkFactory = null;

        public FileSink(IFileSinkFactoryAsync aSinkFactory)
        {
            mSinkFactory = aSinkFactory;
        }

        public override void setOptions(string aOptions)
        {
            mFilePath = aOptions;
        }

        public override async Task<object> getOutputNode(object aUpStreamMediaType)
        {

            List<object> lCompressedMediaTypeList = new List<object>();

            lCompressedMediaTypeList.Add(aUpStreamMediaType);

            List<object> lTopologyOutputNodesList = await mSinkFactory.createOutputNodesAsync(
                lCompressedMediaTypeList,
                mFilePath);

            if (lTopologyOutputNodesList.Count == 0)
                return null;

            return lTopologyOutputNodesList[0];
        }
    }
}
