#include "AudioCaptureProcessor.h"
#include <string>
#include <combaseapi.h>


AudioCaptureProcessor::AudioCaptureProcessor():
	mBlockAlign(0)
{
}


AudioCaptureProcessor::~AudioCaptureProcessor()
{
}



HRESULT STDMETHODCALLTYPE AudioCaptureProcessor::start(
	/* [in] */ LONGLONG aStartPositionInHundredNanosecondUnits,
	/* [in] */ REFGUID aGUIDTimeFormat)
{
	HRESULT lresult(E_FAIL);

	do
	{

		CComPtrCustom<IMMDeviceEnumerator> lMMDeviceEnumerator;

		lresult = lMMDeviceEnumerator.CoCreateInstance(__uuidof(MMDeviceEnumerator));

		if (!lMMDeviceEnumerator)
			break;


		lresult = lMMDeviceEnumerator->GetDefaultAudioEndpoint(
			eRender,
			eConsole,
			&mMMDevice);

		if (!mMMDevice)
			break;

		lresult = mMMDevice->Activate(
			__uuidof(IAudioClient),
			CLSCTX_ALL, NULL,
			(void**)&mAudioClient);

		if (!mAudioClient)
			break;

		mAudioClient->GetMixFormat(&mPtrWAVEFORMATEX);

		if (mPtrWAVEFORMATEX != nullptr)
		{
			mBlockAlign = mPtrWAVEFORMATEX->nBlockAlign;

			mSilenceBlockSize = (mPtrWAVEFORMATEX->nSamplesPerSec / 200) * mBlockAlign;

			mSilenceBlock.reset(new BYTE[mSilenceBlockSize]);

			ZeroMemory(mSilenceBlock.get(), mSilenceBlockSize);
		}

		mAudioClient->Initialize(
			AUDCLNT_SHAREMODE_SHARED,
			AUDCLNT_STREAMFLAGS_LOOPBACK,
			0,
			0,
			mPtrWAVEFORMATEX,
			0
			);


		lresult = mAudioClient->GetService(IID_PPV_ARGS(&mAudioCaptureClient));

		if (!mAudioCaptureClient)
			break;
			
		lresult = mAudioClient->Start();


	} while (false);


	return lresult;
}

HRESULT STDMETHODCALLTYPE AudioCaptureProcessor::stop(void)
{
	if (mAudioClient)
		mAudioClient->Stop();

	return S_OK;
}

HRESULT STDMETHODCALLTYPE AudioCaptureProcessor::pause(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE AudioCaptureProcessor::shutdown(void)
{

	if (mPtrWAVEFORMATEX != nullptr)
		CoTaskMemFree(mPtrWAVEFORMATEX);

	mMMDevice.Release();

	return S_OK;
}

HRESULT STDMETHODCALLTYPE AudioCaptureProcessor::initilaize(
	/* [in] */ IUnknown *aPtrIInitilaizeCaptureSource)
{
	HRESULT lresult(E_FAIL);

	do
	{
		if (aPtrIInitilaizeCaptureSource == nullptr)
			break;
		
		CComQIPtrCustom<IInitilaizeCaptureSource> lInitilaize;

		lInitilaize = aPtrIInitilaizeCaptureSource;

		if (lInitilaize == nullptr)
			break;

		std::wstring lPresentationDescriptor;

		lPresentationDescriptor += L"<?xml version='1.0' encoding='UTF-8'?>";
			lPresentationDescriptor += L"<PresentationDescriptor StreamCount='1'>";
			lPresentationDescriptor += L"<PresentationDescriptor.Attributes Title='Attributes of Presentation'>";
			lPresentationDescriptor += L"<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK' GUID='{58F0AAD8-22BF-4F8A-BB3D-D2C4978C6E2F}' Title='The symbolic link for a audio capture driver.' Description='Contains the unique symbolic link for a audio capture driver.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='ImageCaptureProcessor' />";
			lPresentationDescriptor += L"</Attribute>";
			lPresentationDescriptor += L"<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME' GUID='{60D0E559-52F8-4FA2-BBCE-ACDB34A8EC01}' Title='The display name for a device.' Description='The display name is a human-readable string, suitable for display in a user interface.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='Audio Capture Processor' />";
			lPresentationDescriptor += L"</Attribute>";
			lPresentationDescriptor += L"</PresentationDescriptor.Attributes>";
			lPresentationDescriptor += L"<StreamDescriptor Index='0' MajorType='MFMediaType_Audio' MajorTypeGUID='{73647561-0000-0010-8000-00AA00389B71}'>";
			lPresentationDescriptor += L"<Attribute Name='MF_SD_STREAM_NAME' GUID='{4F1B099D-D314-41E5-A781-7FEFAA4C501F}' Title='The name of a stream.' Description='Contains the name of a stream.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='Audio Capture Processor' />";
			lPresentationDescriptor += L"</Attribute>";
			lPresentationDescriptor += L"<MediaTypes TypeCount='1'>";
			lPresentationDescriptor += L"<MediaType Index='0'>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_MAJOR_TYPE' GUID='{48EBA18E-F8C9-4687-BF11-0A74C9F96A8F}' Title='Major type GUID for a media type.' Description='The major type defines the overall category of the media data.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='MFMediaType_Audio' GUID='{73647561-0000-0010-8000-00AA00389B71}' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_SUBTYPE' GUID='{F7E34C9A-42E8-4714-B74B-CB29D72C35E5}' Title='Subtype GUID for a media type.' Description='The subtype GUID defines a specific media format type within a major type.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='MFAudioFormat_Float' GUID='{00000003-0000-0010-8000-00AA00389B71}' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_AUDIO_NUM_CHANNELS' GUID='{37E48BF5-645E-4C5B-89DE-ADA9E29B696A}' Title='Number of audio channels.' Description='Number of audio channels in an audio media type.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='2' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_AUDIO_SAMPLES_PER_SECOND' GUID='{5fAEEAE7-0290-4C31-9E8A-C534F68D9DBA}' Title='Number of audio samples per second (integer value).' Description='Number of audio samples per second in an audio media type.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='48000' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_AUDIO_BLOCK_ALIGNMENT' GUID='{322DE230-9EEB-43BD-AB7A-FF412251541D}' Title='Block alignment, in bytes.' Description='Block alignment, in bytes, for an audio media type. The block alignment is the minimum atomic unit of data for the audio format.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='8' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_AUDIO_AVG_BYTES_PER_SECOND' GUID='{1AAB75C8-CFEF-451C-AB95-AC034B8E1731}' Title='Average number of bytes per second.' Description='Average number of bytes per second in an audio media type.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='384000' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_AUDIO_BITS_PER_SAMPLE' GUID='{F2DEB57F-40FA-4764-AA33-ED4F2D1FF669}' Title='Number of bits per audio sample.' Description='Number of bits per audio sample in an audio media type.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='32' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_ALL_SAMPLES_INDEPENDENT' GUID='{C9173739-5E56-461C-B713-46FB995CB95F}' Title='Independent of samples.' Description='Specifies for a media type whether each sample is independent of the other samples in the stream.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='True' />";
			lPresentationDescriptor += L"</MediaTypeItem>";
			lPresentationDescriptor += L"<MediaTypeItem Name='CM_DURATION_PERIOD' GUID='{63BCBEBB-03D3-4BF9-B233-AECCAE6E15E8}' Title='Source period duration.' Description='Duration in 100 nanoseconds.'>";
			lPresentationDescriptor +=		L"<SingleValue Value='100000' />";
			lPresentationDescriptor += L"</MediaTypeItem>";			
			lPresentationDescriptor += L"</MediaType>";
			lPresentationDescriptor += L"</MediaTypes>";
			lPresentationDescriptor += L"</StreamDescriptor>";
			lPresentationDescriptor += L"</PresentationDescriptor>";
			
			BSTR lPresentationDescriptorBSTR = SysAllocString(lPresentationDescriptor.c_str());

			lresult = lInitilaize->setPresentationDescriptor(lPresentationDescriptorBSTR);

			SysFreeString(lPresentationDescriptorBSTR);


	} while (false);

	return lresult;
}

HRESULT STDMETHODCALLTYPE AudioCaptureProcessor::setCurrentMediaType(
	/* [in] */ IUnknown *aPtrICurrentMediaType)
{
	return S_OK;
}

int lCount = 0;

HRESULT STDMETHODCALLTYPE AudioCaptureProcessor::sourceRequest(
	/* [in] */ IUnknown *aPtrISourceRequestResult)
{
	HRESULT lresult(E_FAIL);



	UINT32 lNextPacketSize = 0;

	lresult = mAudioCaptureClient->GetNextPacketSize(&lNextPacketSize);

	if (FAILED(lresult))
		lNextPacketSize = 0;

	lresult = S_OK;

	if (lNextPacketSize > 0)
	{
		BYTE* lPtrData;

		UINT32 lNumFramesToRead = 0;

		DWORD lFlags(0);

		lresult = mAudioCaptureClient->GetBuffer(
			&lPtrData,
			&lNumFramesToRead,
			&lFlags,
			NULL,
			NULL
			);

		if (FAILED(lresult))
		{
			lNumFramesToRead = 0;
		}
		else
		{
			if (lFlags & AUDCLNT_BUFFERFLAGS_SILENT)
			{
			}
			else
			{
				if (lNumFramesToRead > 0)
				{

					CComQIPtrCustom<ISourceRequestResult> lISourceRequestResult;

					lISourceRequestResult = aPtrISourceRequestResult;

					if (lISourceRequestResult)
						lISourceRequestResult->setData(lPtrData,
						lNumFramesToRead * mBlockAlign, TRUE);

				}
			}
		}

		lresult = mAudioCaptureClient->ReleaseBuffer(lNumFramesToRead);

		lCount = 0;
	}
	else
	{
		++lCount;

		if (lCount >= 2)
		{

			CComQIPtrCustom<ISourceRequestResult> lISourceRequestResult;

			lISourceRequestResult = aPtrISourceRequestResult;

			if (lISourceRequestResult)
				lISourceRequestResult->setData(mSilenceBlock.get(),
					mSilenceBlockSize, TRUE);

			--lCount;
		}
		
	}


	return lresult;
}