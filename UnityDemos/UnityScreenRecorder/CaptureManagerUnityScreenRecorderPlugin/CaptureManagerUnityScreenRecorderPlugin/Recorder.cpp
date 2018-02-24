#include "Recorder.h"
#include <Unknwnbase.h>
#include "../Common/ComPtrCustom.h"
#include "../Common/pugixml.hpp"
#include "../Common/CaptureManagerTypeInfo.h"

using namespace pugi;

#define IID_PPV_ARGSIUnknown(ppType) __uuidof(**(ppType)), (IUnknown**)(ppType)

class DECLSPEC_UUID("EE8C3745-F45B-42B3-A8CC-C7A696440955")
	VaribleBitRateCLSID;

class DECLSPEC_UUID("CA37E2BE-BEC0-4B17-946D-44FBC1B3DF55")
	ConstantBitRateCLSID;

//class DECLSPEC_UUID("E80A6BFD-D9C2-4A1F-95DC-14685CACEF3E")
//	MP4CLSID;

////<EncoderFactory Name="H264 Encoder MFT" Title="H264 Encoder MFT" CLSID="{6CA50344-051A-4DED-9779-A43305165E35}" /> 
//class DECLSPEC_UUID("6CA50344-051A-4DED-9779-A43305165E35")
//	VideoEncoderCLSID;

////<EncoderFactory Name="Microsoft AAC Audio Encoder MFT" Title="Microsoft AAC Audio Encoder MFT" CLSID="{93AF0C51-2275-45D2-A35B-F2BA21CAED00}" />
//class DECLSPEC_UUID("93AF0C51-2275-45D2-A35B-F2BA21CAED00")
//	AudioEncoderCLSID;

GUID g_FileFormatIID;

BOOL g_initEncoder = FALSE;

CComPtrCustom<IEncoderNodeFactory> g_VideoEncoderNodeFactory;

CComPtrCustom<IEncoderNodeFactory> g_AudioEncoderNodeFactory;

CComPtrCustom<IRenderingControl> g_RenderingControl;

CComPtrCustom<IUnknown> g_RendererActivateTopologyNode;

CComPtrCustom<IUnknown> g_CaptureProcessor;


int initEncoders(
	ICaptureManagerControl* g_CaptureManagerControl,
	std::wstring aEncoderIID,
	std::wstring aAudioEncoderIID,
	std::wstring aFileFormatIID);

void collectMediaInfo(xml_document& lSourcesXmlDoc, std::wstring a_SymbolicLink, std::vector<std::wstring>& a_refMediaInfo);

HRESULT createSourceNode(
	ICaptureManagerControl* g_CaptureManagerControl,
	const std::wstring& aRefSymbolicLink,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown* aPtrUnkEncoderNode,
	IUnknown** aPtrPtrUnkSourceNode);

HRESULT createSourceNode(
	ICaptureManagerControl* g_CaptureManagerControl,
	IUnknown* a_UnkCaptureProcessor,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown* aPtrUnkEncoderNode,
	IUnknown** aPtrPtrUnkSourceNode);

HRESULT createSession(
	ICaptureManagerControl* g_CaptureManagerControl,
	std::vector<IUnknown*> aSourceNodes,
	ISession** aPtrPtrSession);

HRESULT createSourceMediaType(
	ICaptureManagerControl* g_CaptureManagerControl,
	const std::wstring& aRefSymbolicLink,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown** aPtrPtrSourceMediaType);

HRESULT createSourceMediaType(
	ICaptureManagerControl* g_CaptureManagerControl,
	IUnknown* a_UnkCaptureProcessor,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown** aPtrPtrSourceMediaType);

HRESULT createCompresssedMediaType(
	IUnknown* aPtrSourceMediaType,
	REFGUID aRefEncodingModeGUID,
	IEncoderNodeFactory* aPtrEncoderNodeFactory,
	IUnknown** aPtrPtrUnkCompresssedMediaType);

HRESULT createOutputNodes(
	ICaptureManagerControl* g_CaptureManagerControl,
	std::vector<IUnknown*> aCompressedMediaTypes,
	std::wstring& aRefFileName,
	REFIID a_RefFileFormat,
	std::vector<CComPtrCustom<IUnknown>>& aOutputNodes);

HRESULT createEncoderNode(
	IUnknown* aPtrSourceMediaType,
	REFGUID aRefEncodingModeGUID,
	IEncoderNodeFactory* aPtrEncoderNodeFactory,
	IUnknown* aPtrUnkOutputNode,
	IUnknown** aPtrPtrUnkEncoderNode);

HRESULT createSpreaderNode(
	ICaptureManagerControl* g_CaptureManagerControl,
	IUnknown* aPtrUnkVideoEncoderNode,
	IUnknown* aPtrUnkVideoRendererNode,
	IUnknown** aPtrPtrUnkSplitterNode);

Recorder::Recorder():
mPtrICaptureManagerControl(nullptr),
mPtrISession(nullptr),
mMediaIndex(0)
{
}

Recorder::~Recorder()
{
	if (g_RendererActivateTopologyNode != nullptr)
		g_RendererActivateTopologyNode.Release();

	if (mPtrICaptureManagerControl != nullptr)
		mPtrICaptureManagerControl->Release();

	if (mPtrISession != nullptr)
	{
		mPtrISession->stopSession();
		mPtrISession->closeSession();
		mPtrISession->Release();
	}	
}

std::vector<std::wstring> Recorder::getMediaInfo()
{
	return mMediaInfo;
}

void Recorder::init(
	std::wstring a_SymbolicLink, 
	ICaptureManagerControl* aPtrICaptureManagerControl)
{
	//if (aPtrICaptureManagerControl == nullptr)
	//	return;
	//
	//mPtrICaptureManagerControl = aPtrICaptureManagerControl;

	//mPtrICaptureManagerControl->AddRef();

	//if (g_initEncoder == FALSE);
	//	initEncoders(mPtrICaptureManagerControl);


	//// get ISourceControl inetrface
	//CComPtrCustom<ISourceControl> lSourceControl;

	//HRESULT lhresult = mPtrICaptureManagerControl->createControl(
	//	IID_PPV_ARGSIUnknown(&lSourceControl));

	//if (FAILED(lhresult))
	//	return;

	//BSTR lXMLString = nullptr;

	//lhresult = lSourceControl->getCollectionOfSources(&lXMLString);

	//if (FAILED(lhresult))
	//	return;


	//xml_document lSourcesXmlDoc;

	//lSourcesXmlDoc.load_string(lXMLString);
	//
	//// Get source collection.
	//collectMediaInfo(lSourcesXmlDoc, a_SymbolicLink, mMediaInfo);

	//SysFreeString(lXMLString);

	//m_SymbolicLink = a_SymbolicLink;
}

void Recorder::init(IUnknown* a_UnkSource,
	ICaptureManagerControl* aPtrICaptureManagerControl,
	std::wstring aVideoEncoderIID,
	std::wstring aAudioEncoderIID,
	std::wstring aFileFormatIID)
{
	if (aPtrICaptureManagerControl == nullptr)
		return;

	mPtrICaptureManagerControl = aPtrICaptureManagerControl;

	mPtrICaptureManagerControl->AddRef();

	if (g_initEncoder == FALSE);
	initEncoders(mPtrICaptureManagerControl,
		aVideoEncoderIID,
		aAudioEncoderIID,
		aFileFormatIID);
	
	g_CaptureProcessor = a_UnkSource;

	m_SymbolicLink.clear();
}

void Recorder::setMediaInfoIndex(unsigned int aMediaIndex)
{
	mMediaIndex = aMediaIndex;
}

void Recorder::startPreview(void* aPtrWindow, bool aEnableInnerRenderer)
{
	closeRecorder();

	HRESULT lhresult(E_FAIL);

	VARIANT lArrayPtrTopologyOutputNodes;

	VariantInit(&lArrayPtrTopologyOutputNodes);

	do
	{
		if (mPtrICaptureManagerControl == nullptr)
			break;

		CComPtrCustom<ISinkControl> lSinkControl;

		lhresult = mPtrICaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSinkControl));

		if (FAILED(lhresult))
			break;

		if (!g_RenderingControl)
			lhresult = mPtrICaptureManagerControl->createMisc(
				IID_PPV_ARGSIUnknown(&g_RenderingControl));

		if (FAILED(lhresult))
			break;
		
		CComPtrCustom<IEVRMultiSinkFactory> lEVRMultiSinkFactory;

		lhresult = lSinkControl->createSinkFactory(
			GUID_NULL,
			IID_PPV_ARGSIUnknown(&lEVRMultiSinkFactory));

		if (FAILED(lhresult))
			break;

		lhresult = lEVRMultiSinkFactory->createOutputNodes(
			nullptr,
			(IUnknown*)aPtrWindow,
			1,
			&lArrayPtrTopologyOutputNodes);

		if (FAILED(lhresult))
			break;



		if (lArrayPtrTopologyOutputNodes.vt == VT_SAFEARRAY | VT_UNKNOWN)
		{
			long i = 0;

			VARIANT lVar;

			VariantInit(&lVar);

			lhresult = SafeArrayGetElement(lArrayPtrTopologyOutputNodes.parray, &i, &lVar);

			if (FAILED(lhresult))
				continue;
			
			if (lVar.punkVal != nullptr)
				g_RendererActivateTopologyNode = lVar.punkVal;
			
			VariantClear(&lVar);
		}

		SafeArrayDestroy(lArrayPtrTopologyOutputNodes.parray);

		lArrayPtrTopologyOutputNodes.parray = nullptr;



		CComPtrCustom<IUnknown> lVideoSourceNode;

		if (!m_SymbolicLink.empty())
		{
			lhresult = createSourceNode(
				mPtrICaptureManagerControl,
				m_SymbolicLink,
				0,
				mMediaIndex,
				g_RendererActivateTopologyNode,
				&lVideoSourceNode);
		}
		else
		{

			lhresult = createSourceNode(
				mPtrICaptureManagerControl,
				g_CaptureProcessor,
				0,
				mMediaIndex,
				g_RendererActivateTopologyNode,
				&lVideoSourceNode);
		}

		if (FAILED(lhresult))
			break;

		CComPtrCustom<ISession> lSession;


		std::vector<IUnknown*> lSourceNodes;

		lSourceNodes.push_back(lVideoSourceNode.detach());

		lhresult = createSession(
			mPtrICaptureManagerControl,
			lSourceNodes,
			&lSession);

		if (FAILED(lhresult))
			break;

		g_RenderingControl->enableInnerRendering(
			g_RendererActivateTopologyNode,
			aEnableInnerRenderer ? TRUE : FALSE);
		
		if (lSession != nullptr)
		{
			lSession->startSession(0, GUID_NULL);

			mPtrISession = lSession.detach();
		}

	} while (false);

	VariantClear(&lArrayPtrTopologyOutputNodes);
}

void Recorder::startPreviewAndRecording(
	void* aPtrWindow,
	bool aEnableInnerRenderer,
	std::wstring aFilePath)
{
	closeRecorder();

	HRESULT lhresult(E_FAIL);

	VARIANT lArrayPtrTopologyOutputNodes;

	VariantInit(&lArrayPtrTopologyOutputNodes);

	do
	{
		if (mPtrICaptureManagerControl == nullptr)
			break;

		CComPtrCustom<ISinkControl> lSinkControl;

		lhresult = mPtrICaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSinkControl));

		if (FAILED(lhresult))
			break;

		if (!g_RenderingControl)
			lhresult = mPtrICaptureManagerControl->createMisc(
				IID_PPV_ARGSIUnknown(&g_RenderingControl));

		if (FAILED(lhresult))
			break;

		CComPtrCustom<IEVRMultiSinkFactory> lEVRMultiSinkFactory;

		lhresult = lSinkControl->createSinkFactory(
			GUID_NULL,
			IID_PPV_ARGSIUnknown(&lEVRMultiSinkFactory));

		if (FAILED(lhresult))
			break;

		lhresult = lEVRMultiSinkFactory->createOutputNodes(
			nullptr,
			(IUnknown*)aPtrWindow,
			1,
			&lArrayPtrTopologyOutputNodes);

		if (FAILED(lhresult))
			break;
		
		if (lArrayPtrTopologyOutputNodes.vt == VT_SAFEARRAY | VT_UNKNOWN)
		{
			long i = 0;

			VARIANT lVar;

			VariantInit(&lVar);

			lhresult = SafeArrayGetElement(lArrayPtrTopologyOutputNodes.parray, &i, &lVar);

			if (FAILED(lhresult))
				continue;

			if (lVar.punkVal != nullptr)
				g_RendererActivateTopologyNode = lVar.punkVal;

			VariantClear(&lVar);
		}

		SafeArrayDestroy(lArrayPtrTopologyOutputNodes.parray);

		lArrayPtrTopologyOutputNodes.parray = nullptr;




		//	 Create MediaType for selected Video Source

		CComPtrCustom<IUnknown> lVideoSourceMediaType;


		if (!m_SymbolicLink.empty())
		{

			lhresult = createSourceMediaType(
				mPtrICaptureManagerControl,
				m_SymbolicLink,
				0,
				mMediaIndex,
				&lVideoSourceMediaType);
		}
		else
		{

			lhresult = createSourceMediaType(
				mPtrICaptureManagerControl,
				g_CaptureProcessor,
				0,
				mMediaIndex,
				&lVideoSourceMediaType);
		}

		if (FAILED(lhresult))
			break;


		// Create MediaType for selected Video Encoder

		CComPtrCustom<IUnknown> lVideoCompressedMediaType;

		if (g_VideoEncoderNodeFactory == nullptr)
			break;

		createCompresssedMediaType(
			lVideoSourceMediaType,
			__uuidof(VaribleBitRateCLSID),
			g_VideoEncoderNodeFactory,
			&lVideoCompressedMediaType);


		// create output nodes

		std::vector<IUnknown*> lCompressedMediaTypes;

		lCompressedMediaTypes.push_back(lVideoCompressedMediaType.detach());

		std::vector<CComPtrCustom<IUnknown>> lOutputNodes;
		
		lhresult = createOutputNodes(
			mPtrICaptureManagerControl,
			lCompressedMediaTypes,
			aFilePath,
			g_FileFormatIID,
			lOutputNodes);

		if (FAILED(lhresult))
			break;



		// Create Video Encoder Node

		CComPtrCustom<IUnknown> lVideoEncoderNode;

		if (g_VideoEncoderNodeFactory == nullptr)
			break;

		lhresult = createEncoderNode(
			lVideoSourceMediaType,
			__uuidof(VaribleBitRateCLSID),
			g_VideoEncoderNodeFactory,
			lOutputNodes[0],
			&lVideoEncoderNode);

		if (FAILED(lhresult))
			break;


		// Create splitter for Video Renderer

		CComPtrCustom<IUnknown> lSpreaderNode;

		lhresult = createSpreaderNode(
			mPtrICaptureManagerControl,
			lVideoEncoderNode,
			g_RendererActivateTopologyNode,
			&lSpreaderNode);

		if (FAILED(lhresult))
			break;


		CComPtrCustom<IUnknown> lVideoSourceNode;
		
		if (!m_SymbolicLink.empty())
		{
			lhresult = createSourceNode(
				mPtrICaptureManagerControl,
				m_SymbolicLink,
				0,
				mMediaIndex,
				lSpreaderNode,
				&lVideoSourceNode);
		}
		else
		{

			lhresult = createSourceNode(
				mPtrICaptureManagerControl,
				g_CaptureProcessor,
				0,
				mMediaIndex,
				lSpreaderNode,
				&lVideoSourceNode);
		}

		if (FAILED(lhresult))
			break;

		CComPtrCustom<ISession> lSession;


		std::vector<IUnknown*> lSourceNodes;

		lSourceNodes.push_back(lVideoSourceNode.detach());

		lhresult = createSession(
			mPtrICaptureManagerControl,
			lSourceNodes,
			&lSession);

		if (FAILED(lhresult))
			break;



		g_RenderingControl->enableInnerRendering(
			g_RendererActivateTopologyNode,
			aEnableInnerRenderer ? TRUE : FALSE);

		if (lSession != nullptr)
		{
			lSession->startSession(0, GUID_NULL);

			mPtrISession = lSession.detach();
		}

	} while (false);

	VariantClear(&lArrayPtrTopologyOutputNodes);
}

void Recorder::renderToTarget(void* aPtrRenderTarget)
{
	do
	{
		if (aPtrRenderTarget == nullptr)
			break;

		if (!g_RenderingControl)
			break;

		if (!g_RendererActivateTopologyNode)
			break;

		g_RenderingControl->renderToTarget(
			g_RendererActivateTopologyNode,
			(IUnknown*)aPtrRenderTarget,
			TRUE);

		

	} while (false);

}


void Recorder::closeRecorder()
{
	if (mPtrISession != nullptr)
	{
		mPtrISession->stopSession();
		mPtrISession->closeSession();
		mPtrISession->Release();
	}

	mPtrISession = nullptr;
}

int initEncoders(
	ICaptureManagerControl* g_CaptureManagerControl,
	std::wstring aVideoEncoderIID,
	std::wstring aAudioEncoderIID,
	std::wstring aFileFormatIID)
{
	HRESULT lhresult(E_FAIL);

	if (g_CaptureManagerControl == nullptr)
		return -1;

	// get IEncoderControl inereface
	CComPtrCustom<IEncoderControl> lEncoderControl;

	lhresult = g_CaptureManagerControl->createControl(
		IID_PPV_ARGSIUnknown(&lEncoderControl));


	// get Video Encoder Node Factory


	GUID VideoEncoderCLSID;

	IIDFromString(aVideoEncoderIID.c_str(), &VideoEncoderCLSID);

	lhresult = lEncoderControl->createEncoderNodeFactory(
		VideoEncoderCLSID, IID_PPV_ARGSIUnknown(&g_VideoEncoderNodeFactory));

	// get Audio Encoder Node Factory


	GUID AudioEncoderCLSID;

	IIDFromString(aAudioEncoderIID.c_str(), &AudioEncoderCLSID);

	lhresult = lEncoderControl->createEncoderNodeFactory(
		AudioEncoderCLSID, IID_PPV_ARGSIUnknown(&g_AudioEncoderNodeFactory));



	IIDFromString(aFileFormatIID.c_str(), &g_FileFormatIID);
	


	//BSTR lXMLString = nullptr;

	//lEncoderControl->getCollectionOfEncoders(&lXMLString);

	//SysFreeString(lXMLString);

	/*


	<Group Title="Video" GUID="{73646976-0000-0010-8000-00AA00389B71}">
	<EncoderFactory Name="H264 Encoder MFT" Title="H264 Encoder MFT" CLSID="{6CA50344-051A-4DED-9779-A43305165E35}" />
	<EncoderFactory Name="Intel® Quick Sync Video H.264 Encoder MFT" Title="Intel® Quick Sync Video H.264 Encoder MFT" CLSID="{4BE8D3C0-0515-4A37-AD55-E4BAE19AF471}" />
	<EncoderFactory Name="WMVideo8 Encoder MFT" Title="WMVideo8 Encoder MFT" CLSID="{7E320092-596A-41B2-BBEB-175D10504EB6}" />
	<EncoderFactory Name="WMVideo9 Encoder MFT" Title="WMVideo9 Encoder MFT" CLSID="{D23B90D0-144F-46BD-841D-59E4EB19DC59}" />
	<EncoderFactory Name="WMVideo9 Screen Encoder MFT" Title="WMVideo9 Screen Encoder MFT" CLSID="{F7FFE0A0-A4F5-44B5-949E-15ED2BC66F9D}" />
	</Group>
	- <Group Title="Audio" GUID="{73647561-0000-0010-8000-00AA00389B71}">
	<EncoderFactory Name="MP3 Encoder ACM Wrapper MFT" Title="MP3 Encoder ACM Wrapper MFT" CLSID="{11103421-354C-4CCA-A7A3-1AFF9A5B6701}" />
	<EncoderFactory Name="Microsoft AAC Audio Encoder MFT" Title="Microsoft AAC Audio Encoder MFT" CLSID="{93AF0C51-2275-45D2-A35B-F2BA21CAED00}" />
	<EncoderFactory Name="Microsoft Dolby Digital Encoder MFT" Title="Microsoft Dolby Digital Encoder MFT" CLSID="{AC3315C9-F481-45D7-826C-0B406C1F64B8}" />
	<EncoderFactory Name="Microsoft MPEG-2 Audio Encoder MFT" Title="Microsoft MPEG-2 Audio Encoder MFT" CLSID="{46A4DD5C-73F8-4304-94DF-308F760974F4}" />
	<EncoderFactory Name="WM Speech Encoder DMO" Title="WM Speech Encoder DMO" CLSID="{1F1F4E1A-2252-4063-84BB-EEE75F8856D5}" />
	<EncoderFactory Name="WMAudio Encoder MFT" Title="WMAudio Encoder MFT" CLSID="{70F598E9-F4AB-495A-99E2-A7C4D3D89ABF}" />
	</Group>


	*/

	g_initEncoder = TRUE;

	return lhresult;
}

void collectMediaInfo(xml_document& lSourcesXmlDoc, std::wstring a_SymbolicLink, std::vector<std::wstring>& a_refMediaInfo)
{	
	
	// find symbolic link for Video MediaInfo
	auto lFindVideoMediaInfo = [&](const xml_node &node)
	{
		bool lresult = false;

		if (lstrcmpW(node.name(), L"MediaType") == 0)
		{
			auto lchild_node = node.first_child();

			std::wstring lMediaInfo;

			while (!lchild_node.empty())
			{
				auto lName = lchild_node.attribute(L"Name");

				std::wstring lnameString(lName.as_string());

				if (lnameString == L"MF_MT_FRAME_SIZE")
				{
					auto lvaluePartsNode = lchild_node.first_child();

					if (!lvaluePartsNode.empty())
					{
						auto lvalueChildNode = lvaluePartsNode.first_child();

						if (!lvalueChildNode.empty())
						{
							lMediaInfo += lvalueChildNode.first_attribute().as_string();

							lMediaInfo += L": ";

							lMediaInfo += lvalueChildNode.first_attribute().next_attribute().as_string();
						}

						lvalueChildNode = lvalueChildNode.next_sibling();

						if (!lvalueChildNode.empty())
						{

							lMediaInfo += L", ";

							lMediaInfo += lvalueChildNode.first_attribute().as_string();

							lMediaInfo += L": ";

							lMediaInfo += lvalueChildNode.first_attribute().next_attribute().as_string();
						}
					}

				}
				else if (lnameString == L"MF_MT_FRAME_RATE")
				{					
					auto lvalueChildNode = lchild_node.first_child();

					if (!lvalueChildNode.empty())
					{
						lMediaInfo += L", Frame rate: ";
						
						lMediaInfo += lvalueChildNode.first_attribute().as_string();
					}
				}
				
				lchild_node = lchild_node.next_sibling();
			}

			if (!lMediaInfo.empty())
				a_refMediaInfo.push_back(lMediaInfo);
		}

		return lresult;
	};

	// find symbolic link for Video Stream Descriptor
	auto lFindVideoStreamDescriptor = [&](const xml_node &node)
	{
		bool lresult = false;

		if (lstrcmpW(node.name(), L"StreamDescriptor") == 0)
		{

			auto lIndex = node.attribute(L"Index");

			std::wstring j(lIndex.as_string());

			if (!lIndex.empty() && (j == L"0"))
			{
				node.find_node(lFindVideoMediaInfo);

				lresult = true;
			}
		}

		return lresult;
	};

	// find symbolic link for Video Source
	auto lFindVideoSymbolicLink = [&](const xml_node &node)
	{
		bool lresult = false;
		
		if (lstrcmpW(node.name(), L"Source.Attributes") == 0)
		{

			// name 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' has only video source
			xml_node lVideoSymbolicLinkAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK");

			if (!lVideoSymbolicLinkAttrNode.empty())
			{
				auto lFirstValueAttrNode = lVideoSymbolicLinkAttrNode.first_child().attribute(L"Value");

				if (!lFirstValueAttrNode.empty() && lFirstValueAttrNode.as_string() == a_SymbolicLink)
				{
					lresult = true;
				}
				
			}
		}

		return lresult;
	};

	// find first Video Source
	auto lFindFirstVideoSource = [lFindVideoSymbolicLink, lFindVideoStreamDescriptor](const xml_node &node)
	{
		bool lresult = false;

		if ((lstrcmpW(node.name(), L"Source") == 0))
		{
			xml_node lSourceNode = node.find_node(lFindVideoSymbolicLink);

			if (!lSourceNode.empty())
				node.find_node(lFindVideoStreamDescriptor);
		}

		return lresult;
	};

	xml_node lVideoSourceXMLNode = lSourcesXmlDoc.find_node(lFindFirstVideoSource);
}

HRESULT createSourceNode(
	ICaptureManagerControl* g_CaptureManagerControl,
	const std::wstring& aRefSymbolicLink,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown* aPtrUnkEncoderNode,
	IUnknown** aPtrPtrUnkSourceNode)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrPtrUnkSourceNode == nullptr)
			break;

		if (aPtrUnkEncoderNode == nullptr)
			break;

		if (aRefSymbolicLink.empty())
			break;

		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			return lhresult;

		BSTR lSymbolicLink = SysAllocString(aRefSymbolicLink.c_str());

		lhresult = lSourceControl->createSourceNodeWithDownStreamConnection(
			lSymbolicLink,
			aStreamIndex,
			aMediaTypeIndex,
			aPtrUnkEncoderNode,
			aPtrPtrUnkSourceNode);

		SysFreeString(lSymbolicLink);

		if (FAILED(lhresult))
			return lhresult;

	} while (false);

	return lhresult;
}

HRESULT createSourceNode(
	ICaptureManagerControl* g_CaptureManagerControl,
	IUnknown* a_UnkCaptureProcessor,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown* aPtrUnkEncoderNode,
	IUnknown** aPtrPtrUnkSourceNode)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrPtrUnkSourceNode == nullptr)
			break;

		if (aPtrUnkEncoderNode == nullptr)
			break;

		if (a_UnkCaptureProcessor == nullptr)
			break;

		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			return lhresult;

		CComPtrCustom<IUnknown> lMediaSource;

		lhresult = lSourceControl->createSourceFromCaptureProcessor(a_UnkCaptureProcessor, &lMediaSource);

		if (FAILED(lhresult))
			return lhresult;


		lhresult = lSourceControl->createSourceNodeFromExternalSourceWithDownStreamConnection(
			lMediaSource,
			aStreamIndex,
			aMediaTypeIndex,
			aPtrUnkEncoderNode,
			aPtrPtrUnkSourceNode);
		
		if (FAILED(lhresult))
			return lhresult; 

	} while (false);

	return lhresult;
}

HRESULT createSession(
	ICaptureManagerControl* g_CaptureManagerControl,
	std::vector<IUnknown*> aSourceNodes,
	ISession** aPtrPtrSession)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aSourceNodes.empty())
			break;

		if (g_CaptureManagerControl == nullptr)
			break;

		if (aPtrPtrSession == nullptr)
			break;

		// get ISessionControl inetrface
		CComPtrCustom<ISessionControl> lSessionControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSessionControl));

		if (FAILED(lhresult))
			return lhresult;



		SAFEARRAY* pSA = NULL;
		SAFEARRAYBOUND bound[1];
		bound[0].lLbound = 0;
		bound[0].cElements = aSourceNodes.size();
		pSA = SafeArrayCreate(VT_VARIANT, 1, bound);


		long i = 0;

		for (auto& litem : aSourceNodes)
		{
			VARIANT lVar;

			VariantInit(&lVar);

			lVar.vt = VT_UNKNOWN;

			lVar.punkVal = litem;

			lhresult = SafeArrayPutElement(pSA, &i, &lVar);

			VariantClear(&lVar);

			if (FAILED(lhresult))
				return lhresult;

			++i;
		}
		
		if (FAILED(lhresult))
			return lhresult;

		VARIANT theSourceNodeArray;

		VariantInit(&theSourceNodeArray);

		theSourceNodeArray.vt = VT_SAFEARRAY | VT_UNKNOWN;

		theSourceNodeArray.parray = pSA;

		lhresult = lSessionControl->createSession(
			theSourceNodeArray,
			IID_PPV_ARGSIUnknown(aPtrPtrSession));

		SafeArrayDestroy(pSA);

		VariantClear(&theSourceNodeArray);

		if (FAILED(lhresult))
			return lhresult;


	} while (false);

	return lhresult;
}

HRESULT createSourceMediaType(
	ICaptureManagerControl* g_CaptureManagerControl,
	const std::wstring& aRefSymbolicLink,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown** aPtrPtrSourceMediaType)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrPtrSourceMediaType == nullptr)
			break;

		if (aRefSymbolicLink.empty())
			break;

		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			return lhresult;

		BSTR lSymbolicLink = SysAllocString(aRefSymbolicLink.c_str());

		lhresult = lSourceControl->getSourceOutputMediaType(
			lSymbolicLink,
			aStreamIndex,
			aMediaTypeIndex,
			aPtrPtrSourceMediaType);

		SysFreeString(lSymbolicLink);

		if (FAILED(lhresult))
			return lhresult;

	} while (false);

	return lhresult;
}

HRESULT createSourceMediaType(
	ICaptureManagerControl* g_CaptureManagerControl,
	IUnknown* a_UnkCaptureProcessor,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown** aPtrPtrSourceMediaType)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrPtrSourceMediaType == nullptr)
			break;

		if (a_UnkCaptureProcessor == nullptr)
			break;

		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			return lhresult;

		CComPtrCustom<IUnknown> lMediaSource;

		lhresult = lSourceControl->createSourceFromCaptureProcessor(a_UnkCaptureProcessor, &lMediaSource);

		if (FAILED(lhresult))
			return lhresult;


		lhresult = lSourceControl->getSourceOutputMediaTypeFromMediaSource(
			lMediaSource,
			aStreamIndex,
			aMediaTypeIndex,
			aPtrPtrSourceMediaType);
		
		if (FAILED(lhresult))
			return lhresult;

	} while (false);

	return lhresult;
}

HRESULT createCompresssedMediaType(
	IUnknown* aPtrSourceMediaType,
	REFGUID aRefEncodingModeGUID,
	IEncoderNodeFactory* aPtrEncoderNodeFactory,
	IUnknown** aPtrPtrUnkCompresssedMediaType)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrEncoderNodeFactory == nullptr)
			break;

		if (aPtrPtrUnkCompresssedMediaType == nullptr)
			break;

		if (aPtrSourceMediaType == nullptr)
			break;

		lhresult = aPtrEncoderNodeFactory->createCompressedMediaType(
			aPtrSourceMediaType,
			aRefEncodingModeGUID,
			50,
			0,
			aPtrPtrUnkCompresssedMediaType);

	} while (false);

	return lhresult;
}

HRESULT createOutputNodes(
	ICaptureManagerControl* g_CaptureManagerControl,
	std::vector<IUnknown*> aCompressedMediaTypes,
	std::wstring& aRefFileName,
	REFIID a_RefFileFormat,
	std::vector<CComPtrCustom<IUnknown>>& aOutputNodes)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aCompressedMediaTypes.empty())
			break;
		if (g_CaptureManagerControl == nullptr)
			break;

		// get ISinkControl inetrface
		CComPtrCustom<ISinkControl> lSinkControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSinkControl));

		if (FAILED(lhresult))
			return lhresult;

		CComPtrCustom<IFileSinkFactory> lFileSinkFactory;

		lhresult = lSinkControl->createSinkFactory(
			a_RefFileFormat,
			IID_PPV_ARGSIUnknown(&lFileSinkFactory));

		if (FAILED(lhresult))
			return lhresult;



		SAFEARRAY* pSA = NULL;
		SAFEARRAYBOUND bound[1];
		bound[0].lLbound = 0;
		bound[0].cElements = aCompressedMediaTypes.size();
		pSA = SafeArrayCreate(VT_VARIANT, 1, bound);

		long i = 0;

		for (auto& litem : aCompressedMediaTypes)
		{
			VARIANT lVar;

			VariantInit(&lVar);

			lVar.vt = VT_UNKNOWN;

			lVar.punkVal = litem;

			lhresult = SafeArrayPutElement(pSA, &i, &lVar);

			++i;

			VariantClear(&lVar);

			if (FAILED(lhresult))
				return lhresult;
		}

		if (FAILED(lhresult))
			return lhresult;
				
		VARIANT theCompressedMediaTypeArray;

		VariantInit(&theCompressedMediaTypeArray);

		theCompressedMediaTypeArray.vt = VT_SAFEARRAY | VT_UNKNOWN;

		theCompressedMediaTypeArray.parray = pSA;


		BSTR lFileName = SysAllocString(aRefFileName.c_str());

		VARIANT theOutputNodeArray;

		VariantInit(&theOutputNodeArray);

		lhresult = lFileSinkFactory->createOutputNodes(
			theCompressedMediaTypeArray,
			lFileName,
			&theOutputNodeArray);

		SysFreeString(lFileName);

		SafeArrayDestroy(pSA);

		VariantClear(&theCompressedMediaTypeArray);

		if (FAILED(lhresult))
			return lhresult;

		if (theOutputNodeArray.vt == VT_SAFEARRAY | VT_UNKNOWN)
		{

			for (long i = 0; i < aCompressedMediaTypes.size(); i++)
			{
				VARIANT lVar;

				VariantInit(&lVar);

				lhresult = SafeArrayGetElement(theOutputNodeArray.parray, &i, &lVar);

				if (FAILED(lhresult))
					return lhresult;

				CComPtrCustom<IUnknown> lOutputNode;

				if (lVar.punkVal != nullptr)
					lVar.punkVal->QueryInterface(IID_PPV_ARGS(&lOutputNode));

				aOutputNodes.push_back(lOutputNode);

				VariantClear(&lVar);
			}				

		}

		SafeArrayDestroy(theOutputNodeArray.parray);

		VariantClear(&theOutputNodeArray);

	} while (false);

	return lhresult;
}

HRESULT createEncoderNode(
	IUnknown* aPtrSourceMediaType,
	REFGUID aRefEncodingModeGUID,
	IEncoderNodeFactory* aPtrEncoderNodeFactory,
	IUnknown* aPtrUnkOutputNode,
	IUnknown** aPtrPtrUnkEncoderNode)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrEncoderNodeFactory == nullptr)
			break;

		if (aPtrPtrUnkEncoderNode == nullptr)
			break;

		if (aPtrSourceMediaType == nullptr)
			break;

		if (aPtrUnkOutputNode == nullptr)
			break;

		lhresult = aPtrEncoderNodeFactory->createEncoderNode(
			aPtrSourceMediaType,
			aRefEncodingModeGUID,
			50,
			0,
			aPtrUnkOutputNode,
			aPtrPtrUnkEncoderNode);

	} while (false);

	return lhresult;
}

HRESULT createSpreaderNode(
	ICaptureManagerControl* g_CaptureManagerControl,
	IUnknown* aPtrUnkVideoEncoderNode,
	IUnknown* aPtrUnkVideoRendererNode,
	IUnknown** aPtrPtrUnkSplitterNode)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrUnkVideoEncoderNode == nullptr)
			break;

		if (aPtrUnkVideoRendererNode == nullptr)
			break;

		if (aPtrPtrUnkSplitterNode == nullptr)
			break;


		// get IStreamControl inetrface
		CComPtrCustom<IStreamControl> lStreamControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lStreamControl));

		if (FAILED(lhresult))
			return lhresult;

		CComPtrCustom<ISpreaderNodeFactory> lSpreaderNodeFactory;

		lhresult = lStreamControl->createStreamControlNodeFactory(
			IID_PPV_ARGSIUnknown(&lSpreaderNodeFactory));

		if (FAILED(lhresult))
			return lhresult;





		SAFEARRAY* pSA = NULL;
		SAFEARRAYBOUND bound[1];
		bound[0].lLbound = 0;
		bound[0].cElements = 2;
		pSA = SafeArrayCreate(VT_VARIANT, 1, bound);


		VARIANT lVar;

		VariantInit(&lVar);

		lVar.vt = VT_UNKNOWN;

		lVar.punkVal = aPtrUnkVideoEncoderNode;

		long i = 0;

		lhresult = SafeArrayPutElement(pSA, &i, &lVar);

		if (FAILED(lhresult))
			return lhresult;

		VariantInit(&lVar);

		lVar.vt = VT_UNKNOWN;

		lVar.punkVal = aPtrUnkVideoRendererNode;

		i = 1;

		lhresult = SafeArrayPutElement(pSA, &i, &lVar);

		if (FAILED(lhresult))
			return lhresult;

		VARIANT theOutNodeArray;

		VariantInit(&theOutNodeArray);

		theOutNodeArray.vt = VT_SAFEARRAY | VT_UNKNOWN;

		theOutNodeArray.parray = pSA;

		lhresult = lSpreaderNodeFactory->createSpreaderNode(
			theOutNodeArray,
			aPtrPtrUnkSplitterNode);

		SafeArrayDestroy(pSA);

		VariantClear(&theOutNodeArray);

		if (FAILED(lhresult))
			return lhresult;


	} while (false);

	return lhresult;
}