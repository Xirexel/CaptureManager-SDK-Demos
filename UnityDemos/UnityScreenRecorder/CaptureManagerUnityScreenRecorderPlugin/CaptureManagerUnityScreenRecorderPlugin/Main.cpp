#include "PlatformBase.h"
#include <Unknwnbase.h>
#include <memory>
#include <d3d9.h>
#include <d3d11.h>


#include "../Unity/IUnityGraphics.h"
#include "../Unity/IUnityGraphicsD3D11.h"
#include "RecorderFactory.h"
#include "IRecorder.h"
#include "../Common/ComPtrCustom.h"
#include "CaptureProcessor.h"


// --------------------------------------------------------------------------
// UnitySetInterfaces

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static ID3D11Device* s_Device = NULL;

static RecorderFactory g_RecorderFactory;

std::unique_ptr<IRecorder> g_Recorder;

CComPtrCustom<ID3D11Texture2D> g_StagingTexture;

CComPtrCustom<ICaptureProcessorControl> g_ICaptureProcessorControl;

IUnknown* g_ptrBaseUnknown = nullptr;

IUnknown* g_ptrCaptureBaseUnknown = nullptr;


extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	// Create graphics API implementation upon initialization
	if (eventType == kUnityGfxDeviceEventInitialize)
	{
		IUnityGraphicsD3D11* d3d = s_UnityInterfaces->Get<IUnityGraphicsD3D11>();
		s_Device = d3d->GetDevice();	
	}
	
	// Cleanup graphics API implementation upon shutdown
	if (eventType == kUnityGfxDeviceEventShutdown)
	{
		g_StagingTexture.Release();
	}
}

extern "C" BSTR UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getCollectionOfSources()
{
	return g_RecorderFactory.getSourceXML();
}

extern "C" BSTR UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getCollectionOfEncoders()
{
	return g_RecorderFactory.getEncoderXML();
}

extern "C" BSTR UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getCollectionOfFileFormats()
{
	return g_RecorderFactory.getFileFormatXML();
}

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API startCaptureProcessor(
	int aStreamIndex,
	int aMediaTypeIndex,
	void* texture,
	void* capture,
	BSTR aVideoEncoderIID,
	BSTR aFileFormat,
	BSTR aFileName,
	BOOL a_Recording)
{
	if (g_Recorder)
		g_Recorder.reset();

	if (texture == nullptr)
		return;

	if (s_Device == nullptr)
		return;

	//CComPtrCustom<ID3D11DeviceContext> lID3D11DeviceContext;

	//s_Device->GetImmediateContext(&lID3D11DeviceContext);

	//ID3D11RenderTargetView** lViews = nullptr;

	//lID3D11DeviceContext->OMGetRenderTargets(0, lViews, nullptr);

	//CComPtrCustom<ID3D11Resource> lID3D11Resource;

	//lViews[0]->GetResource(&lID3D11Resource);

	//lViews[0]->Release();

	CComPtrCustom<ID3D11Texture2D> lTexture;

	auto lhres = ((IUnknown*)capture)->QueryInterface(IID_PPV_ARGS(&lTexture));

	if (FAILED(lhres))
		return;


	CComPtrCustom<IUnknown> lUnkCaptureProcessor;

	lhres = CaptureProcessor::create(&lUnkCaptureProcessor, lTexture);

	if (FAILED(lhres))
		return;

	g_ICaptureProcessorControl.Release();

	lhres = lUnkCaptureProcessor->QueryInterface(IID_PPV_ARGS(&g_ICaptureProcessorControl));

	if (FAILED(lhres))
		return;

	g_ptrCaptureBaseUnknown = (IUnknown*)capture;

	

	g_Recorder.reset(g_RecorderFactory.createRecorder(lUnkCaptureProcessor,
		std::wstring(aVideoEncoderIID),
		std::wstring(aVideoEncoderIID),
		std::wstring(aFileFormat)));

	if (g_Recorder)
	{
		g_Recorder->setMediaInfoIndex(aMediaTypeIndex);

		try
		{
			CComPtrCustom<IDirect3DSurface9> lSurface;

			g_ptrBaseUnknown = (IUnknown*)texture;

			auto lresult = g_ptrBaseUnknown->QueryInterface(IID_PPV_ARGS(&lSurface));

			if (FAILED(lresult))
			{
				CComPtrCustom<IDirect3DTexture9> lTexture;

				lresult = g_ptrBaseUnknown->QueryInterface(IID_PPV_ARGS(&lTexture));

				if (SUCCEEDED(lresult))
				{
					lresult = lTexture->GetSurfaceLevel(0, &lSurface);
				}
			}

			if (a_Recording == FALSE)
			{
				if (lSurface)
					g_Recorder->startPreview(lSurface, false);
				else
					g_Recorder->startPreview(g_ptrBaseUnknown, false);
			}
			else
			{

				if (lSurface)
					g_Recorder->startPreviewAndRecording(lSurface, false, aFileName);
				else
					g_Recorder->startPreviewAndRecording(g_ptrBaseUnknown, false, aFileName);
			}

			ID3D11Texture2D* a_CaptureTexture = (ID3D11Texture2D*)g_ptrCaptureBaseUnknown;

			D3D11_TEXTURE2D_DESC lDesc = { 0 };
			a_CaptureTexture->GetDesc(&lDesc);

			lDesc.Usage = D3D11_USAGE_STAGING;
			lDesc.BindFlags = 0;
			lDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
			lDesc.MiscFlags = 0;


			CComPtrCustom<ID3D11Device> lDevice;

			a_CaptureTexture->GetDevice(&lDevice);

			g_StagingTexture.Release();

			lDevice->CreateTexture2D(&lDesc, NULL, &g_StagingTexture);
		}
		catch (...)
		{

		}

	}

}


extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API stopCapture()
{
	if (g_Recorder)
	{
		g_Recorder->closeRecorder();

		g_Recorder.reset();
	}

	g_StagingTexture.Release();
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	if (g_Recorder)
		g_Recorder->renderToTarget(g_ptrBaseUnknown);

	ID3D11Texture2D* a_CaptureTexture = (ID3D11Texture2D*)g_ptrCaptureBaseUnknown;
	

	CComPtrCustom<ID3D11Device> lDevice;

	a_CaptureTexture->GetDevice(&lDevice);
	
	CComPtrCustom<ID3D11DeviceContext> lID3D11DeviceContext;

	lDevice->GetImmediateContext(&lID3D11DeviceContext);

	lID3D11DeviceContext->CopyResource(g_StagingTexture, a_CaptureTexture);

	D3D11_MAPPED_SUBRESOURCE mapped = { 0 };
	lID3D11DeviceContext->Map(g_StagingTexture, 0, D3D11_MAP_READ, 0, &mapped);

	g_ICaptureProcessorControl->fillBackBuffer((BYTE*)mapped.pData, mapped.RowPitch);

	lID3D11DeviceContext->Unmap(g_StagingTexture, 0);

	g_ICaptureProcessorControl->swapBuffers();

}


// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}