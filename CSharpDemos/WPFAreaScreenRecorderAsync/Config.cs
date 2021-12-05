using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFAreaScreenRecorderAsync
{
    class Config
    {
        private static Config mInstance = null;

        public static Config Instance
        {
            get
            {

                if (mInstance == null)
                    mInstance = new Config();

                return mInstance;
            }
        }


        private Config() { }
    }
}
