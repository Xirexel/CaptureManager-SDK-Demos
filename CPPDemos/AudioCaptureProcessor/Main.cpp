// AudioCaptureProcessor.cpp : Defines the entry point for the console application.
//

#define WIN32_LEAN_AND_MEAN    

#include <SDKDDKVer.h>

#include <stdio.h>
#include <tchar.h>


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

#include "AudioCaptureProcessor.h"

#define IID_PPV_ARGSIUnknown(ppType) __uuidof(**(ppType)), (IUnknown**)(ppType)



CLSID AudioEncoderCLSID;

GUID AudioEncoderModeCLSID;

GUID SinkCLSID;


using namespace pugi;

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
	IUnknown* aPtrUnkAudioCompressedMediaType,
	std::wstring& aRefFileName,
	IUnknown** aPtrPtrUnkAudioOutputNode);

HRESULT createEncoderNode(
	IUnknown* aPtrSourceMediaType,
	REFGUID aRefEncodingModeGUID,
	IEncoderNodeFactory* aPtrEncoderNodeFactory,
	IUnknown* aPtrUnkOutputNode,
	IUnknown** aPtrPtrUnkEncoderNode);

HRESULT createSourceNode(
	IUnknown* aPtrUnkMediaSource,
	UINT aStreamIndex,
	UINT aMediaTypeIndex,
	IUnknown* aPtrUnkEncoderNode,
	IUnknown** aPtrPtrUnkSourceNode);

HRESULT createSession(
	IUnknown* aPtrUnkAudioSourceNode,
	ISession** aPtrPtrSession);

LRESULT CALLBACK WndProc(
	HWND hWnd,
	UINT message,
	WPARAM wParam,
	LPARAM lParam);

HWND createWindow();

void runLoop();

int _tmain(int argc, _TCHAR* argv[])
{
	g_fileName = L"Audio.asf";


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

	CComPtrCustom<ICaptureProcessor> lAudioCaptureProcessor(new AudioCaptureProcessor());

	


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

	

	initEncoders();

	// Create MediaType for selected Audio Source

	CComPtrCustom<IUnknown> lAudioCaptureMediaType;
	
	CComPtrCustom<IUnknown> lMediaSource;

	do
	{


		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			return lhresult;
		
		lhresult = lSourceControl->createSourceFromCaptureProcessor(lAudioCaptureProcessor, &lMediaSource);
		
		if (FAILED(lhresult))
			return lhresult;

		lhresult = lSourceControl->getSourceOutputMediaTypeFromMediaSource(
			lMediaSource,
			0,
			0,
			&lAudioCaptureMediaType);

		if (FAILED(lhresult))
			return lhresult;

	} while (false);
	

	// Create MediaType for selected Audio Encoder

	CComPtrCustom<IUnknown> lAudioCompressedMediaType;

	if (g_AudioEncoderNodeFactory == nullptr)
		return -1;



	lhresult = lEncoderControl->getMediaTypeCollectionOfEncoder(
		lAudioCaptureMediaType,
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
		lAudioCaptureMediaType,
		AudioEncoderModeCLSID,
		g_AudioEncoderNodeFactory,
		&lAudioCompressedMediaType);



	// create output nodes
	
	CComPtrCustom<IUnknown> lAudioOutputNode;

	lhresult = createOutputNodes(
		lAudioCompressedMediaType,
		g_fileName,
		&lAudioOutputNode);

	if (FAILED(lhresult))
		return lhresult;

	

	// Create Audio Encoder Node

	CComPtrCustom<IUnknown> lAudioEncoderNode;

	if (g_AudioEncoderNodeFactory == nullptr)
		return -1;

	createEncoderNode(
		lAudioCaptureMediaType,
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

	
	
	// Create Source node for selected Audio Source

	CComPtrCustom<IUnknown> lAudioSourceNode;
	
	createSourceNode(
		lMediaSource,
		0,
		0,
		lAudioEncoderNode,
		&lAudioSourceNode);



	// Create Session

	createSession(
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
	
	auto laudioEncoderCLSIDNode = lEncodersXmlDoc.select_node(L"/EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory[1]/@CLSID");


	// get Video Encoder Node Factory
	

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
	IUnknown* aPtrUnkAudioCompressedMediaType,
	std::wstring& aRefFileName,
	IUnknown** aPtrPtrUnkAudioOutputNode)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrUnkAudioCompressedMediaType == nullptr)
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
		bound[0].cElements = 1;
		pSA = SafeArrayCreate(VT_VARIANT, 1, bound);


		VARIANT lVar;

		VariantInit(&lVar);

		lVar.vt = VT_UNKNOWN;

		lVar.punkVal = aPtrUnkAudioCompressedMediaType;

		LONG i = 0;

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
	IUnknown* aPtrMediaSource,
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

		if (aPtrMediaSource == nullptr)
			break;

		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = g_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			return lhresult;
		
		lhresult = lSourceControl->createSourceNodeFromExternalSourceWithDownStreamConnection(
			aPtrMediaSource,
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
	IUnknown* aPtrUnkAudioSourceNode,
	ISession** aPtrPtrSession)
{
	HRESULT lhresult(E_FAIL);

	do
	{
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

		lVar.punkVal = aPtrUnkAudioSourceNode;

		long i = 0;

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
	wc.lpszClassName = L"AudioCaptureProcessor";
	RegisterClass(&wc);

	/* create main window */
	hWnd = CreateWindow(
		L"AudioCaptureProcessor", (L"AudioCaptureProcessor Sample: " + g_fileName).c_str(),
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

