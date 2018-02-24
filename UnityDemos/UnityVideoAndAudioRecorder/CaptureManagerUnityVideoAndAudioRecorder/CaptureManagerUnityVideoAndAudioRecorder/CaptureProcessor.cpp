
#include "CaptureProcessor.h"
#include "RecorderFactory.h"
#include "../Common/ComPtrCustom.h"

static const GUID MFVideoFormat_RGB32 =
{ 22, 0x0000, 0x0010, { 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71 } };



static RecorderFactory g_RecorderFactory;

CaptureProcessor::CaptureProcessor()
{
}


CaptureProcessor::~CaptureProcessor()
{
}

HRESULT CaptureProcessor::create(IUnknown** aPtrptrUnkICaptureProcessor, ID3D11Texture2D* a_CaptureTexture)
{
	std::wstring lPresentationDescriptor = L"<?xml version='1.0' encoding='UTF-8'?>";
	lPresentationDescriptor += L"<PresentationDescriptor StreamCount='1'>";
	lPresentationDescriptor += L"<PresentationDescriptor.Attributes Title='Attributes of Presentation'>";
	lPresentationDescriptor += L"<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' GUID='{58F0AAD8-22BF-4F8A-BB3D-D2C4978C6E2F}' Title='The symbolic link for a video capture driver.' Description='Contains the unique symbolic link for a video capture driver.'>";
	lPresentationDescriptor += L"<SingleValue Value='ImageCaptureProcessor' />";
	lPresentationDescriptor += L"</Attribute>";
	lPresentationDescriptor += L"<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME' GUID='{60D0E559-52F8-4FA2-BBCE-ACDB34A8EC01}' Title='The display name for a device.' Description='The display name is a human-readable string, suitable for display in a user interface.'>";
	lPresentationDescriptor += L"<SingleValue Value='Image Capture Processor' />";
	lPresentationDescriptor += L"</Attribute>";
	lPresentationDescriptor += L"</PresentationDescriptor.Attributes>";
	lPresentationDescriptor += L"<StreamDescriptor Index='0' MajorType='MFMediaType_Video' MajorTypeGUID='{73646976-0000-0010-8000-00AA00389B71}'>";
	lPresentationDescriptor += L"<MediaTypes TypeCount='1'>";
	lPresentationDescriptor += L"<MediaType Index='0'>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_FRAME_SIZE' GUID='{1652C33D-D6B2-4012-B834-72030849A37D}' Title='Width and height of the video frame.' Description='Width and height of a video frame, in pixels.'>";
	lPresentationDescriptor += L"<Value.ValueParts>";
	lPresentationDescriptor += L"<ValuePart Title='Width' Value='Temp_Width' />";
	lPresentationDescriptor += L"<ValuePart Title='Height' Value='Temp_Height' />";
	lPresentationDescriptor += L"</Value.ValueParts>";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_AVG_BITRATE' GUID='{20332624-FB0D-4D9E-BD0D-CBF6786C102E}' Title='Approximate data rate of the video stream.' Description='Approximate data rate of the video stream, in bits per second, for a video media type.'>";
	lPresentationDescriptor += L"<SingleValue  Value='33570816' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_MAJOR_TYPE' GUID='{48EBA18E-F8C9-4687-BF11-0A74C9F96A8F}' Title='Major type GUID for a media type.' Description='The major type defines the overall category of the media data.'>";
	lPresentationDescriptor += L"<SingleValue Value='MFMediaType_Video' GUID='{73646976-0000-0010-8000-00AA00389B71}' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_DEFAULT_STRIDE' GUID='{644B4E48-1E02-4516-B0EB-C01CA9D49AC6}' Title='Default surface stride.' Description='Default surface stride, for an uncompressed video media type. Stride is the number of bytes needed to go from one row of pixels to the next.'>";
	lPresentationDescriptor += L"<SingleValue Value='Temp_Stride' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_FIXED_SIZE_SAMPLES' GUID='{B8EBEFAF-B718-4E04-B0A9-116775E3321B}' Title='The fixed size of samples in stream.' Description='Specifies for a media type whether the samples have a fixed size.'>";
	lPresentationDescriptor += L"<SingleValue Value='True' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_FRAME_RATE' GUID='{C459A2E8-3D2C-4E44-B132-FEE5156C7BB0}' Title='Frame rate.' Description='Frame rate of a video media type, in frames per second.'>";
	lPresentationDescriptor += L"<RatioValue Value='30.0'>";
	lPresentationDescriptor += L"<Value.ValueParts>";
	lPresentationDescriptor += L"<ValuePart Title='Numerator'  Value='30' />";
	lPresentationDescriptor += L"<ValuePart Title='Denominator'  Value='1' />";
	lPresentationDescriptor += L"</Value.ValueParts>";
	lPresentationDescriptor += L"</RatioValue>";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_PIXEL_ASPECT_RATIO' GUID='{C6376A1E-8D0A-4027-BE45-6D9A0AD39BB6}' Title='Pixel aspect ratio.' Description='Pixel aspect ratio for a video media type.'>";
	lPresentationDescriptor += L"<RatioValue  Value='1'>";
	lPresentationDescriptor += L"<Value.ValueParts>";
	lPresentationDescriptor += L"<ValuePart Title='Numerator'  Value='1' />";
	lPresentationDescriptor += L"<ValuePart Title='Denominator'  Value='1' />";
	lPresentationDescriptor += L"</Value.ValueParts>";
	lPresentationDescriptor += L"</RatioValue>";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_ALL_SAMPLES_INDEPENDENT' GUID='{C9173739-5E56-461C-B713-46FB995CB95F}' Title='Independent of samples.' Description='Specifies for a media type whether each sample is independent of the other samples in the stream.'>";
	lPresentationDescriptor += L"<SingleValue Value='True' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_SAMPLE_SIZE' GUID='{DAD3AB78-1990-408B-BCE2-EBA673DACC10}' Title='The fixed size of each sample in stream.' Description='Specifies the size of each sample, in bytes, in a media type.'>";
	lPresentationDescriptor += L"<SingleValue Value='Temp_SampleSize' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_INTERLACE_MODE' GUID='{E2724BB8-E676-4806-B4B2-A8D6EFB44CCD}' Title='Describes how the frames are interlaced.' Description='Describes how the frames in a video media type are interlaced.'>";
	lPresentationDescriptor += L"<SingleValue Value='MFVideoInterlace_Progressive' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"<MediaTypeItem Name='MF_MT_SUBTYPE' GUID='{F7E34C9A-42E8-4714-B74B-CB29D72C35E5}' Title='Subtype GUID for a media type.' Description='The subtype GUID defines a specific media format type within a major type.'>";
	lPresentationDescriptor += L"<SingleValue GUID='{Temp_SubTypeGUID}' />";
	lPresentationDescriptor += L"</MediaTypeItem>";
	lPresentationDescriptor += L"</MediaType>";
	lPresentationDescriptor += L"</MediaTypes>";
	lPresentationDescriptor += L"</StreamDescriptor>";
	lPresentationDescriptor += L"</PresentationDescriptor>";
	
	D3D11_TEXTURE2D_DESC lDesc = { 0 };
	a_CaptureTexture->GetDesc(&lDesc);

	auto lWidth = (lDesc.Width >> 1) << 1;

	auto Height = (lDesc.Height >> 1) << 1;



	CComPtrCustom<CaptureProcessor> lCaptureProcessor(new CaptureProcessor());

	auto lStride = g_RecorderFactory.getStrideForBitmapInfoHeader(MFVideoFormat_RGB32, lWidth);

	auto lfind = lPresentationDescriptor.find(L"Temp_Width");

	wchar_t lvalue[256];

	_itow_s (lWidth, lvalue, 10);

	lPresentationDescriptor.replace(lfind, 10, lvalue);

	_itow_s(Height, lvalue, 10);

	lfind = lPresentationDescriptor.find(L"Temp_Height");

	lPresentationDescriptor.replace(lfind, 11, lvalue);

	_itow_s(-lStride, lvalue, 10);

	lfind = lPresentationDescriptor.find(L"Temp_Stride");

	lPresentationDescriptor.replace(lfind, 11, lvalue);

	DWORD l_SampleSize = ::abs(lStride) * Height;

	_itow_s(l_SampleSize, lvalue, 10);

	lfind = lPresentationDescriptor.find(L"Temp_SampleSize");

	lPresentationDescriptor.replace(lfind, 15, lvalue);

	wchar_t lGUID[1024];

	StringFromGUID2(MFVideoFormat_RGB32, lGUID, 1024);

	lfind = lPresentationDescriptor.find(L"{Temp_SubTypeGUID}");

	lPresentationDescriptor.replace(lfind, 18, lGUID);

	lCaptureProcessor->mPresentationDescriptor = lPresentationDescriptor;

	lCaptureProcessor->m_SampleSize = l_SampleSize;

	lCaptureProcessor->m_FirstBuffer.reset(new BYTE[l_SampleSize]());

	lCaptureProcessor->m_SecondBuffer.reset(new BYTE[l_SampleSize]());

	lCaptureProcessor->m_ptrFrontBuffer = lCaptureProcessor->m_FirstBuffer.get();

	lCaptureProcessor->m_ptrBackBuffer = lCaptureProcessor->m_SecondBuffer.get();

	lCaptureProcessor->mHeight = Height;

	lCaptureProcessor->mStride = (DWORD)::abs(lStride);

	lDesc.Usage = D3D11_USAGE_STAGING;
	lDesc.BindFlags = 0;
	lDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	lDesc.MiscFlags = 0;
	

	CComPtrCustom<ID3D11Device> lDevice;

	a_CaptureTexture->GetDevice(&lDevice);

	CComPtrCustom<ID3D11Texture2D> lStagingTexture;

	lDevice->CreateTexture2D(&lDesc, NULL, &lStagingTexture);

	CComPtrCustom<ID3D11DeviceContext> lID3D11DeviceContext;

	lDevice->GetImmediateContext(&lID3D11DeviceContext);

	lID3D11DeviceContext->CopyResource(lStagingTexture, a_CaptureTexture);

	D3D11_MAPPED_SUBRESOURCE mapped = { 0 };
	lID3D11DeviceContext->Map(lStagingTexture, 0, D3D11_MAP_READ, 0, &mapped);

	for (size_t i = 0; i < Height; i++)
	{
		memcpy(lCaptureProcessor->m_ptrFrontBuffer + (i * ::abs(lStride)), (BYTE*)mapped.pData + (i * mapped.RowPitch), ::abs(lStride));
	}


	lID3D11DeviceContext->Unmap(lStagingTexture, 0);


	return lCaptureProcessor->QueryInterface(IID_PPV_ARGS(aPtrptrUnkICaptureProcessor));
}

HRESULT STDMETHODCALLTYPE CaptureProcessor::start(
	/* [in] */ LONGLONG aStartPositionInHundredNanosecondUnits,
	/* [in] */ REFGUID aGUIDTimeFormat)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CaptureProcessor::stop(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CaptureProcessor::pause(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CaptureProcessor::shutdown(void)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CaptureProcessor::initilaize(
/* [in] */ IUnknown *aPtrIInitilaizeCaptureSource)
{
	CComPtrCustom<IInitilaizeCaptureSource> lIInitilaizeCaptureSource;

	auto lhres = aPtrIInitilaizeCaptureSource->QueryInterface(IID_PPV_ARGS(&lIInitilaizeCaptureSource));

	if (FAILED(lhres))
		return lhres;

	lIInitilaizeCaptureSource->setPresentationDescriptor((BSTR)mPresentationDescriptor.c_str());

	return lhres;
}

HRESULT STDMETHODCALLTYPE CaptureProcessor::setCurrentMediaType(
/* [in] */ IUnknown *aPtrICurrentMediaType)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CaptureProcessor::sourceRequest(
/* [in] */ IUnknown *aPtrISourceRequestResult)
{
	CComPtrCustom<ISourceRequestResult> lISourceRequestResult;

	auto lhres = aPtrISourceRequestResult->QueryInterface(IID_PPV_ARGS(&lISourceRequestResult));

	if (FAILED(lhres))
		return lhres;
	
	lISourceRequestResult->setData(m_ptrFrontBuffer, m_SampleSize, TRUE);

	return S_OK;
}

//void swap1(ARGB *orig, BGR *dest, unsigned imageSize) {
//	unsigned x;
//	for (x = 0; x < imageSize; x++) {
//		*((uint32_t*)(((uint8_t*)dest) + x * 3)) = OSSwapInt32(((uint32_t*)orig)[x]);
//	}
//}

//void swap2(ARGB *orig, BGR *dest, unsigned imageSize) {
//	asm(
//		"0:\n\t"
//		"movl   (%1),%%eax\n\t"
//		"bswapl %%eax\n\t"
//		"movl   %%eax,(%0)\n\t"
//		"addl   $4,%1\n\t"
//		"addl   $3,%0\n\t"
//		"decl   %2\n\t"
//		"jnz    0b"
//		:: "D" (dest), "S" (orig), "c" (imageSize)
//		: "flags", "eax"
//		);
//}

//void swap2_2(uint8_t *orig, uint8_t *dest, size_t imagesize) {
//	int8_t mask[16] = { 3, 2, 1, 7, 6, 5, 11, 10, 9, 15, 14, 13, 0xFF, 0xFF, 0xFF, 0XFF };//{0xFF, 0xFF, 0xFF, 0xFF, 13, 14, 15, 9, 10, 11, 5, 6, 7, 1, 2, 3};
//	__asm{
//		"lddqu  (%3),%%xmm1\n\t"
//			"0:\n\t"
//			"lddqu  (%1),%%xmm0\n\t"
//			"pshufb %%xmm1,%%xmm0\n\t"
//			"movdqu %%xmm0,(%0)\n\t"
//			"add    $16,%1\n\t"
//			"add    $12,%0\n\t"
//			"sub    $4,%2\n\t"
//			"jnz    0b"
//			:: "r" (dest), "r" (orig), "r" (imagesize), "r" (mask)
//			: "flags", "xmm0", "xmm1"
//	};
//}

void CaptureProcessor::fillBackBuffer(BYTE* aPtrData, UINT RowPitch)
{
	auto lPixelsCount = mStride >> 2;

	DWORD lposition = 0;

	//std::lock_guard<std::mutex> lLock(mLockMutex);

	for (size_t i = 0; i < mHeight; i++)
	{
		auto lDest = m_ptrBackBuffer + (i * mStride);

		auto lSrc = aPtrData + (i * RowPitch);

		for (size_t x = 0; x < lPixelsCount; x++)
		{
			lposition = x << 2;

			lDest[lposition] = lSrc[lposition + 2];
			lDest[lposition + 1] = lSrc[lposition + 1];
			lDest[lposition + 2] = lSrc[lposition];
			lDest[lposition + 3] = lSrc[lposition + 3];
		}
				
		//memcpy(m_ptrBackBuffer + (i * mStride), aPtrData + (i * RowPitch), mStride);
	}

}

void CaptureProcessor::swapBuffers()
{
	//std::lock_guard<std::mutex> lLock(mLockMutex);

	auto ltemp = m_ptrFrontBuffer;
	
	m_ptrFrontBuffer = m_ptrBackBuffer;

	m_ptrBackBuffer = ltemp;
}