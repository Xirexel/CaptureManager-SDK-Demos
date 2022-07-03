#pragma once

#include "Common\CaptureManagerTypeInfo.h"
#include "Common\ComPtrCustom.h"

#include "BaseUnknown.h"
#include <Mmdeviceapi.h> 
#include <Audioclient.h>
#include <memory>

class AudioCaptureProcessor:
	public BaseUnknown<ICaptureProcessor>
{
public:
	AudioCaptureProcessor();

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE start(
		/* [in] */ LONGLONG aStartPositionInHundredNanosecondUnits,
		/* [in] */ REFGUID aGUIDTimeFormat)override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE stop(void)override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE pause(void)override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE shutdown(void)override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE initilaize(
		/* [in] */ IUnknown *aPtrIInitilaizeCaptureSource)override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE setCurrentMediaType(
		/* [in] */ IUnknown *aPtrICurrentMediaType)override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE sourceRequest(
		/* [in] */ IUnknown *aPtrISourceRequestResult)override;

private:
	virtual ~AudioCaptureProcessor();
	
	CComPtrCustom<IMMDevice> mMMDevice;

	CComPtrCustom<IAudioClient> mAudioClient;

	CComPtrCustom<IAudioCaptureClient> mAudioCaptureClient;

	UINT32 mSilenceBlockSize;

	UINT32 mBlockAlign;

	std::unique_ptr<BYTE[]> mSilenceBlock;

	WAVEFORMATEX* mPtrWAVEFORMATEX = nullptr;

	HANDLE mAudioSamplesReadyEvent;
};

