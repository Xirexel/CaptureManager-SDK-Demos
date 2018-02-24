#pragma once

#include <d3d11.h>
#include <string>
#include <memory>
#include <mutex>
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"

#include "../Common/CaptureManagerTypeInfo.h"

MIDL_INTERFACE("8CB2E9C9-44A6-42F7-87D3-C52571CB94E4")
ICaptureProcessorControl : public IUnknown
{
public:

	virtual void fillBackBuffer(BYTE* aPtrData, UINT RowPitch) = 0;

	virtual void swapBuffers() = 0;
};


class CaptureProcessor : public BaseUnknown<ICaptureProcessor, ICaptureProcessorControl>
{
	std::unique_ptr<BYTE[]> m_FirstBuffer;

	std::unique_ptr<BYTE[]> m_SecondBuffer;

	BYTE* m_ptrFrontBuffer;

	BYTE* m_ptrBackBuffer;

	DWORD m_SampleSize;

	std::wstring mPresentationDescriptor;

	std::mutex mLockMutex;

	DWORD mHeight;

	DWORD mStride;

public:

	CaptureProcessor();
	virtual ~CaptureProcessor();

	static HRESULT create(IUnknown** aPtrptrUnkICaptureProcessor, ID3D11Texture2D* a_CaptureTexture);


	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE start(
		/* [in] */ LONGLONG aStartPositionInHundredNanosecondUnits,
		/* [in] */ REFGUID aGUIDTimeFormat) override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE stop(void) override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE pause(void) override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE shutdown(void) override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE initilaize(
		/* [in] */ IUnknown *aPtrIInitilaizeCaptureSource) override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE setCurrentMediaType(
		/* [in] */ IUnknown *aPtrICurrentMediaType) override;

	virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE sourceRequest(
		/* [in] */ IUnknown *aPtrISourceRequestResult) override;





	virtual void fillBackBuffer(BYTE* aPtrData, UINT RowPitch) override;

	virtual void swapBuffers() override;
};

