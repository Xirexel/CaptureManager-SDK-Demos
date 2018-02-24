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

IUnknown* g_ptrBaseUnknown = nullptr;


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

extern "C" BSTR	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getEncoderMediaTypes(
	BSTR aSymbolicLink,
	int aStreamIndex,
	int aMediaTypeIndex,
	BSTR aVideoEncoderIID)
{
	return g_RecorderFactory.getEncoderMediaTypes(aSymbolicLink, aStreamIndex, aMediaTypeIndex, aVideoEncoderIID);
}

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API startRecording(
	BSTR aVideoSymbolicLink,
	int aVideoStreamIndex,
	int aVideoMediaTypeIndex,
	BSTR aAudioSymbolicLink,
	int aAudioStreamIndex,
	int aAudioMediaTypeIndex,
	BSTR aVideoEncoderIID,
	BSTR aVideoEncoderModeIID,
	int aVideoCompressedMediaTypeIndex,
	BSTR aAudioEncoderIID,
	BSTR aAudioEncoderModeIID,
	int aAudioCompressedMediaTypeIndex,
	BSTR aFileFormatIID,
	BSTR aFileName,
	void* texture)
{
	if (g_Recorder)
		g_Recorder.reset();

	if (texture == nullptr)
		return;


	g_Recorder.reset(g_RecorderFactory.createRecorder(
		std::wstring(aVideoSymbolicLink),
		std::wstring(aAudioSymbolicLink),
		std::wstring(aVideoEncoderIID),
		std::wstring(aAudioEncoderIID),
		std::wstring(aFileFormatIID)));

	if (g_Recorder)
	{
		g_Recorder->setSourceMediaInfoIndex(
			aVideoStreamIndex,
			aVideoMediaTypeIndex,
			aAudioStreamIndex,
			aAudioMediaTypeIndex);


		g_Recorder->setCompressionMediaInfoIndex(
			std::wstring(aVideoEncoderModeIID),
			aVideoCompressedMediaTypeIndex,
			std::wstring(aAudioEncoderModeIID),
			aAudioCompressedMediaTypeIndex);


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
			
			if (lSurface)
				g_Recorder->startPreviewAndRecording(lSurface, false, aFileName);
			else
				g_Recorder->startPreviewAndRecording(g_ptrBaseUnknown, false, aFileName);
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
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	if (g_Recorder)
		g_Recorder->renderToTarget(g_ptrBaseUnknown);
}


// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}