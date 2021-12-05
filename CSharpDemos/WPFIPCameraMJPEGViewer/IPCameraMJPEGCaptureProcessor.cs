using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WPFIPCameraMJPEGViewer
{

    internal class ByteArrayUtils
    {
        // Check if the array contains needle on specified position
        public static bool Compare(byte[] array, byte[] needle, int startIndex)
        {
            int needleLen = needle.Length;
            // compare
            for (int i = 0, p = startIndex; i < needleLen; i++, p++)
            {
                if (array[p] != needle[i])
                {
                    return false;
                }
            }
            return true;
        }

        // Find subarray in array
        public static int Find(byte[] array, byte[] needle, int startIndex, int count)
        {
            int needleLen = needle.Length;
            int index;

            while (count >= needleLen)
            {
                index = Array.IndexOf(array, needle[0], startIndex, count - needleLen + 1);

                if (index == -1)
                    return -1;

                int i, p;
                // check for needle
                for (i = 0, p = index; i < needleLen; i++, p++)
                {
                    if (array[p] != needle[i])
                    {
                        break;
                    }
                }

                if (i == needleLen)
                {
                    // found needle
                    return index;
                }

                count -= (index - startIndex + 1);
                startIndex = index + 1;
            }
            return -1;
        }
    }

    class IPCameraMJPEGCaptureProcessor : ICaptureProcessor
    {
        static Guid MFVideoFormat_MJPG = new Guid(0x47504A4D, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        string mPresentationDescriptor = "";

        private static int bufSize = 1024 * 512;

        private static int readSize = 1024;


        private Thread thread = null;
        private ManualResetEvent stopEvent = null;
        private ManualResetEvent reloadEvent = null;

        ISourceRequestResult mISourceRequestResult = null;

        byte[] mPixels = null;

        IntPtr mRawData = IntPtr.Zero;

        int mWidth = 0;

        int mHeight = 0;

        private static string mURL = "http://mx.cafesydney.com:8888/mjpg/video.mjpg";

        private IPCameraMJPEGCaptureProcessor() { }

        static public ICaptureProcessor createCaptureProcessor()
        {

            string lPresentationDescriptor = "<?xml version='1.0' encoding='UTF-8'?>" +
            "<PresentationDescriptor StreamCount='1'>" +
                "<PresentationDescriptor.Attributes Title='Attributes of Presentation'>" +
                    "<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' GUID='{58F0AAD8-22BF-4F8A-BB3D-D2C4978C6E2F}' Title='The symbolic link for a video capture driver.' Description='Contains the unique symbolic link for a video capture driver.'>" +
                        "<SingleValue Value='MJPEGCaptureProcessor' />" +
                    "</Attribute>" +
                    "<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME' GUID='{60D0E559-52F8-4FA2-BBCE-ACDB34A8EC01}' Title='The display name for a device.' Description='The display name is a human-readable string, suitable for display in a user interface.'>" +
                        "<SingleValue Value='MJPEG Capture Processor' />" +
                    "</Attribute>" +
                "</PresentationDescriptor.Attributes>" +
                "<StreamDescriptor Index='0' MajorType='MFMediaType_Video' MajorTypeGUID='{73646976-0000-0010-8000-00AA00389B71}'>" +
                    "<MediaTypes TypeCount='1'>" +
                        "<MediaType Index='0'>" +
                            "<MediaTypeItem Name='MF_MT_FRAME_SIZE' GUID='{1652C33D-D6B2-4012-B834-72030849A37D}' Title='Width and height of the video frame.' Description='Width and height of a video frame, in pixels.'>" +
                                "<Value.ValueParts>" +
                                    "<ValuePart Title='Width' Value='Temp_Width' />" +
                                    "<ValuePart Title='Height' Value='Temp_Height' />" +
                                "</Value.ValueParts>" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_AVG_BITRATE' GUID='{20332624-FB0D-4D9E-BD0D-CBF6786C102E}' Title='Approximate data rate of the video stream.' Description='Approximate data rate of the video stream, in bits per second, for a video media type.'>" +
                                "<SingleValue  Value='33570816' />" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='CM_DIRECT_CALL' GUID='{DD0570F7-0D02-4897-A55E-F65BFACA1955}' Title='Independent of samples.' Description='Specifies for a media type whether each sample is independent of the other samples in the stream.'>" +
                                "<SingleValue Value='True' />" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_MAJOR_TYPE' GUID='{48EBA18E-F8C9-4687-BF11-0A74C9F96A8F}' Title='Major type GUID for a media type.' Description='The major type defines the overall category of the media data.'>" +
                                "<SingleValue Value='MFMediaType_Video' GUID='{73646976-0000-0010-8000-00AA00389B71}' />" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_FIXED_SIZE_SAMPLES' GUID='{B8EBEFAF-B718-4E04-B0A9-116775E3321B}' Title='The fixed size of samples in stream.' Description='Specifies for a media type whether the samples have a fixed size.'>" +
                                "<SingleValue Value='False' />" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_FRAME_RATE' GUID='{C459A2E8-3D2C-4E44-B132-FEE5156C7BB0}' Title='Frame rate.' Description='Frame rate of a video media type, in frames per second.'>" +
                                "<RatioValue Value='10.0'>" +
                                    "<Value.ValueParts>" +
                                        "<ValuePart Title='Numerator'  Value='10' />" +
                                        "<ValuePart Title='Denominator'  Value='1' />" +
                                    "</Value.ValueParts>" +
                                "</RatioValue>" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_PIXEL_ASPECT_RATIO' GUID='{C6376A1E-8D0A-4027-BE45-6D9A0AD39BB6}' Title='Pixel aspect ratio.' Description='Pixel aspect ratio for a video media type.'>" +
                                "<RatioValue  Value='1'>" +
                                    "<Value.ValueParts>" +
                                        "<ValuePart Title='Numerator'  Value='1' />" +
                                        "<ValuePart Title='Denominator'  Value='1' />" +
                                    "</Value.ValueParts>" +
                                "</RatioValue>" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_ALL_SAMPLES_INDEPENDENT' GUID='{C9173739-5E56-461C-B713-46FB995CB95F}' Title='Independent of samples.' Description='Specifies for a media type whether each sample is independent of the other samples in the stream.'>" +
                                "<SingleValue Value='True' />" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_INTERLACE_MODE' GUID='{E2724BB8-E676-4806-B4B2-A8D6EFB44CCD}' Title='Describes how the frames are interlaced.' Description='Describes how the frames in a video media type are interlaced.'>" +
                                "<SingleValue Value='MFVideoInterlace_Progressive' />" +
                            "</MediaTypeItem>" +
                            "<MediaTypeItem Name='MF_MT_SUBTYPE' GUID='{F7E34C9A-42E8-4714-B74B-CB29D72C35E5}' Title='Subtype GUID for a media type.' Description='The subtype GUID defines a specific media format type within a major type.'>" +
                                "<SingleValue GUID='{Temp_SubTypeGUID}' />" +
                            "</MediaTypeItem>" +
                        "</MediaType>" +
                    "</MediaTypes>" +
                "</StreamDescriptor>" +
            "</PresentationDescriptor>";

            IPCameraMJPEGCaptureProcessor lICaptureProcessor = new IPCameraMJPEGCaptureProcessor();
            

            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(mURL);

                WebResponse resp = req.GetResponse();

                // check content type
                string ct = resp.ContentType;
                if (ct.IndexOf("multipart/x-mixed-replace") == -1)
                    throw new ApplicationException("Invalid URL");

                byte[] boundary = null;

                int boundaryLen, delimiterLen = 0, delimiter2Len = 0;

                Stream stream = null;

                // get boundary
                ASCIIEncoding encoding = new ASCIIEncoding();
                boundary = encoding.GetBytes(ct.Substring(ct.IndexOf("boundary=", 0) + 9));
                boundaryLen = boundary.Length;

                // get response stream
                stream = resp.GetResponseStream();

                int read, todo = 0, total = 0, pos = 0, align = 1;
                int start = 0, stop = 0;

                byte[] buffer = new byte[bufSize];

                byte[] delimiter = null;
                byte[] delimiter2 = null;

                MemoryStream lMemoryStream = null;

                do
                {
                    if ((read = stream.Read(buffer, total, readSize)) == 0)
                        throw new ApplicationException();

                    total += read;
                    todo += read;

                    // does we know the delimiter ?
                    if (delimiter == null)
                    {
                        // find boundary
                        pos = ByteArrayUtils.Find(buffer, boundary, pos, todo);

                        if (pos == -1)
                        {
                            // was not found
                            todo = boundaryLen - 1;
                            pos = total - todo;
                            continue;
                        }

                        todo = total - pos;

                        if (todo < 2)
                            continue;

                        // check new line delimiter type
                        if (buffer[pos + boundaryLen] == 10)
                        {
                            delimiterLen = 2;
                            delimiter = new byte[2] { 10, 10 };
                            delimiter2Len = 1;
                            delimiter2 = new byte[1] { 10 };
                        }
                        else
                        {
                            delimiterLen = 4;
                            delimiter = new byte[4] { 13, 10, 13, 10 };
                            delimiter2Len = 2;
                            delimiter2 = new byte[2] { 13, 10 };
                        }

                        pos += boundaryLen + delimiter2Len;
                        todo = total - pos;
                    }

                    // search for image
                    if (align == 1)
                    {
                        start = ByteArrayUtils.Find(buffer, delimiter, pos, todo);
                        if (start != -1)
                        {
                            // found delimiter
                            start += delimiterLen;
                            pos = start;
                            todo = total - pos;
                            align = 2;
                        }
                        else
                        {
                            // delimiter not found
                            todo = delimiterLen - 1;
                            pos = total - todo;
                        }
                    }

                    bool lout = false;

                    // search for image end
                    while ((align == 2) && (todo >= boundaryLen))
                    {
                        stop = ByteArrayUtils.Find(buffer, boundary, pos, todo);
                        if (stop != -1)
                        {
                            pos = stop;
                            todo = total - pos;

                            lMemoryStream = new MemoryStream(buffer, start, stop - start);

                            var l = JpegBitmapDecoder.Create(lMemoryStream, BitmapCreateOptions.None, BitmapCacheOption.None);

                            if (l != null && l.Frames.Count > 0)
                            {
                                var lFrame = l.Frames[0];

                                lICaptureProcessor.mWidth = lFrame.PixelWidth;

                                lICaptureProcessor.mHeight = lFrame.PixelHeight;

                                lout = true;

                                break;
                            }


                            // increment frames counter
                            //framesReceived++;

                            //// image at stop
                            //if (NewFrame != null)
                            //{
                            //    Bitmap bmp = (Bitmap)Bitmap.FromStream(new MemoryStream(buffer, start, stop - start));
                            //    // notify client
                            //    NewFrame(this, new CameraEventArgs(bmp));
                            //    // release the image
                            //    bmp.Dispose();
                            //    bmp = null;
                            //}

                            // shift array
                            pos = stop + boundaryLen;
                            todo = total - pos;
                            //Array.Copy(buffer, pos, buffer, 0, todo);

                            total = todo;
                            pos = 0;
                            align = 1;
                        }
                        else
                        {
                            // delimiter not found
                            todo = boundaryLen - 1;
                            pos = total - todo;
                        }
                    }

                    if (lout)
                        break;

                } while (true);

                stream.Close();

                resp.Close();



                lPresentationDescriptor = lPresentationDescriptor.Replace("Temp_Width", ((uint)lICaptureProcessor.mWidth).ToString());

                lPresentationDescriptor = lPresentationDescriptor.Replace("Temp_Height", ((uint)lICaptureProcessor.mHeight).ToString());

                lPresentationDescriptor = lPresentationDescriptor.Replace("Temp_SubTypeGUID", MFVideoFormat_MJPG.ToString());

                lICaptureProcessor.mPresentationDescriptor = lPresentationDescriptor;
            }


            return lICaptureProcessor;
        }


        public void initilaize(IInitilaizeCaptureSource IInitilaizeCaptureSource)
        {
            if (IInitilaizeCaptureSource != null)
            {
                IInitilaizeCaptureSource.setPresentationDescriptor(mPresentationDescriptor);
            }
        }

        public void pause()
        {
        }

        public void setCurrentMediaType(ICurrentMediaType aICurrentMediaType)
        {
            if (aICurrentMediaType == null)
                throw new NotImplementedException();

            uint lStreamIndex = 0;

            uint lMediaTypeIndex = 0;

            aICurrentMediaType.getStreamIndex(out lStreamIndex);

            aICurrentMediaType.getMediaTypeIndex(out lMediaTypeIndex);

            if (lStreamIndex != 0 || lMediaTypeIndex != 0)
                throw new NotImplementedException();
        }

        public void shutdown()
        {
            if (mRawData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mRawData);

                mRawData = IntPtr.Zero;
            }
        }

        public void sourceRequest(ISourceRequestResult aISourceRequestResult)
        {
            if (aISourceRequestResult == null)
                return;

            uint lStreamIndex = 0;

            aISourceRequestResult.getStreamIndex(out lStreamIndex);

            if (lStreamIndex == 0)
            {
                if (mISourceRequestResult == null)
                {
                    mISourceRequestResult = aISourceRequestResult;
                }
            }
        }

        public void start(long aStartPositionInHundredNanosecondUnits, ref Guid aGUIDTimeFormat)
        {
            if (thread == null)
            {
                // create events
                stopEvent = new ManualResetEvent(false);
                reloadEvent = new ManualResetEvent(false);

                // create and start new thread
                thread = new Thread(new ThreadStart(WorkerThread));
                thread.Name = mURL;
                thread.TrySetApartmentState(ApartmentState.MTA);
                thread.Start();
            }
        }

        public void stop()
        {
            if (this.Running)
            {
                thread.Abort();
                WaitForStop();
            }
        }


        // Signal thread to stop work
        public void SignalToStop()
        {
            // stop thread
            if (thread != null)
            {
                // signal to stop
                stopEvent.Set();
            }
        }

        // Wait for thread stop
        public void WaitForStop()
        {
            if (thread != null)
            {
                // wait for thread stop
                thread.Join();

                Free();
            }
        }

        // Get state of the video source thread
        public bool Running
        {
            get
            {
                if (thread != null)
                {
                    if (thread.Join(0) == false)
                        return true;

                    // the thread is not running, so free resources
                    Free();
                }
                return false;
            }
        }

        // Free resources
        private void Free()
        {
            thread = null;

            // release events
            stopEvent.Close();
            stopEvent = null;
            reloadEvent.Close();
            reloadEvent = null;
        }

        int mLength = 0;

        // Thread entry point
        public void WorkerThread()
        {
            while (true)
            {
                // reset reload event
                //reloadEvent.Reset();

                HttpWebRequest req = null;
                WebResponse resp = null;
                Stream stream = null;
                //byte[] delimiter = null;
                //byte[] delimiter2 = null;
                //byte[] boundary = null;
                //int boundaryLen, delimiterLen = 0, delimiter2Len = 0;
                //int read, todo = 0, total = 0, pos = 0, align = 1;
                //int start = 0, stop = 0;

                // align
                //  1 = searching for image start
                //  2 = searching for image end
                try
                {
                    //Create an HTTP request, as long as the request does not end, MJPEG server will always send real-time image content to the response body of the request
                    HttpWebRequest hwRequest = (System.Net.HttpWebRequest)WebRequest.Create(mURL);
                    hwRequest.Method = "GET";
                    HttpWebResponse hwResponse = (HttpWebResponse)hwRequest.GetResponse();
                    //Read the separator of each image specified by boundary, DroidCam is: - dcmjpeg
                    string contentType = hwResponse.Headers["Content-Type"];
                    string boundryKey = "boundary=";
                    string boundary = contentType.Substring(contentType.IndexOf(boundryKey) + boundryKey.Length);

                    //Get response volume flow
                    stream = hwResponse.GetResponseStream();
                    string headerName = "Content-Length:";
                    //Temporary storage of string data
                    StringBuilder sb = new StringBuilder();
                    int len = 1024;
                    while (true)
                    {
                        //Read a line of data
                        while (true)
                        {
                            char c = (char)stream.ReadByte();
                            //Console.Write(c);
                            if (c == '\n')
                            {
                                break;
                            }
                            sb.Append(c);
                        }
                        string line = sb.ToString();
                        sb.Remove(0, sb.Length);
                        //Whether the current line contains content length:
                        int i = line.IndexOf(headerName);
                        if (i != -1)
                        {
                            //Before each picture, there is a brief introduction to the picture (picture type and length). Here, we only care about the value after the length (content length:), which is used for subsequent reading of the picture
                            int imageFileLength = Convert.ToInt32(line.Substring(i + headerName.Length).Trim());
                            //Content-Length:xxx  After that, there will be a / r/n newline character, which will be the real image data
                            //Skip / r/n here
                            stream.Read(new byte[2], 0, 2);
                            //Start to read the image data. imageFileLength is the length after the content length: read
                            byte[] imageFileBytes = new byte[imageFileLength];
                            var lreadBytes = stream.Read(imageFileBytes, 0, imageFileBytes.Length);

                            var lrestLength = imageFileLength - lreadBytes;

                            while (lrestLength > 0)
                            {
                                lreadBytes += stream.Read(imageFileBytes, lreadBytes, lrestLength);

                                lrestLength = imageFileLength - lreadBytes;
                            }

                            //JPEG The header of the file is: FF D8 FF ，The end of the file is: FF D9，very important，It's better to print when debugging, so as to distinguish whether the read-in data is exactly the same as all the contents of the picture
                            //Console.WriteLine("file header): + imagefilebytes [0]. ToString (" X ") +" + imagefilebytes [1]. ToString ("X") + "+ imagefilebytes [2]. ToString (" X ") +" + imagefilebytes [3]. ToString ("X") + "+ imagefilebytes [4]. ToString (" X "))));
                            //Console.WriteLine (end of file: + imagefilebytes [imagefilelength - 2]. ToString ("X") + "+ imagefilebytes [imagefilelength - 1]. ToString (" X ")));
                            //If the file read in is incomplete, the bigger the picture is, the faster the program cycle read speed is, and the more likely it is to lead to incomplete file read. If there is a good solution, I hope you can give me some advice. Thank you very much!
                            //Is the end of the file FF D9
                            //if (imageFileBytes[imageFileLength - 2].ToString("X") != "FF" && imageFileBytes[imageFileLength - 1].ToString("X") != "D9")
                            //{
                            //    //If the content of the read file is incomplete, skip the second file and let the stream position jump to the beginning of the next picture
                            //    //Console.WriteLine (start correction...);
                            //    char l = '0';
                            //    while (true)
                            //    {
                            //        char c = (char)stream.ReadByte();
                            //        //Here, only the first two characters in dcmjpeg are judged. When two consecutive characters in the read stream are, it means that the stream has read to the beginning of the next picture
                            //        if (l == boundary[0] && c == boundary[1])
                            //        {
                            //            break;
                            //        }
                            //        l = c;
                            //    }
                            //}
                            //else
                            {

                                MemoryStream lbuff = new MemoryStream(imageFileBytes);

                                lbuff.Position = 0;

                                var lBitmap = JpegBitmapDecoder.Create(lbuff, BitmapCreateOptions.None, BitmapCacheOption.None);

                                if (lBitmap.Frames != null && lBitmap.Frames.Count > 0)
                                {
                                    BitmapSource bitmapSource = lBitmap.Frames[0];

                                    //Read the picture successfully!
                                    //m_callbackAction(bitmapSource);

                                    if (mISourceRequestResult != null)
                                    {
                                        IntPtr lptrData = Marshal.AllocHGlobal(imageFileBytes.Length);

                                        Marshal.Copy(imageFileBytes, 0, lptrData, imageFileBytes.Length);

                                        mISourceRequestResult.setData(lptrData, (uint)imageFileBytes.Length, 1);

                                        Marshal.FreeHGlobal(lptrData);
                                    }
                                }
                            }
                            //If you sleep several tens of milliseconds properly here, it will reduce the situation of incomplete picture reading. The reason of incomplete picture random reading has not been found yet
                            //Thread.Sleep(250);
                        }
                    }
                    stream.Close();
                    hwResponse.Close();
                }
                catch (WebException ex)
                {
                    System.Diagnostics.Debug.WriteLine("=============: " + ex.Message);
                    // wait for a while before the next try
                    Thread.Sleep(250);
                }
                catch (ApplicationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("=============: " + ex.Message);
                    // wait for a while before the next try
                    Thread.Sleep(250);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("=============: " + ex.Message);
                }
                finally
                {
                    // abort request
                    if (req != null)
                    {
                        req.Abort();
                        req = null;
                    }
                    // close response stream
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }
                    // close response
                    if (resp != null)
                    {
                        resp.Close();
                        resp = null;
                    }
                }

                //// need to stop ?
                //if (stopEvent.WaitOne(0, true))
                //    break;
            }
        }
    }
}
