// EVRVieweingAndRecording.cpp : Defines the entry point for the console application.
//

#define WIN32_LEAN_AND_MEAN    

#include "stdafx.h"


#include <windows.h>
#include <Commdlg.h>

#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>
#include <gl/gl.h>
#include <memory>
#include <vector>
#include <atomic>
#include <Unknwnbase.h>
#include <fstream>
#include <iostream>
#include <map>
#include "Common/ComPtrCustom.h"
#include "Common/pugixml.hpp"
#include "Common/CaptureManagerTypeInfo.h"
#include "Common/CaptureManagerLoader.h"

#define IID_PPV_ARGSIUnknown(ppType) __uuidof(**(ppType)), (IUnknown**)(ppType)


CLSID VideoEncoderCLSID;

CLSID AudioEncoderCLSID;

GUID VideoEncoderModeCLSID;

GUID AudioEncoderModeCLSID;


GUID SinkCLSID;


using namespace pugi;

struct SourceData
{
	std::wstring m_SymbolicLink;

	std::wstring m_FriendlyName;
};

std::vector<SourceData> g_VideoSources;

std::vector<SourceData> g_AudioSources;


int g_VideoSourceIndex = 0;

int g_AudioSourceIndex = 0;

std::wstring g_fileName;

BOOL bQuit = FALSE;


class SessionCallback : public ISessionCallback
{
public:

	SessionCallback() :
		mRefCount(1)
	{}

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE invoke(
		/* [in] */ DWORD aCallbackEventCode,
		/* [in] */ DWORD aSessionDescriptor)
	{
		switch (aCallbackEventCode)
		{
		case SessionCallbackEventCode::Error:
		{}
			break;
		case SessionCallbackEventCode::Status_Error:
		{
			bQuit = TRUE;
		}
			break;
		case SessionCallbackEventCode::Execution_Error:
		{}
			break;
		case SessionCallbackEventCode::ItIsReadyToStart:
		{}
			break;
		case SessionCallbackEventCode::ItIsStarted:
		{}
			break;
		case SessionCallbackEventCode::ItIsPaused:
		{}
			break;
		case SessionCallbackEventCode::ItIsStopped:
		{}
			break;
		case SessionCallbackEventCode::ItIsEnded:
		{}
			break;
		case SessionCallbackEventCode::ItIsClosed:
		{}
			break;
		case SessionCallbackEventCode::VideoCaptureDeviceRemoved:
		{}
			break;

		case SessionCallbackEventCode::UnknownEvent:
		default:
		{}
			break;
		}

		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, _COM_Outptr_ void __RPC_FAR *__RPC_FAR *ppvObject)
	{
		HRESULT lhresult = E_NOINTERFACE;

		do
		{
			if (ppvObject == NULL)
			{
				lhresult = E_POINTER;

				break;
			}

			lhresult = S_OK;

			if (riid == IID_IUnknown)
			{
				*ppvObject = static_cast<IUnknown*>(this);

				break;
			}
			else if (riid == __uuidof(ISessionCallback))
			{
				*ppvObject = static_cast<ISessionCallback*>(this);

				break;
			}

			*ppvObject = NULL;

			lhresult = E_NOINTERFACE;

		} while (false);

		if (SUCCEEDED(lhresult))
			AddRef();

		return lhresult;
	}

	virtual ULONG STDMETHODCALLTYPE AddRef(void)
	{
		return ++mRefCount;
	}

	virtual ULONG STDMETHODCALLTYPE Release(void)
	{
		ULONG lCount = --mRefCount;

		if (lCount == 0)
		{
			delete this;
		}
		return lCount;
	}

private:

	std::atomic<ULONG> mRefCount;

	virtual ~SessionCallback(){}
};

CComPtrCustom<ICaptureManagerControl> g_CaptureManagerControl;

CComPtrCustom<IEncoderNodeFactory> g_VideoEncoderNodeFactory;

CComPtrCustom<IEncoderNodeFactory> g_AudioEncoderNodeFactory;

CComPtrCustom<ISession> g_Session;

void collectSources(xml_document& lSourcesXmlDoc);

int initEncoders();

HRESULT createSourceMediaType(
	const std::wstring& aRefSymbolicLink,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown** aPtrPtrSourceMediaType);

HRESULT createCompresssedMediaType(
	IUnknown* aPtrSourceMediaType,
	REFGUID aRefEncodingModeGUID,
	IEncoderNodeFactory* aPtrEncoderNodeFactory,
	IUnknown** aPtrPtrUnkCompresssedMediaType);

HRESULT createOutputNodes(
	IUnknown* aPtrUnkVideoCompressedMediaType,
	IUnknown* aPtrUnkAudioCompressedMediaType,
	std::wstring& aRefFileName,
	IUnknown** aPtrPtrUnkVideoOutputNode,
	IUnknown** aPtrPtrUnkAudioOutputNode);

HRESULT createEncoderNode(
	IUnknown* aPtrSourceMediaType,
	REFGUID aRefEncodingModeGUID,
	IEncoderNodeFactory* aPtrEncoderNodeFactory,
	IUnknown* aPtrUnkOutputNode,
	IUnknown** aPtrPtrUnkEncoderNode);

HRESULT createSourceNode(
	const std::wstring& aRefSymbolicLink,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown* aPtrUnkEncoderNode,
	IUnknown** aPtrPtrUnkSourceNode);

HRESULT createSession(
	IUnknown* aPtrUnkVideoSourceNode,
	IUnknown* aPtrUnkAudioSourceNode,
	ISession** aPtrPtrSession);

LRESULT CALLBACK WndProc(
	HWND hWnd,
	UINT message,
	WPARAM wParam,
	LPARAM lParam);

HWND createWindow();

HRESULT createSpreaderNode(
	IUnknown* aPtrUnkVideoEncoderNode,
	IUnknown* aPtrUnkVideoRendererNode,
	IUnknown** aPtrPtrUnkSplitterNode);

void runLoop();

int _tmain(int argc, _TCHAR* argv[])
{

	HRESULT lhresult = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);

	if (FAILED(lhresult))
		return lhresult;

	CComPtrCustom<IClassFactory> lCoLogPrintOut;

	lhresult = CaptureManagerLoader::getInstance().createCalssFactory(__uuidof(CoLogPrintOut), &lCoLogPrintOut);

	if (FAILED(lhresult))
		return lhresult;

	CComPtrCustom<ILogPrintOutControl> lLogPrintOutControl;

	lCoLogPrintOut->LockServer(true);

	lhresult = lCoLogPrintOut->CreateInstance(
		nullptr,
		IID_PPV_ARGS(&lLogPrintOutControl));

	if (FAILED(lhresult))
		return lhresult;

	// set log file for info
	lhresult = lLogPrintOutControl->addPrintOutDestination(
		(DWORD)INFO_LEVEL,
		L"Log.txt");

	// set log file for info
	lhresult = lLogPrintOutControl->addPrintOutDestination(
		(DWORD)ERROR_LEVEL,
		L"Log.txt");

	if (FAILED(lhresult))
		return lhresult;

	/* initialisation CaptureManager */

	CComPtrCustom<IClassFactory> lCoCaptureManager;

	lhresult = CaptureManagerLoader::getInstance().createCalssFactory(__uuidof(CoCaptureManager), &lCoCaptureManager);

	if (FAILED(lhresult))
		return lhresult;

	lCoCaptureManager->LockServer(true);

	// get ICaptureManagerControl interfrace
	lhresult = lCoCaptureManager->CreateInstance(
		nullptr,
		IID_PPV_ARGS(&g_CaptureManagerControl));

	if (FAILED(lhresult))
		return lhresult;



	// Create Video renderer node
	// get ISinkControl inetrface
	CComPtrCustom<ISinkControl> lSinkControl;

	lhresult = g_CaptureManagerControl->createControl(
		IID_PPV_ARGSIUnknown(&lSinkControl));

	if (FAILED(lhresult))
		return lhresult;


	BSTR lXMLstring = nullptr;


	lSinkControl->getCollectionOfSinks(&lXMLstring);

	xml_document lSinksXmlDoc;

	auto k = lSinksXmlDoc.load_string(lXMLstring);

	if (lXMLstring != nullptr)
		SysFreeString(lXMLstring);

	auto lSinkCLSIDNode = lSinksXmlDoc.select_node(L"/SinkFactories/SinkFactory[@GUID='{D6E342E3-7DDD-4858-AB91-4253643864C2}']/Value.ValueParts/ValuePart");

	if (lSinkCLSIDNode.node().empty())
		return -1;

	std::wstring lfileExt(lSinkCLSIDNode.node().attribute(L"Value").as_string());


	lhresult = IIDFromString(lSinkCLSIDNode.node().attribute(L"GUID").as_string(),
		&SinkCLSID);

	// get IEncoderControl inereface
	CComPtrCustom<IEncoderControl> lEncoderControl;

	lhresult = g_CaptureManagerControl->createControl(
		IID_PPV_ARGSIUnknown(&lEncoderControl));

	if (FAILED(lhresult))
		return lhresult;

	// get ISourceControl inetrface
	CComPtrCustom<ISourceControl> lSourceControl;

	lhresult = g_CaptureManagerControl->createControl(
		IID_PPV_ARGSIUnknown(&lSourceControl));

	if (FAILED(lhresult))
		return lhresult;

	BSTR lXMLString = nullptr;

	lhresult = lSourceControl->getCollectionOfSources(&lXMLString);

	if (FAILED(lhresult))
		return lhresult;


	xml_document lSourcesXmlDoc;

	lSourcesXmlDoc.load_string(lXMLString);


	// Get source collection.
	collectSources(lSourcesXmlDoc);

	SysFreeString(lXMLString);

	std::wcout << std::endl << L"Select Video source:" << std::endl;

	int lIndex = 0;

	for (auto& lItem : g_VideoSources)
	{
		std::wcout << ++lIndex << L" " << lItem.m_FriendlyName << std::endl;
	}

	std::wcout << std::endl << L"Enter number: ";

	std::cin >> g_VideoSourceIndex;

	if (g_VideoSourceIndex == 0)
		return -1;



	std::wcout << std::endl << L"Select Audio source:" << std::endl;

	lIndex = 0;

	for (auto& lItem : g_AudioSources)
	{
		std::wcout << ++lIndex << L" " << lItem.m_FriendlyName << std::endl;
	}

	std::wcout << std::endl << L"Enter number: ";

	std::cin >> g_AudioSourceIndex;

	if (g_AudioSourceIndex == 0)
		return -1;

	std::wcout << std::endl << L"Enter name of output file (with ." << lfileExt << " ext): ";

	std::wcin >> g_fileName;

	initEncoders();

	// Create MediaType for selected Video Source

	CComPtrCustom<IUnknown> lVideoSourceMediaType;

	if ((g_VideoSourceIndex - 1) < 0)
		return -1;

	createSourceMediaType(
		g_VideoSources[g_VideoSourceIndex - 1].m_SymbolicLink,
		0,
		0,
		&lVideoSourceMediaType);



	// Create MediaType for selected Audio Source

	CComPtrCustom<IUnknown> lAudioSourceMediaType;

	if ((g_AudioSourceIndex - 1) < 0)
		return -1;

	createSourceMediaType(
		g_AudioSources[g_AudioSourceIndex - 1].m_SymbolicLink,
		0,
		0,
		&lAudioSourceMediaType);





	// Create MediaType for selected Video Encoder

	CComPtrCustom<IUnknown> lVideoCompressedMediaType;

	if (g_VideoEncoderNodeFactory == nullptr)
		return -1;

	lXMLstring = nullptr;

	lhresult = lEncoderControl->getMediaTypeCollectionOfEncoder(
		lVideoSourceMediaType,
		VideoEncoderCLSID,
		&lXMLstring);

	xml_document lVideoEncoderMediaTypeXmlDoc;

	k = lVideoEncoderMediaTypeXmlDoc.load_string(lXMLstring);

	if (lXMLstring != nullptr)
		SysFreeString(lXMLstring);

	lXMLstring = nullptr;

	auto lvideoEncoderModeAttr = lVideoEncoderMediaTypeXmlDoc.select_node(L"/EncoderMediaTypes/Group/@GUID");


	lhresult = IIDFromString(lvideoEncoderModeAttr.attribute().as_string(),
		&VideoEncoderModeCLSID);



	createCompresssedMediaType(
		lVideoSourceMediaType,
		VideoEncoderModeCLSID,
		g_VideoEncoderNodeFactory,
		&lVideoCompressedMediaType);



	// Create MediaType for selected Audio Encoder

	CComPtrCustom<IUnknown> lAudioCompressedMediaType;

	if (g_AudioEncoderNodeFactory == nullptr)
		return -1;



	lhresult = lEncoderControl->getMediaTypeCollectionOfEncoder(
		lAudioSourceMediaType,
		AudioEncoderCLSID,
		&lXMLstring);

	xml_document lAudioEncoderMediaTypeXmlDoc;

	k = lAudioEncoderMediaTypeXmlDoc.load_string(lXMLstring);

	if (lXMLstring != nullptr)
		SysFreeString(lXMLstring);

	lXMLstring = nullptr;

	auto laudioEncoderModeAttr = lAudioEncoderMediaTypeXmlDoc.select_node(L"/EncoderMediaTypes/Group/@GUID");


	lhresult = IIDFromString(laudioEncoderModeAttr.attribute().as_string(),
		&AudioEncoderModeCLSID);


	createCompresssedMediaType(
		lAudioSourceMediaType,
		AudioEncoderModeCLSID,
		g_AudioEncoderNodeFactory,
		&lAudioCompressedMediaType);



	// create output nodes

	CComPtrCustom<IUnknown> lVideoOutputNode;

	CComPtrCustom<IUnknown> lAudioOutputNode;

	lhresult = createOutputNodes(
		lVideoCompressedMediaType,
		lAudioCompressedMediaType,
		g_fileName,
		&lVideoOutputNode,
		&lAudioOutputNode);

	if (FAILED(lhresult))
		return lhresult;


	// Create Video Encoder Node

	CComPtrCustom<IUnknown> lVideoEncoderNode;

	if (g_VideoEncoderNodeFactory == nullptr)
		return -1;



	createEncoderNode(
		lVideoSourceMediaType,
		VideoEncoderModeCLSID,
		g_VideoEncoderNodeFactory,
		lVideoOutputNode,
		&lVideoEncoderNode);



	// Create Audio Encoder Node

	CComPtrCustom<IUnknown> lAudioEncoderNode;

	if (g_AudioEncoderNodeFactory == nullptr)
		return -1;

	createEncoderNode(
		lAudioSourceMediaType,
		AudioEncoderModeCLSID,
		g_AudioEncoderNodeFactory,
		lAudioOutputNode,
		&lAudioEncoderNode);



	CComPtrCustom<IEVRSinkFactory> lEVRSinkFactory;

	lhresult = lSinkControl->createSinkFactory(
		GUID_NULL,
		IID_PPV_ARGSIUnknown(&lEVRSinkFactory));

	if (FAILED(lhresult))
		return lhresult;

	HWND lHWND = createWindow();

	CComPtrCustom<IUnknown> RendererActivateTopologyNode;

	lhresult = lEVRSinkFactory->createOutputNode(
		lHWND,
		&RendererActivateTopologyNode);

	if (FAILED(lhresult))
		return lhresult;


	// Create splitter for Video Renderer

	CComPtrCustom<IUnknown> lSpreaderNode;

	createSpreaderNode(
		lVideoEncoderNode,
		RendererActivateTopologyNode,
		&lSpreaderNode);


	// Create Source node for selected Video Source

	CComPtrCustom<IUnknown> lVideoSourceNode;

	if ((g_VideoSourceIndex - 1) < 0)
		return -1;

	createSourceNode(
		g_VideoSources[g_VideoSourceIndex - 1].m_SymbolicLink,
		0,
		0,
		lSpreaderNode,
		&lVideoSourceNode);



	// Create Source node for selected Audio Source

	CComPtrCustom<IUnknown> lAudioSourceNode;

	if ((g_AudioSourceIndex - 1) < 0)
		return -1;

	createSourceNode(
		g_AudioSources[g_AudioSourceIndex - 1].m_SymbolicLink,
		0,
		0,
		lAudioEncoderNode,
		&lAudioSourceNode);



	// Create Session

	createSession(
		lVideoSourceNode,
		lAudioSourceNode,
		&g_Session);





	CComPtrCustom<IConnectionPointContainer> IConnectionPointContainer;

	lhresult = g_Session->getIConnectionPointContainer(
		IID_PPV_ARGSIUnknown(&IConnectionPointContainer));

	if (FAILED(lhresult))
		return lhresult;

	CComPtrCustom<IConnectionPoint> lConnectionPoint;

	lhresult = IConnectionPointContainer->FindConnectionPoint(
		__uuidof(ISessionCallback),
		&lConnectionPoint);

	if (FAILED(lhresult))
		return lhresult;

	CComPtrCustom<ISessionCallback> lSessionCallback = new SessionCallback();

	DWORD lStreamID;

	lhresult = lConnectionPoint->Advise(
		lSessionCallback,
		&lStreamID);

	if (FAILED(lhresult))
		return lhresult;



	if (g_Session != nullptr)
	{
		g_Session->startSession(0, GUID_NULL);
	}

	runLoop();

	if (g_Session != nullptr)
	{
		g_Session->stopSession();

		g_Session->closeSession();
	}

	lCoCaptureManager->LockServer(false);

	return 0;
}


void collectSources(xml_document& lSourcesXmlDoc)
{
	// find symbolic link for Video Source
	auto lFindVideoSymbolicLink = [](const xml_node &node)
	{
		bool lresult = false;


		if (lstrcmpW(node.name(), L"Source.Attributes") == 0)
		{

			// name 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' has only video source
			xml_node lVideoSymbolicLinkAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK");

			if (!lVideoSymbolicLinkAttrNode.empty())
			{
				xml_node lFriendlyNameAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME");

				if (!lFriendlyNameAttrNode.empty())
				{
					auto lFirstValueAttrNode = lVideoSymbolicLinkAttrNode.first_child().attribute(L"Value");

					auto lSecondValueAttrNode = lFriendlyNameAttrNode.first_child().attribute(L"Value");


					if (!lFirstValueAttrNode.empty() && !lSecondValueAttrNode.empty())
					{
						SourceData lSourceData;

						lSourceData.m_SymbolicLink = lFirstValueAttrNode.as_string();

						lSourceData.m_FriendlyName = lSecondValueAttrNode.as_string();

						g_VideoSources.push_back(lSourceData);
					}

					lresult = true;
				}
			}
			else
			{
				// name 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK' has only audio source
				lVideoSymbolicLinkAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK");

				if (!lVideoSymbolicLinkAttrNode.empty())
				{
					xml_node lFriendlyNameAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME");

					if (!lFriendlyNameAttrNode.empty())
					{
						auto lFirstValueAttrNode = lVideoSymbolicLinkAttrNode.first_child().attribute(L"Value");

						auto lSecondValueAttrNode = lFriendlyNameAttrNode.first_child().attribute(L"Value");


						if (!lFirstValueAttrNode.empty() && !lSecondValueAttrNode.empty())
						{
							SourceData lSourceData;

							lSourceData.m_SymbolicLink = lFirstValueAttrNode.as_string();

							lSourceData.m_FriendlyName = lSecondValueAttrNode.as_string();

							g_AudioSources.push_back(lSourceData);
						}

						lresult = true;
					}
				}
			}
		}

		return lresult;
	};

	// find first Video Source
	auto lFindFirstVideoSource = [lFindVideoSymbolicLink](const xml_node &node)
	{
		bool lresult = false;

		if ((lstrcmpW(node.name(), L"Source") == 0))
		{
			xml_node lAttrNode = node.find_node(lFindVideoSymbolicLink);
		}

		return lresult;
	};

	xml_node lVideoSourceXMLNode = lSourcesXmlDoc.find_node(lFindFirstVideoSource);
}

int initEncoders()
{
	HRESULT lhresult(E_FAIL);

	if (g_CaptureManagerControl == nullptr)
		return -1;

	// get IEncoderControl inereface
	CComPtrCustom<IEncoderControl> lEncoderControl;

	lhresult = g_CaptureManagerControl->createControl(
		IID_PPV_ARGSIUnknown(&lEncoderControl));

	BSTR lXMLstring = nullptr;

	lhresult = lEncoderControl->getCollectionOfEncoders(&lXMLstring);

	xml_document lEncodersXmlDoc;

	auto k = lEncodersXmlDoc.load_string(lXMLstring);

	if (lXMLstring != nullptr)
		SysFreeString(lXMLstring);
	
	auto lvideoEncoderCLSIDNode = lEncodersXmlDoc.select_node(L"/EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory[1]/@CLSID");

	auto laudioEncoderCLSIDNode = lEncodersXmlDoc.select_node(L"/EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory[1]/@CLSID");

	

	lhresult = IIDFromString(lvideoEncoderCLSIDNode.attribute().as_string(),
		&VideoEncoderCLSID);

	// get Video Encoder Node Factory

	lhresult = lEncoderControl->createEncoderNodeFactory(
		VideoEncoderCLSID, IID_PPV_ARGSIUnknown(&g_VideoEncoderNodeFactory));



	lhresult = IIDFromString(laudioEncoderCLSIDNode.attribute().as_string(),
		&AudioEncoderCLSID);

	// get Audio Encoder Node Factory

	lhresult = lEncoderControl->createEncoderNodeFactory(
		AudioEncoderCLSID, IID_PPV_ARGSIUnknown(&g_AudioEncoderNodeFactory));

	
	return lhresult;
}


HRESULT createSourceMediaType(
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
	IUnknown* aPtrUnkVideoCompressedMediaType,
	IUnknown* aPtrUnkAudioCompressedMediaType,
	std::wstring& aRefFileName,
	IUnknown** aPtrPtrUnkVideoOutputNode,
	IUnknown** aPtrPtrUnkAudioOutputNode)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrUnkVideoCompressedMediaType == nullptr)
			break;
		if (aPtrUnkAudioCompressedMediaType == nullptr)
			break;
		if (aPtrPtrUnkVideoOutputNode == nullptr)
			break;
		if (aPtrPtrUnkAudioOutputNode == nullptr)
			break;

		// get ISinkControl inetrface
		CComPtrCustom<ISinkControl> lSinkControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSinkControl));

		if (FAILED(lhresult))
			return lhresult;

		CComPtrCustom<IFileSinkFactory> lFileSinkFactory;
		
		

		lhresult = lSinkControl->createSinkFactory(
			SinkCLSID,
			IID_PPV_ARGSIUnknown(&lFileSinkFactory));

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

		lVar.punkVal = aPtrUnkVideoCompressedMediaType;

		long i = 0;

		lhresult = SafeArrayPutElement(pSA, &i, &lVar);

		if (FAILED(lhresult))
			return lhresult;

		VariantInit(&lVar);

		lVar.vt = VT_UNKNOWN;

		lVar.punkVal = aPtrUnkAudioCompressedMediaType;

		i = 1;

		lhresult = SafeArrayPutElement(pSA, &i, &lVar);

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

		if (theOutputNodeArray.vt == (VT_SAFEARRAY | VT_UNKNOWN))
		{
			i = 0;

			VariantInit(&lVar);

			lhresult = SafeArrayGetElement(theOutputNodeArray.parray, &i, &lVar);

			if (FAILED(lhresult))
				return lhresult;

			if (lVar.punkVal != nullptr)
				lVar.punkVal->QueryInterface(aPtrPtrUnkVideoOutputNode);

			VariantClear(&lVar);


			i = 1;

			VariantInit(&lVar);

			lhresult = SafeArrayGetElement(theOutputNodeArray.parray, &i, &lVar);

			if (FAILED(lhresult))
				return lhresult;

			if (lVar.punkVal != nullptr)
				lVar.punkVal->QueryInterface(aPtrPtrUnkAudioOutputNode);

			VariantClear(&lVar);
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

HRESULT createSourceNode(
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

HRESULT createSession(
	IUnknown* aPtrUnkVideoSourceNode,
	IUnknown* aPtrUnkAudioSourceNode,
	ISession** aPtrPtrSession)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrUnkVideoSourceNode == nullptr)
			break;
		if (aPtrUnkAudioSourceNode == nullptr)
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
		bound[0].cElements = 2;
		pSA = SafeArrayCreate(VT_VARIANT, 1, bound);


		VARIANT lVar;

		VariantInit(&lVar);

		lVar.vt = VT_UNKNOWN;

		lVar.punkVal = aPtrUnkVideoSourceNode;

		long i = 0;

		lhresult = SafeArrayPutElement(pSA, &i, &lVar);

		if (FAILED(lhresult))
			return lhresult;

		VariantInit(&lVar);

		lVar.vt = VT_UNKNOWN;

		lVar.punkVal = aPtrUnkAudioSourceNode;

		i = 1;

		lhresult = SafeArrayPutElement(pSA, &i, &lVar);

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

LRESULT CALLBACK WndProc(
	HWND hWnd,
	UINT message,
	WPARAM wParam,
	LPARAM lParam)
{

	switch (message)
	{
	case WM_CREATE:

		return 0;
	case WM_CLOSE:
		PostQuitMessage(0);
		return 0;

	case WM_DESTROY:
		return 0;

	case WM_KEYDOWN:
		switch (wParam)
		{
		case VK_ESCAPE:
		{
			if (g_Session != nullptr)
			{
				g_Session->stopSession();

				g_Session->closeSession();
			}
		}
			PostQuitMessage(0);
			return 0;
		}
		return 0;

	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}
}

HWND createWindow()
{
	WNDCLASS wc;
	HWND hWnd;

	/* register window class */
	wc.style = CS_OWNDC;
	wc.lpfnWndProc = WndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = NULL;
	wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
	wc.lpszMenuName = NULL;
	wc.lpszClassName = L"EVRVieweingAndRecording";
	RegisterClass(&wc);

	/* create main window */
	hWnd = CreateWindow(
		L"EVRVieweingAndRecording", L"EVRVieweingAndRecording Sample",
		WS_OVERLAPPEDWINDOW | WS_VISIBLE,
		//WS_CAPTION | WS_POPUPWINDOW | WS_VISIBLE,
		0, 0, 800, 400,
		NULL, NULL, NULL, NULL);

	return hWnd;
}

HRESULT createSpreaderNode(
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

void runLoop()
{
	MSG msg;

	/* program main loop */
	while (!bQuit)
	{
		/* check for messages */
		if (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
		{
			/* handle or dispatch messages */
			if (msg.message == WM_QUIT)
			{
				bQuit = TRUE;
			}
			else
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}

		Sleep(200);
	}

}

