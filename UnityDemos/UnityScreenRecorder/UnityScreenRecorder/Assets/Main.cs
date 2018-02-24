using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{

//    <?xml version="1.0"?>
//<!--XML Document of encoders-->
//<EncoderFactories>
//    <Group Title="Video" GUID="{73646976-0000-0010-8000-00AA00389B71}">
//        <EncoderFactory Name="WMVideo8 Encoder MFT" Title="WMVideo8 Encoder MFT" ="{7E320092-596A-41B2-BBEB-175D10504EB6}" />
//        <EncoderFactory Name="WMVideo9 Encoder MFT" Title="WMVideo9 Encoder MFT" CLSID="{D23B90D0-144F-46BD-841D-59E4EB19DC59}" />
//        <EncoderFactory Name="WMVideo9 Screen Encoder MFT" Title="WMVideo9 Screen Encoder MFT" CLSID="{F7FFE0A0-A4F5-44B5-949E-15ED2BC66F9D}" />
//    </Group>
//    <Group Title="Audio" GUID="{73647561-0000-0010-8000-00AA00389B71}">
//        <EncoderFactory Name="WM Speech Encoder DMO" Title="WM Speech Encoder DMO" CLSID="{1F1F4E1A-2252-4063-84BB-EEE75F8856D5}" />
//        <EncoderFactory Name="WMAudio Encoder MFT" Title="WMAudio Encoder MFT" CLSID="{70F598E9-F4AB-495A-99E2-A7C4D3D89ABF}" />
//    </Group>
//</EncoderFactories>



//    <?xml version="1.0"?>
//<!--XML Document of sink factories-->
//<SinkFactories>
//    <SinkFactory Name="EVRSinkFactory" GUID="{2F34AF87-D349-45AA-A5F1-E4104D5C458E}" Title="Enhanced Video Renderer sink factory">
//        <Value.ValueParts>
//            <ValuePart Title="Container format" Value="Default" MIME="" Description="Default EVR implementation" MaxPortCount="1" GUID="{71FBA544-3A8E-4D6C-B322-98184BC8DCEA}" />
//        </Value.ValueParts>
//    </SinkFactory>
//    <SinkFactory Name="EVRMultiSinkFactory" GUID="{10E52132-A73F-4A9E-A91B-FE18C91D6837}" Title="Enhanced Video Renderer multi sink factory">
//        <Value.ValueParts>
//            <ValuePart Title="Container format" Value="Default" MIME="" Description="Default EVR implementation" MaxPortCount="2" GUID="{E926E7A7-7DD0-4B15-88D7-413704AF865F}" />
//        </Value.ValueParts>
//    </SinkFactory>
//    <SinkFactory Name="CMVRMultiSinkFactory" GUID="{A2224D8D-C3C1-4593-8AC9-C0FCF318FF05}" Title="CaptureManager Video Renderer multi sink factory">
//        <Value.ValueParts>
//            <ValuePart Title="Container format" Value="Default" MIME="" Description="Default EVR implementation" MaxPortCount="2" GUID="{E926E7A7-7DD0-4B15-88D7-413704AF865F}" />
//        </Value.ValueParts>
//    </SinkFactory>
//    <SinkFactory Name="SampleGrabberCallSinkFactory" GUID="{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}" Title="Sample grabber call sink factory">
//        <Value.ValueParts>
//            <ValuePart Title="Container format" Value="ASYNC" MIME="" Description="Grabbing without blocking of call thread" MaxPortCount="1" GUID="{3C9F1C2E-0023-4861-8BD8-C6DED220E94D}" />
//            <ValuePart Title="Container format" Value="SYNC" MIME="" Description="Grabbing with blocking of call thread" MaxPortCount="1" GUID="{C1864678-66C7-48EA-8ED4-48EF37054990}" />
//            <ValuePart Title="Container format" Value="PULL" MIME="" Description="Grabbing with direct calling of sample" MaxPortCount="1" GUID="{B1B7F389-8D2F-471A-993A-20AB1CDE89A7}" />
//        </Value.ValueParts>
//    </SinkFactory>
//    <SinkFactory Name="SampleGrabberCallbackSinkFactory" GUID="{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}" Title="Sample grabber callback sink factory">
//        <Value.ValueParts>
//            <ValuePart Title="Container format" Value="ASYNC" MIME="" Description="Grabbing without blocking of call thread" MaxPortCount="1" GUID="{3C9F1C2E-0023-4861-8BD8-C6DED220E94D}" />
//        </Value.ValueParts>
//    </SinkFactory>
//    <SinkFactory Name="FileSinkFactory" GUID="{D6E342E3-7DDD-4858-AB91-4253643864C2}" Title="File sink factory">
//        <Value.ValueParts>
//            <ValuePart Title="Container format" Value="ASF" MIME="video/x-ms-asf" Description="ASF Media Container" MaxPortCount="126" GUID="{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}" />
//        </Value.ValueParts>
//    </SinkFactory>
//    <SinkFactory Name="ByteStreamSinkFactory" GUID="{2E891049-964A-4D08-8F36-95CE8CB0DE9B}" Title="Byte stream sink factory">
//        <Value.ValueParts>
//            <ValuePart Title="Container format" Value="ASF" MIME="video/x-ms-asf" Description="ASF Media Container" MaxPortCount="126" GUID="{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}" />
//        </Value.ValueParts>
//    </SinkFactory>
//</SinkFactories>

    
    [DllImport("CaptureManagerUnityScreenRecorderPlugin")]
    private static extern IntPtr getCollectionOfEncoders();

    [DllImport("CaptureManagerUnityScreenRecorderPlugin")]
    private static extern IntPtr getCollectionOfFileFormats();
    
    [DllImport("CaptureManagerUnityScreenRecorderPlugin")]
    private static extern void stopCapture();
    
    [DllImport("CaptureManagerUnityScreenRecorderPlugin")]
    private static extern void startCaptureProcessor(
        int aStreamIndex,
        int aMediaTypeIndex,
        System.IntPtr aRenderTexture,
        System.IntPtr aCaptureTexture,
	    System.IntPtr aVideoEncoderIID,
        System.IntPtr aFileFormatIID,
        System.IntPtr aFileName,
        int a_IsRecording);



    [DllImport("CaptureManagerUnityScreenRecorderPlugin")]
    private static extern IntPtr GetRenderEventFunc();

    public Dropdown encoderDropdown = null;

    public Dropdown fileFormatDropdown = null;

    public Button startStopBtn = null;

    public Toggle recordingEnbling = null;

    public GameObject recordingEnblingPanel = null;

    public InputField fileNameInputField = null;

    private RenderTexture renderTexture;

    private Texture2D captureTexture;

    private bool isRecording = false;

    private string m_fileName = "Test";

    struct EncoderData
    {
        public string mTitle;

        public string mCLSID;

        public string mValue;
    }

    List<EncoderData> mEncoderDataList = new List<EncoderData>();

    List<string> mEncoderTitleList = new List<string>();

    List<EncoderData> mFileFormatDataList = new List<EncoderData>();

    List<string> mFileFormatTitleList = new List<string>();
        
    bool isStarted = false;
    
    void getEncoderList()
    {
        var lBSTR = getCollectionOfEncoders();

        string lxmldoc = Marshal.PtrToStringBSTR(lBSTR);

        Marshal.FreeBSTR(lBSTR);

        XmlDocument doc = new XmlDocument();

        doc.LoadXml(lxmldoc);

        var lEncoderNodes = doc.SelectNodes("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory");

        if (lEncoderNodes == null)
            return;

        mEncoderDataList.Clear();

        foreach (var item in lEncoderNodes)
        {
            XmlNode lEncoderNode = item as XmlNode;

            if (lEncoderNode != null)
            {
                var lTitleAttr = lEncoderNode.SelectSingleNode("@Title");

                var lCLSIDAttr = lEncoderNode.SelectSingleNode("@CLSID");

                if (lCLSIDAttr != null)
                {
                    var lEncoderData = new EncoderData();

                    lEncoderData.mTitle = lTitleAttr.Value;

                    lEncoderData.mCLSID = lCLSIDAttr.Value;
                    
                    mEncoderDataList.Add(lEncoderData);

                    mEncoderTitleList.Add(lTitleAttr.Value);
                }

            }
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

        // Create screen buffer

        captureTexture = new Texture2D((Screen.width >> 1) << 1, (Screen.height >> 1) << 1, TextureFormat.RGBA32, false);


        // Set texture onto our matrial
        GetComponent<Renderer>().material.mainTexture = tex;


        // fill encoder droplists
        
        getEncoderList();

        getFileFormatList();

        if (encoderDropdown != null)
        {
            encoderDropdown.AddOptions(mEncoderTitleList);
            
            encoderDropdown.onValueChanged.AddListener(delegate
            {
                selectEncoder();
            });
        }

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

        if (recordingEnbling != null)
        {
            recordingEnbling.onValueChanged.AddListener(delegate
            {
                isRecording = recordingEnbling.isOn;

                if(isRecording)
                {
                    encoderDropdown.gameObject.SetActive(true);

                    fileFormatDropdown.gameObject.SetActive(false);

                    startStopBtn.gameObject.SetActive(false);
                    
                    fileNameInputField.gameObject.SetActive(false);  

                    encoderDropdown.value = 0;

                    fileFormatDropdown.value = 0;
                }
                else
                {
                    encoderDropdown.gameObject.SetActive(false);

                    fileFormatDropdown.gameObject.SetActive(false);

                    fileNameInputField.gameObject.SetActive(false);  

                    startStopBtn.gameObject.SetActive(true);
                }

            });
        }


        yield return StartCoroutine("CallPluginAtEndOfFrames");
    }

    void selectEncoder()
    {
        if (encoderDropdown == null)
            return;
        if (fileFormatDropdown == null)
            return;

        if (encoderDropdown.value != 0)
        {
            fileFormatDropdown.gameObject.SetActive(true);
        }
        else
        {
            fileFormatDropdown.gameObject.SetActive(false);

            fileNameInputField.gameObject.SetActive(false);  

            fileFormatDropdown.value = 0;
        }

    }

    void selectFileFormat()
    {
        if (encoderDropdown == null)
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
        if (encoderDropdown == null)
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

                encoderDropdown.gameObject.SetActive(true);

                fileFormatDropdown.gameObject.SetActive(true);

                fileNameInputField.gameObject.SetActive(true);  

                startStopBtn.GetComponentsInChildren<Text>()[0].text = "Start";
            }
            else
            {
                start();

                encoderDropdown.gameObject.SetActive(false);

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
        recordingEnblingPanel.SetActive(false);

        int lvideoEncoderIndex = 0;

        int lfileFormatIndex = 0;

        if (isRecording)
        {
            if (encoderDropdown.value == 0)
                return;

            if (fileFormatDropdown.value == 0)
                return;

            lvideoEncoderIndex = encoderDropdown.value - 1;

            lfileFormatIndex = fileFormatDropdown.value - 1;
        }

        var lEncoderCLSID = Marshal.StringToBSTR(mEncoderDataList[lvideoEncoderIndex].mCLSID);

        var lFileFormatGUID = Marshal.StringToBSTR(mFileFormatDataList[lfileFormatIndex].mCLSID);

        var lfileName = Marshal.StringToBSTR(m_fileName);

        
        
        startCaptureProcessor(
            0,
            0,
            renderTexture.GetNativeTexturePtr(),
            captureTexture.GetNativeTexturePtr(),
            lEncoderCLSID,
            lFileFormatGUID,
            lfileName,
            isRecording ? 1 : 0);

        Marshal.FreeBSTR(lfileName);

        Marshal.FreeBSTR(lFileFormatGUID);

        Marshal.FreeBSTR(lEncoderCLSID);
    }

    void stop()
    {
        recordingEnblingPanel.SetActive(true);

        stopCapture();
    }
    
    bool f = true;

    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();

            if (f)
            {

                captureTexture.ReadPixels(new Rect(0, 0, (Screen.width >> 1) << 1, (Screen.height >> 1) << 1), 0, 0);

                captureTexture.Apply();

                f = false;
            }

            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            // For our simple plugin, it does not matter which ID we pass here.
            if (isStarted)
            {
                captureTexture.ReadPixels(new Rect(0, 0, (Screen.width >> 1) << 1, (Screen.height >> 1) << 1), 0, 0);

                captureTexture.Apply();

                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }
    }

}

