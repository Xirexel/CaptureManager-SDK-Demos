using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    [DllImport("CaptureManagerUnityVideoAndAudioRecorder")]
    private static extern IntPtr getCollectionOfSources();

    [DllImport("CaptureManagerUnityVideoAndAudioRecorder")]
    private static extern IntPtr getCollectionOfEncoders();

    [DllImport("CaptureManagerUnityVideoAndAudioRecorder")]
    private static extern IntPtr getCollectionOfFileFormats();

    [DllImport("CaptureManagerUnityVideoAndAudioRecorder")]
    private static extern void stopCapture();

    [DllImport("CaptureManagerUnityVideoAndAudioRecorder")]
    private static extern IntPtr getEncoderMediaTypes(
        System.IntPtr aSymbolicLink,
        int aStreamIndex,
        int aMediaTypeIndex,
        System.IntPtr aVideoEncoderIID);

    [DllImport("CaptureManagerUnityVideoAndAudioRecorder")]
    private static extern void startRecording(
        System.IntPtr aVideoSymbolicLink,
        int aVideoStreamIndex,
        int aVideoMediaTypeIndex,
        System.IntPtr aAudioSymbolicLink,
        int aAudioStreamIndex,
        int aAudioMediaTypeIndex,
        int aCompressionQuality,
        System.IntPtr aVideoEncoderIID,
        System.IntPtr aVideoEncoderModeIID,
        int aVideoCompressedMediaTypeIndex,
        System.IntPtr aAudioEncoderIID,
        System.IntPtr aAudioEncoderModeIID,
        int aAudioCompressedMediaTypeIndex,
        System.IntPtr aFileFormatIID,
        System.IntPtr aFileName,
        System.IntPtr aRenderTexture);

    [DllImport("CaptureManagerUnityVideoAndAudioRecorder")]
    private static extern IntPtr GetRenderEventFunc();


    // Video source drop
    public Dropdown videoSourceDropdown = null;

    public Dropdown videoStreamDropdown = null;

    public Dropdown videoMediaTypeDropdown = null;


    // Audio source drop
    public Dropdown audioSourceDropdown = null;

    public Dropdown audioStreamDropdown = null;

    public Dropdown audioMediaTypeDropdown = null;



    struct SourceData
    {
        public string mFriendlyName;

        public string mSymbolicLink;

        public XmlNode mSourceNode;
    }
    
    struct StreamData
    {
        public string mFriendlyName;

        public string mGUID;

        public List<MediaTypeData> mMediaTypes;
    }

    struct MediaTypeData
    {
        public string mFriendlyData;
    }

    List<SourceData> mVideoSourceDataList = new List<SourceData>();

    List<SourceData> mAudioSourceDataList = new List<SourceData>();




    List<StreamData> mVideoStreamDataList = new List<StreamData>();

    List<StreamData> mAudioStreamDataList = new List<StreamData>();



    public Dropdown videoEncoderDropdown = null;

    public Dropdown videoEncoderModeDropdown = null;

    public Dropdown videoEncoderMediaTypeDropdown = null;
    
    public Dropdown fileFormatDropdown = null;


    public Dropdown audioEncoderDropdown = null;

    public Dropdown audioEncoderModeDropdown = null;

    public Dropdown audioEncoderMediaTypeDropdown = null;


    public GameObject compressionPanel = null;

    public Slider compressionSlider = null;


    public Button startStopBtn = null;
    
    public InputField fileNameInputField = null;

    public GameObject renderSurface = null;

    private RenderTexture renderTexture;
    
    private bool isRecording = true;

    private string m_fileName = "Test";

    private string m_currentVideoEncoderMode = "";

    private string m_currentAudioEncoderMode = "";

    struct EncoderData
    {
        public string mTitle;

        public string mCLSID;

        public string mValue;
    }

    List<EncoderData> mAudioEncoderDataList = new List<EncoderData>();

    List<EncoderData> mVideoEncoderDataList = new List<EncoderData>();
    
    List<EncoderData> mFileFormatDataList = new List<EncoderData>();

    List<string> mFileFormatTitleList = new List<string>();

    bool isStarted = false;


    void getSourceLists()
    {
        var lBSTR = getCollectionOfSources();

        string lxmldoc = Marshal.PtrToStringBSTR(lBSTR);

        Marshal.FreeBSTR(lBSTR);
        
        XmlDocument doc = new XmlDocument();

        doc.LoadXml(lxmldoc);
        
        var lSourceNodes = doc.SelectNodes("Sources/Source");

        if (lSourceNodes == null)
            return;
        
        mVideoSourceDataList.Clear();

        mAudioSourceDataList.Clear();
        
        foreach (var item in lSourceNodes)
        {
            XmlNode lSourceNode = item as XmlNode;

            if (lSourceNode != null)
            {
                var lFriendlyNameAttr = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME']/SingleValue/@Value");

                var lSymbolicLinkAttr = lSourceNode.SelectSingleNode(
                    "Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                var lhardware = lSourceNode.SelectSingleNode("Source.Attributes//SingleValue[@Value='Hardware device']");

                if (lSymbolicLinkAttr != null && lhardware != null)
                {
                    var lSourceData = new SourceData();

                    lSourceData.mFriendlyName = lFriendlyNameAttr.Value;

                    lSourceData.mSymbolicLink = lSymbolicLinkAttr.Value;

                    lSourceData.mSourceNode = lSourceNode;

                    mVideoSourceDataList.Add(lSourceData);
                }


                lSymbolicLinkAttr = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                if (lSymbolicLinkAttr != null && (lhardware != null || !lSymbolicLinkAttr.Value.Contains("AudioLoopBack")))
                {
                    var lSourceData = new SourceData();

                    lSourceData.mFriendlyName = lFriendlyNameAttr.Value;

                    lSourceData.mSymbolicLink = lSymbolicLinkAttr.Value;

                    lSourceData.mSourceNode = lSourceNode;

                    mAudioSourceDataList.Add(lSourceData);
                }
            }
        } 
    }

    void selectVideoStream()
    {
        if (videoSourceDropdown == null)
            return;
        if (videoStreamDropdown == null)
            return;

        if (videoSourceDropdown.value > 0 && videoSourceDropdown.value <= mVideoSourceDataList.Count)
        {
            videoStreamDropdown.value = 0;

            videoStreamDropdown.ClearOptions();

            videoStreamDropdown.onValueChanged.RemoveAllListeners();

            var l_streamNodes = mVideoSourceDataList[videoSourceDropdown.value - 1].mSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor");

            if (l_streamNodes == null)
                return;

            mVideoStreamDataList.Clear();

            List<string> l_StreamFriendlyNames = new List<string>();

            l_StreamFriendlyNames.Add("");   
            
            foreach (var item in l_streamNodes)
            {    
                XmlNode lStreamNode = item as XmlNode;

                if (lStreamNode != null)
                {

                    l_StreamFriendlyNames.Add("Video Stream");    
                                   
                    
                    StreamData l_StreamData = new StreamData();
                    
                    var l_media_type_nodes = lStreamNode.SelectNodes("MediaTypes/MediaType");

                    l_StreamData.mMediaTypes = new List<MediaTypeData>();
                    
                    foreach (var mediatypenode in l_media_type_nodes)
                    {
                        XmlNode l_mediatypenode = mediatypenode as XmlNode;

                        MediaTypeData l_MediaTypeData = new MediaTypeData();

                        l_MediaTypeData.mFriendlyData = l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value").Value;

                        l_MediaTypeData.mFriendlyData += " x " + l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2]/@Value").Value;

                        l_MediaTypeData.mFriendlyData += ", " + l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_RATE']/RatioValue/@Value").Value;

                        l_MediaTypeData.mFriendlyData += ", " + l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value").Value.Replace("MFVideoFormat_", "");
                        
                        l_StreamData.mMediaTypes.Add(l_MediaTypeData);
                    }
                    
                    mVideoStreamDataList.Add(l_StreamData);
                }
            }

            videoStreamDropdown.AddOptions(l_StreamFriendlyNames);

            videoStreamDropdown.gameObject.SetActive(true);

            videoStreamDropdown.onValueChanged.AddListener(delegate
            {
                videoMediaTypeDropdown.value = 0;

                selectVideoMediaType();
            });
        }
        else
        {
            videoStreamDropdown.value = 0;

            videoStreamDropdown.ClearOptions();

            videoStreamDropdown.gameObject.SetActive(false);
        }
    }

    void selectVideoMediaType()
    {
        if (videoStreamDropdown == null)
            return;
        if (videoMediaTypeDropdown == null)
            return;

        if (videoStreamDropdown.value > 0 && videoStreamDropdown.value <= mVideoStreamDataList.Count)
        {
            videoMediaTypeDropdown.ClearOptions();

            List<string> l_MediaTypeFriendlyNames = new List<string>();

            l_MediaTypeFriendlyNames.Add("");

            foreach (var item in mVideoStreamDataList[videoStreamDropdown.value - 1].mMediaTypes)
            {
                l_MediaTypeFriendlyNames.Add(item.mFriendlyData);
            }

            videoMediaTypeDropdown.AddOptions(l_MediaTypeFriendlyNames);

            videoMediaTypeDropdown.gameObject.SetActive(true);

            videoMediaTypeDropdown.value = 0;

            videoMediaTypeDropdown.onValueChanged.AddListener(delegate
            {
                videoEncoderDropdown.value = 0;

                videoEncoderDropdown.ClearOptions();

                resetVideoEncoderList();
            });
        }
        else
        {
            videoEncoderDropdown.value = 0;

            videoEncoderDropdown.ClearOptions();

            videoEncoderDropdown.gameObject.SetActive(false);

            videoMediaTypeDropdown.gameObject.SetActive(false);
        }
    }

    void selectAudioStream()
    {
        if (audioSourceDropdown == null)
            return;
        if (audioStreamDropdown == null)
            return;

        if (audioSourceDropdown.value > 0 && audioSourceDropdown.value <= mAudioSourceDataList.Count)
        {
            audioStreamDropdown.value = 0;

            audioStreamDropdown.ClearOptions();

            audioStreamDropdown.onValueChanged.RemoveAllListeners();

            var l_streamNodes = mAudioSourceDataList[audioSourceDropdown.value - 1].mSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor");

            if (l_streamNodes == null)
                return;

            mAudioStreamDataList.Clear();

            List<string> l_StreamFriendlyNames = new List<string>();

            l_StreamFriendlyNames.Add("");

            foreach (var item in l_streamNodes)
            {
                XmlNode lStreamNode = item as XmlNode;

                if (lStreamNode != null)
                {

                    l_StreamFriendlyNames.Add("Audio Stream");


                    StreamData l_StreamData = new StreamData();

                    var l_media_type_nodes = lStreamNode.SelectNodes("MediaTypes/MediaType");

                    l_StreamData.mMediaTypes = new List<MediaTypeData>();

                    foreach (var mediatypenode in l_media_type_nodes)
                    {
                        XmlNode l_mediatypenode = mediatypenode as XmlNode;

                        MediaTypeData l_MediaTypeData = new MediaTypeData();

                        l_MediaTypeData.mFriendlyData = l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_BITS_PER_SAMPLE']/SingleValue/@Value").Value;

                        l_MediaTypeData.mFriendlyData += " x " + l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_NUM_CHANNELS']/SingleValue/@Value").Value;

                        l_MediaTypeData.mFriendlyData += ", " + l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_SAMPLES_PER_SECOND']/SingleValue/@Value").Value;

                        l_MediaTypeData.mFriendlyData += ", " + l_mediatypenode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value").Value.Replace("MFAudioFormat_", "");

                        l_StreamData.mMediaTypes.Add(l_MediaTypeData);
                    }

                    mAudioStreamDataList.Add(l_StreamData);
                }
            }

            audioStreamDropdown.AddOptions(l_StreamFriendlyNames);

            audioStreamDropdown.gameObject.SetActive(true);

            audioStreamDropdown.onValueChanged.AddListener(delegate
            {
                audioMediaTypeDropdown.value = 0;

                selectAudioMediaType();
            });
        }
        else
        {
            audioStreamDropdown.value = 0;

            audioStreamDropdown.ClearOptions();

            audioStreamDropdown.gameObject.SetActive(false);
        }
    }

    void selectAudioMediaType()
    {
        if (audioStreamDropdown == null)
            return;
        if (audioMediaTypeDropdown == null)
            return;

        if (audioStreamDropdown.value > 0 && audioStreamDropdown.value <= mAudioStreamDataList.Count)
        {
            audioMediaTypeDropdown.ClearOptions();

            List<string> l_MediaTypeFriendlyNames = new List<string>();

            l_MediaTypeFriendlyNames.Add("");

            foreach (var item in mAudioStreamDataList[audioStreamDropdown.value - 1].mMediaTypes)
            {
                l_MediaTypeFriendlyNames.Add(item.mFriendlyData);
            }

            audioMediaTypeDropdown.AddOptions(l_MediaTypeFriendlyNames);

            audioMediaTypeDropdown.gameObject.SetActive(true);

            audioMediaTypeDropdown.value = 0;

            audioMediaTypeDropdown.onValueChanged.AddListener(delegate
            {
                audioEncoderDropdown.value = 0;

                audioEncoderDropdown.ClearOptions();

                resetAudioEncoderList();
            });
        }
        else
        {
            audioEncoderDropdown.value = 0;

            audioEncoderDropdown.ClearOptions();

            audioMediaTypeDropdown.gameObject.SetActive(false);

            audioEncoderDropdown.gameObject.SetActive(false);
        }
    }



    void resetVideoEncoderList()
    {
        var lBSTR = getCollectionOfEncoders();

        string lxmldoc = Marshal.PtrToStringBSTR(lBSTR);

        Marshal.FreeBSTR(lBSTR);

        XmlDocument doc = new XmlDocument();

        doc.LoadXml(lxmldoc);

        var lEncoderNodes = doc.SelectNodes("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory");

        if (lEncoderNodes == null)
            return;

        mVideoEncoderDataList.Clear();
                
        List<string> l_FriendlyNames = new List<string>();

        l_FriendlyNames.Add("");
        
        foreach (var item in lEncoderNodes)
        {
            XmlNode lEncoderNode = item as XmlNode;

            if (lEncoderNode != null)
            {
                var lTitleAttr = lEncoderNode.SelectSingleNode("@Title");

                var lCLSIDAttr = lEncoderNode.SelectSingleNode("@CLSID");

                if (lCLSIDAttr != null)
                {

                    l_FriendlyNames.Add(lTitleAttr.Value);

                    var lEncoderData = new EncoderData();

                    lEncoderData.mTitle = lTitleAttr.Value;

                    lEncoderData.mCLSID = lCLSIDAttr.Value;

                    mVideoEncoderDataList.Add(lEncoderData);
                }

            }
        }

        if(videoEncoderDropdown != null)
        {

            videoEncoderDropdown.value = 0;

            videoEncoderDropdown.ClearOptions();

            videoEncoderDropdown.gameObject.SetActive(true);

            videoEncoderDropdown.AddOptions(l_FriendlyNames);

            videoEncoderDropdown.onValueChanged.RemoveAllListeners();

            videoEncoderDropdown.onValueChanged.AddListener(delegate
            {
                getVideoEncoderMediaTypes();
            });
        }
    }


    void resetAudioEncoderList()
    {
        var lBSTR = getCollectionOfEncoders();

        string lxmldoc = Marshal.PtrToStringBSTR(lBSTR);

        Marshal.FreeBSTR(lBSTR);

        XmlDocument doc = new XmlDocument();

        doc.LoadXml(lxmldoc);

        var lEncoderNodes = doc.SelectNodes("EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory");

        if (lEncoderNodes == null)
            return;

        mAudioEncoderDataList.Clear();

        List<string> l_FriendlyNames = new List<string>();

        l_FriendlyNames.Add("");

        foreach (var item in lEncoderNodes)
        {
            XmlNode lEncoderNode = item as XmlNode;

            if (lEncoderNode != null)
            {
                var lTitleAttr = lEncoderNode.SelectSingleNode("@Title");

                var lCLSIDAttr = lEncoderNode.SelectSingleNode("@CLSID");

                if (lCLSIDAttr != null)
                {

                    l_FriendlyNames.Add(lTitleAttr.Value);

                    var lEncoderData = new EncoderData();

                    lEncoderData.mTitle = lTitleAttr.Value;

                    lEncoderData.mCLSID = lCLSIDAttr.Value;

                    mAudioEncoderDataList.Add(lEncoderData);
                }

            }
        }

        if (audioEncoderDropdown != null)
        {

            audioEncoderDropdown.value = 0;

            audioEncoderDropdown.ClearOptions();

            audioEncoderDropdown.AddOptions(l_FriendlyNames);

            audioEncoderDropdown.gameObject.SetActive(true);

            audioEncoderDropdown.onValueChanged.RemoveAllListeners();

            audioEncoderDropdown.onValueChanged.AddListener(delegate
            {
                getAudioEncoderMediaTypes();
            });
        }
    }

    void getVideoEncoderMediaTypes()
    {
        if (videoEncoderDropdown == null)
            return;

        if (videoEncoderDropdown.value > 0 && videoEncoderDropdown.value <= mVideoEncoderDataList.Count)
        {

            var lEncoderIID = mVideoEncoderDataList[videoEncoderDropdown.value - 1].mCLSID;
            
            var lSymbolicLink = mVideoSourceDataList[videoSourceDropdown.value - 1].mSymbolicLink;

            var l_modes = getEncoderMediaTypes(
                lSymbolicLink,
                videoStreamDropdown.value - 1,
                videoMediaTypeDropdown.value - 1,
                lEncoderIID,
                true);


            videoEncoderModeDropdown.value = 0;

            videoEncoderModeDropdown.ClearOptions();  

            List<string> l_ModeFriendlyNames = new List<string>();
            
            foreach (var item in l_modes)
            {
                StreamData l_mode = (StreamData)item;

                l_ModeFriendlyNames.Add(l_mode.mFriendlyName);                
            }

            videoEncoderModeDropdown.AddOptions(l_ModeFriendlyNames);

            videoEncoderModeDropdown.gameObject.SetActive(true);

            videoEncoderModeDropdown.onValueChanged.RemoveAllListeners();

            videoEncoderModeDropdown.onValueChanged.AddListener(delegate
            {
                if(videoEncoderMediaTypeDropdown == null)
                    return;

                if (videoEncoderModeDropdown.value > 0 && videoEncoderModeDropdown.value <= l_modes.Count)
                {
                    videoEncoderMediaTypeDropdown.value = 0;

                    videoEncoderMediaTypeDropdown.ClearOptions();                                       

                    List<string> l_MediaTypeFriendlyNames = new List<string>();

                    m_currentVideoEncoderMode = l_modes[videoEncoderModeDropdown.value].mGUID;
                    
                    foreach (var item in l_modes[videoEncoderModeDropdown.value].mMediaTypes)
                    {
                        l_MediaTypeFriendlyNames.Add(item.mFriendlyData);
                    }

                    videoEncoderMediaTypeDropdown.AddOptions(l_MediaTypeFriendlyNames);

                    videoEncoderMediaTypeDropdown.gameObject.SetActive(true);
                }
                else
                {
                    videoEncoderMediaTypeDropdown.value = 0;

                    videoEncoderMediaTypeDropdown.ClearOptions();

                    videoEncoderMediaTypeDropdown.gameObject.SetActive(false);
                }

                videoEncoderMediaTypeDropdown.onValueChanged.RemoveAllListeners();

                videoEncoderMediaTypeDropdown.onValueChanged.AddListener(delegate
                {
                    resetFileFormat();
                });

            });
        }
        else
        {
            videoEncoderModeDropdown.value = 0;

            videoEncoderModeDropdown.ClearOptions();

            videoEncoderModeDropdown.gameObject.SetActive(false);
        }
    }

    void getAudioEncoderMediaTypes()
    {
        if (audioEncoderDropdown == null)
            return;

        if (audioEncoderDropdown.value > 0 && audioEncoderDropdown.value <= mAudioEncoderDataList.Count)
        {

            var lEncoderIID = mAudioEncoderDataList[audioEncoderDropdown.value - 1].mCLSID;
            
            var lSymbolicLink = mAudioSourceDataList[audioSourceDropdown.value - 1].mSymbolicLink;

            var l_modes = getEncoderMediaTypes(
                lSymbolicLink,
                audioStreamDropdown.value - 1,
                audioMediaTypeDropdown.value - 1,
                lEncoderIID,
                false);


            audioEncoderModeDropdown.value = 0;

            audioEncoderModeDropdown.ClearOptions();

            List<string> l_ModeFriendlyNames = new List<string>();

            foreach (var item in l_modes)
            {
                StreamData l_mode = (StreamData)item;

                l_ModeFriendlyNames.Add(l_mode.mFriendlyName);
            }

            audioEncoderModeDropdown.AddOptions(l_ModeFriendlyNames);

            audioEncoderModeDropdown.gameObject.SetActive(true);

            audioEncoderModeDropdown.onValueChanged.RemoveAllListeners();

            audioEncoderModeDropdown.onValueChanged.AddListener(delegate
            {
                if (audioEncoderMediaTypeDropdown == null)
                    return;

                if (audioEncoderModeDropdown.value > 0 && audioEncoderModeDropdown.value <= l_modes.Count)
                {
                    audioEncoderMediaTypeDropdown.value = 0;

                    audioEncoderMediaTypeDropdown.ClearOptions();

                    List<string> l_MediaTypeFriendlyNames = new List<string>();

                    m_currentAudioEncoderMode = l_modes[audioEncoderModeDropdown.value].mGUID;

                    foreach (var item in l_modes[audioEncoderModeDropdown.value].mMediaTypes)
                    {
                        l_MediaTypeFriendlyNames.Add(item.mFriendlyData);
                    }

                    audioEncoderMediaTypeDropdown.AddOptions(l_MediaTypeFriendlyNames);

                    audioEncoderMediaTypeDropdown.gameObject.SetActive(true);
                }
                else
                {
                    audioEncoderMediaTypeDropdown.value = 0;

                    audioEncoderMediaTypeDropdown.ClearOptions();

                    audioEncoderMediaTypeDropdown.gameObject.SetActive(false);
                }

                audioEncoderMediaTypeDropdown.onValueChanged.RemoveAllListeners();

                audioEncoderMediaTypeDropdown.onValueChanged.AddListener(delegate
                {
                    resetFileFormat();
                });

            });
        }
        else
        {
            audioEncoderModeDropdown.value = 0;

            audioEncoderModeDropdown.ClearOptions();

            audioEncoderModeDropdown.gameObject.SetActive(false);
        }
    }

    List<StreamData> getEncoderMediaTypes(string aSymbolicLink,
                            int aStreamIndex,
                            int aMediaTypeIndex,
                            string aEncoderIID,
                            bool aIsVideo)
    {
        var l_SymbolicLink_BSTR = Marshal.StringToBSTR(aSymbolicLink);

        var l_EncoderIID_BSTR = Marshal.StringToBSTR(aEncoderIID);

        var lBSTR = getEncoderMediaTypes(l_SymbolicLink_BSTR, aStreamIndex, aMediaTypeIndex, l_EncoderIID_BSTR);

        Marshal.FreeBSTR(l_EncoderIID_BSTR);

        Marshal.FreeBSTR(l_SymbolicLink_BSTR);

        string lxmldoc = Marshal.PtrToStringBSTR(lBSTR);

        Marshal.FreeBSTR(lBSTR);

        Debug.Log(lxmldoc);

        XmlDocument doc = new XmlDocument();

        doc.LoadXml(lxmldoc);

        var lGroupNodes = doc.SelectNodes("EncoderMediaTypes/Group");

        List<StreamData> l_groups = new List<StreamData>();

        l_groups.Add(new StreamData() { mFriendlyName = "", mMediaTypes = new List<MediaTypeData>() { new MediaTypeData(){ mFriendlyData = ""} } });

        if (lGroupNodes == null)
            return l_groups;

        foreach (var item in lGroupNodes)
        {
            XmlNode lGroupNode = item as XmlNode;

            if (lGroupNode != null)
            {
                StreamData l_StreamData = new StreamData();

                l_StreamData.mFriendlyName = lGroupNode.Attributes["Title"].Value;

                l_StreamData.mGUID = lGroupNode.Attributes["GUID"].Value;

                l_StreamData.mMediaTypes = new List<MediaTypeData>();

                l_StreamData.mMediaTypes.Add(new MediaTypeData() { mFriendlyData = "" });

                var l_MediaTypeNodes = lGroupNode.SelectNodes("MediaTypes/MediaType");

                foreach (var itemnode in l_MediaTypeNodes)
                {
                    XmlNode l_MediaTypeNode = itemnode as XmlNode;

                    if (l_MediaTypeNode != null)
                    {

                        MediaTypeData l_MediaTypeData = new MediaTypeData();

                        if (aIsVideo)
                        {

                            l_MediaTypeData.mFriendlyData = l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value").Value;

                            l_MediaTypeData.mFriendlyData += " x " + l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2]/@Value").Value;

                            l_MediaTypeData.mFriendlyData += ", " + l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_RATE']/RatioValue/@Value").Value;

                            l_MediaTypeData.mFriendlyData += ", " + l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value").Value.Replace("MFVideoFormat_", "");
                            
                            var ltempNode = l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_MPEG2_PROFILE']/SingleValue/@Value");
                        
                            if (ltempNode != null)
                            {
                                switch (ltempNode.Value)
	                            {
                                    case "66":
                                        l_MediaTypeData.mFriendlyData += ", " + "Baseline Profile";
                                        break;
                                    case "77":
                                        l_MediaTypeData.mFriendlyData += ", " + "Main Profile";
                                        break;
                                    case "100":
                                        l_MediaTypeData.mFriendlyData += ", " + "High Profile";
                                        break;
		                            default:
                                        break;
	                            }
                            }   
                        }
                        else
                        {
                            var ltempNode = l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_BITS_PER_SAMPLE']/SingleValue/@Value");

                            if (ltempNode != null)
                                l_MediaTypeData.mFriendlyData = ltempNode.Value;

                            l_MediaTypeData.mFriendlyData += " x " + l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_NUM_CHANNELS']/SingleValue/@Value").Value;

                            l_MediaTypeData.mFriendlyData += ", " + l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_SAMPLES_PER_SECOND']/SingleValue/@Value").Value;

                            l_MediaTypeData.mFriendlyData += ", " + l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value").Value.Replace("MFAudioFormat_", "");
                        
                            ltempNode = l_MediaTypeNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_AVG_BYTES_PER_SECOND']/SingleValue/@Value");
                        
                            if (ltempNode != null)
                            {
                                int l_value = 0;

                                if(int.TryParse(ltempNode.Value, out l_value))
                                {
                                    l_MediaTypeData.mFriendlyData += ", " + (l_value * 8)/1000 + " kbits";
                                }
                            }                                                
                        }
                        
                        l_StreamData.mMediaTypes.Add(l_MediaTypeData);
                    }
                }


                l_groups.Add(l_StreamData);
            }
        }

        return l_groups;       
    }

    void resetFileFormat()
    {
        if (videoEncoderMediaTypeDropdown.value == 0 && audioEncoderMediaTypeDropdown.value == 0)
        {
            fileFormatDropdown.value = 0;

            fileFormatDropdown.gameObject.SetActive(false);

            fileNameInputField.gameObject.SetActive(false);


            if (compressionPanel != null)
                compressionPanel.SetActive(false);

            if (compressionSlider != null)
                compressionSlider.gameObject.SetActive(false);   
        }
        else
        {
            fileFormatDropdown.gameObject.SetActive(true);
            
            if (compressionPanel != null)
                compressionPanel.SetActive(true);

            if (compressionSlider != null)
                compressionSlider.gameObject.SetActive(true);   
        }
    }
    
    void getFileFormatList()
    {
        var lBSTR = getCollectionOfFileFormats();

        string lxmldoc = Marshal.PtrToStringBSTR(lBSTR);

        Marshal.FreeBSTR(lBSTR);

        XmlDocument doc = new XmlDocument();

        doc.LoadXml(lxmldoc);

        var lFileFormatNodes = doc.SelectNodes("SinkFactories/SinkFactory[@GUID='{D6E342E3-7DDD-4858-AB91-4253643864C2}']/Value.ValueParts/ValuePart");

        if (lFileFormatNodes == null)
            return;

        mFileFormatDataList.Clear();

        mFileFormatTitleList.Add("");

        foreach (var item in lFileFormatNodes)
        {
            XmlNode lFileFormatNode = item as XmlNode;

            if (lFileFormatNode != null)
            {
                var lTitleAttr = lFileFormatNode.SelectSingleNode("@Description");

                var lGUIDAttr = lFileFormatNode.SelectSingleNode("@GUID");

                var lValue = lFileFormatNode.SelectSingleNode("@Value");

                if (lGUIDAttr != null)
                {
                    var lFileFormatData = new EncoderData();

                    lFileFormatData.mTitle = lTitleAttr.Value;

                    lFileFormatData.mCLSID = lGUIDAttr.Value;

                    lFileFormatData.mValue = lValue.Value;

                    mFileFormatDataList.Add(lFileFormatData);

                    mFileFormatTitleList.Add(lTitleAttr.Value);
                }
            }
        }
    }

    void reset()
    {
        if (videoSourceDropdown != null)
            videoSourceDropdown.ClearOptions();

        if (videoStreamDropdown != null)
        {
            videoStreamDropdown.gameObject.SetActive(false);

            videoStreamDropdown.ClearOptions();
        }

        if (videoMediaTypeDropdown != null)
        {
            videoMediaTypeDropdown.gameObject.SetActive(false);

            videoMediaTypeDropdown.ClearOptions();
        }

        if (videoEncoderDropdown != null)
        {
            videoEncoderDropdown.gameObject.SetActive(false);

            videoEncoderDropdown.ClearOptions();
        }

        if (videoEncoderModeDropdown != null)
        {
            videoEncoderModeDropdown.gameObject.SetActive(false);

            videoEncoderModeDropdown.ClearOptions();
        }

        if (videoEncoderMediaTypeDropdown != null)
        {
            videoEncoderMediaTypeDropdown.gameObject.SetActive(false);

            videoEncoderMediaTypeDropdown.ClearOptions();
        }

        if (audioSourceDropdown != null)
            audioSourceDropdown.ClearOptions();

        if (audioStreamDropdown != null)
        {
            audioStreamDropdown.gameObject.SetActive(false);

            audioStreamDropdown.ClearOptions();
        }

        if (audioMediaTypeDropdown != null)
        {
            audioMediaTypeDropdown.gameObject.SetActive(false);

            audioMediaTypeDropdown.ClearOptions();
        }

        if (audioEncoderDropdown != null)
        {
            audioEncoderDropdown.gameObject.SetActive(false);

            audioEncoderDropdown.ClearOptions();
        }

        if (audioEncoderModeDropdown != null)
        {
            audioEncoderModeDropdown.gameObject.SetActive(false);

            audioEncoderModeDropdown.ClearOptions();
        }

        if (audioEncoderMediaTypeDropdown != null)
        {
            audioEncoderMediaTypeDropdown.gameObject.SetActive(false);

            audioEncoderMediaTypeDropdown.ClearOptions();
        }

        if (compressionPanel != null)
            compressionPanel.SetActive(false);

        if (compressionSlider != null)
            compressionSlider.gameObject.SetActive(false);        

        if (fileFormatDropdown != null)
            fileFormatDropdown.gameObject.SetActive(false);

        if (fileFormatDropdown != null)
            fileNameInputField.gameObject.SetActive(false);

        if (fileFormatDropdown != null)
            startStopBtn.gameObject.SetActive(false);
        
    }


    // Use this for initialization
    IEnumerator Start()
    {

        // Create a texture
        RenderTexture tex = new RenderTexture(800, 600, 0, RenderTextureFormat.ARGB32);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Point;
        // Call Apply() so it's actually uploaded to the GPU
        //tex.Apply();

        tex.Create();

        renderTexture = tex;
        

        // Set texture onto our matrial
        renderSurface.GetComponent<Renderer>().material.mainTexture = tex;

        reset();

        // fill source droplists
    
        getSourceLists();
        
        if (videoSourceDropdown != null)
        {
            videoSourceDropdown.ClearOptions();

            List<string> l_SourceFriendlyNames = new List<string>();

            l_SourceFriendlyNames.Add("");

            foreach (var item in mVideoSourceDataList)
            {
                l_SourceFriendlyNames.Add(item.mFriendlyName);
            }

            videoSourceDropdown.AddOptions(l_SourceFriendlyNames);

            videoSourceDropdown.onValueChanged.RemoveAllListeners();

            videoSourceDropdown.onValueChanged.AddListener(delegate
            {
                videoStreamDropdown.value = 0;

                selectVideoStream();
            });
        }

        if (audioSourceDropdown != null)
        {
            audioSourceDropdown.ClearOptions();

            List<string> l_SourceFriendlyNames = new List<string>();

            l_SourceFriendlyNames.Add("");

            foreach (var item in mAudioSourceDataList)
            {
                l_SourceFriendlyNames.Add(item.mFriendlyName);
            }

            audioSourceDropdown.AddOptions(l_SourceFriendlyNames);

            audioSourceDropdown.onValueChanged.RemoveAllListeners();

            audioSourceDropdown.onValueChanged.AddListener(delegate
            {
                audioStreamDropdown.value = 0;

                selectAudioStream();
            });
        }

        getFileFormatList();

        fileFormatDropdown.value = 0;

        fileFormatDropdown.ClearOptions();

        if (fileFormatDropdown != null)
        {
            fileFormatDropdown.AddOptions(mFileFormatTitleList);

            fileFormatDropdown.onValueChanged.AddListener(delegate
            {
                selectFileFormat();
            });
        }

        if (startStopBtn != null)
        {
            startStopBtn.onClick.AddListener(delegate
            {
                startStopBtnClick();
            });
        }

        if (fileNameInputField != null)
        {
            fileNameInputField.onEndEdit.AddListener((string a_fileName) =>
            {
                m_fileName = a_fileName;
            });
        }
        
        yield return StartCoroutine("CallPluginAtEndOfFrames");
    }
    
    void selectFileFormat()
    {
        if (videoEncoderDropdown == null)
            return;
        if (fileFormatDropdown == null)
            return;
        if (startStopBtn == null)
            return;
        if (fileNameInputField == null)
            return;



        if (fileFormatDropdown.value != 0)
        {
            fileNameInputField.gameObject.SetActive(true);

            var lValue = mFileFormatDataList[fileFormatDropdown.value - 1].mValue;

            m_fileName = "Test." + lValue.ToLower();

            fileNameInputField.text = m_fileName;

            startStopBtn.gameObject.SetActive(true);

            startStopBtn.GetComponentsInChildren<Text>()[0].text = "Start";
        }
        else
        {
            fileNameInputField.gameObject.SetActive(false);

            startStopBtn.gameObject.SetActive(false);
        }
    }

    void startStopBtnClick()
    {
        if (videoEncoderDropdown == null)
            return;
        if (fileFormatDropdown == null)
            return;
        if (startStopBtn == null)
            return;

        if (isRecording)
        {
            if (isStarted)
            {
                stop();

                videoEncoderDropdown.gameObject.SetActive(true);

                fileFormatDropdown.gameObject.SetActive(true);

                fileNameInputField.gameObject.SetActive(true);

                startStopBtn.GetComponentsInChildren<Text>()[0].text = "Start";
            }
            else
            {
                start();

                videoEncoderDropdown.gameObject.SetActive(false);

                fileFormatDropdown.gameObject.SetActive(false);

                fileNameInputField.gameObject.SetActive(false);

                startStopBtn.GetComponentsInChildren<Text>()[0].text = "Stop";
            }
        }
        else
        {
            if (isStarted)
            {
                stop();

                startStopBtn.GetComponentsInChildren<Text>()[0].text = "Start";
            }
            else
            {
                start();

                startStopBtn.GetComponentsInChildren<Text>()[0].text = "Stop";
            }
        }

        isStarted = !isStarted;
    }

    void start()
    {
        int lvideoEncoderIndex = 0;
        
        int lfileFormatIndex = 0;

        if (isRecording)
        {
            if (fileFormatDropdown.value == 0)
                return;

            lvideoEncoderIndex = videoEncoderDropdown.value - 1;
            
            lfileFormatIndex = fileFormatDropdown.value - 1;
        }

		Debug.Log (isRecording);
		Debug.Log (lvideoEncoderIndex);

        string lVideoEncoderCLSID = "";

        if (lvideoEncoderIndex >= 0 && mVideoEncoderDataList.Count > lvideoEncoderIndex)
            lVideoEncoderCLSID = mVideoEncoderDataList[lvideoEncoderIndex].mCLSID;
        
        var lVideoEncoderCLSIDBSTR = Marshal.StringToBSTR(lVideoEncoderCLSID);


        string lAudioEncoderCLSID = "";

        if (audioEncoderDropdown.value > 0 && mAudioEncoderDataList.Count > 0)
            lAudioEncoderCLSID = mAudioEncoderDataList[audioEncoderDropdown.value - 1].mCLSID;

        var lAudioEncoderCLSIDBSTR = Marshal.StringToBSTR(lAudioEncoderCLSID);

        var lFileFormatGUIDBSTR = Marshal.StringToBSTR(mFileFormatDataList[lfileFormatIndex].mCLSID);

        var lfileNameBSTR = Marshal.StringToBSTR(m_fileName);



        var lVideoSymbolicLink = "";

        if (mVideoSourceDataList.Count > (videoSourceDropdown.value - 1) && videoSourceDropdown.value > 0)
            lVideoSymbolicLink = mVideoSourceDataList[videoSourceDropdown.value - 1].mSymbolicLink;

        var lAudioSymbolicLink = "";

        if (mAudioSourceDataList.Count > (audioSourceDropdown.value - 1) && audioSourceDropdown.value > 0)
            lAudioSymbolicLink = mAudioSourceDataList[audioSourceDropdown.value - 1].mSymbolicLink;


        var lVideoSymbolicLinkBSTR = Marshal.StringToBSTR(lVideoSymbolicLink);

        var lAudioSymbolicLinkBSTR = Marshal.StringToBSTR(lAudioSymbolicLink);





        var lcurrentVideoEncoderModeBSTR = Marshal.StringToBSTR(m_currentVideoEncoderMode);

        var lcurrentAudioEncoderModeBSTR = Marshal.StringToBSTR(m_currentAudioEncoderMode);


        int lqualityCompression = 50;

        if (compressionSlider != null)
            lqualityCompression = (int)(compressionSlider.value * 100.0f);


        startRecording(
        lVideoSymbolicLinkBSTR,
        videoStreamDropdown.value - 1,
        videoMediaTypeDropdown.value - 1,
        lAudioSymbolicLinkBSTR,
        audioStreamDropdown.value - 1,
        audioMediaTypeDropdown.value - 1,
        lqualityCompression,
        lVideoEncoderCLSIDBSTR,
        lcurrentVideoEncoderModeBSTR,
        videoEncoderMediaTypeDropdown.value - 1,
        lAudioEncoderCLSIDBSTR,
        lcurrentAudioEncoderModeBSTR,
        audioEncoderMediaTypeDropdown.value - 1,
        lFileFormatGUIDBSTR,
        lfileNameBSTR,
        renderTexture.GetNativeTexturePtr());



        Marshal.FreeBSTR(lfileNameBSTR);

        Marshal.FreeBSTR(lFileFormatGUIDBSTR);

        Marshal.FreeBSTR(lVideoEncoderCLSIDBSTR);

        Marshal.FreeBSTR(lAudioEncoderCLSIDBSTR);

        Marshal.FreeBSTR(lVideoSymbolicLinkBSTR);

        Marshal.FreeBSTR(lAudioSymbolicLinkBSTR);

        Marshal.FreeBSTR(lcurrentVideoEncoderModeBSTR);

        Marshal.FreeBSTR(lcurrentAudioEncoderModeBSTR);

        
    }

    void stop()
    {
        stopCapture();
    }
    
    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();
            
            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            // For our simple plugin, it does not matter which ID we pass here.
            if (isStarted)
            {
                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }
    }
}