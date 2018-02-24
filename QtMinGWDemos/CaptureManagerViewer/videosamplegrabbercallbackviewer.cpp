#include <atomic>
#include <functional>


#include "videosamplegrabbercallbackviewer.h"





VideoSampleGrabberCallbackViewer::VideoSampleGrabberCallbackViewer(
        QUuid aContainerTypeQUuid,
        uint aWidth,
        uint aHeight,
        IUnknown **aPtrPtrOutputNode,
        QWidget *parent) : OpenGLVideoViewer(parent)
{
    mWidth = aWidth;

    mHeight = aHeight;

    CComPtrCustom<ISinkControl> lISinkControl;

    if(CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl))
    {
        CComPtrCustom<ISampleGrabberCallbackSinkFactory> lISampleGrabberCallbackSinkFactory;

        auto lhresult = lISinkControl->createSinkFactory(
                    aContainerTypeQUuid,
                    IID_ISampleGrabberCallbackSinkFactory,
                    (IUnknown**)&lISampleGrabberCallbackSinkFactory);

        if(lhresult == 0)
        {
            CComPtrCustom<IStrideForBitmap> lIStrideForBitmap;

            if(CaptureManagerQtProxy::getInstance().getIStrideForBitmap(&lIStrideForBitmap))
            {
                LONG lStride;

                lhresult = lIStrideForBitmap->getStrideForBitmap(
                            MFVideoFormat_RGB24,
                            aWidth,
                            &lStride);

                DWORD lSampleByteSize = abs(lStride) * aHeight;

                if(lhresult == 0)
                {
                    mData.reset(new unsigned char[lSampleByteSize]);



struct MyISampleGrabberCallback: public ISampleGrabberCallback
{

public:

    MyISampleGrabberCallback(VideoSampleGrabberCallbackViewer* aObj):
        mCount(1), mObj(aObj){}




    virtual /* [local][helpstring] */ HRESULT STDMETHODCALLTYPE invoke(
        /* [in] */ REFGUID aGUIDMajorMediaType,
        /* [in] */ DWORD aSampleFlags,
        /* [in] */ LONGLONG aSampleTime,
        /* [in] */ LONGLONG aSampleDuration,
        /* [in] */ LPVOID aPtrSampleBuffer,
        /* [in] */ DWORD aSampleSize)
                        {

                            mObj->refresh(aPtrSampleBuffer,
                                          aSampleSize);

                            return S_OK;
                        }


public:
  virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void **ppvObject)
    {
        HRESULT lhresult = E_NOINTERFACE;

        do
        {
            GUID lIUnknownGUID = QUuid("{00000000-0000-0000-C000-000000000046}");

            if(riid == lIUnknownGUID)
            {
                *ppvObject = static_cast<IUnknown*>(this);
            }
            else if(riid == IID_ISampleGrabberCallback)
            {
                *ppvObject = static_cast<ISampleGrabberCallback*>(this);
            }
            else
            {
                break;
            }

            lhresult = S_OK;

            AddRef();

        }while(false);

        return lhresult;
    }

  virtual ULONG STDMETHODCALLTYPE AddRef(void)
    {
        return ++mCount;
    }

  virtual ULONG STDMETHODCALLTYPE Release(void)
    {
        auto lCount = --mCount;

        if(lCount == 0)
        {
            delete this;
        }

        return lCount;
    }

private:

    std::atomic<ULONG> mCount;

    VideoSampleGrabberCallbackViewer* mObj;

};

                CComPtrCustom<IUnknown> lMyISampleGrabberCallback(new MyISampleGrabberCallback(this));

                    lhresult = lISampleGrabberCallbackSinkFactory->createOutputNode(
                                MFMediaType_Video,
                                MFVideoFormat_RGB24,
                                lMyISampleGrabberCallback,
                                (IUnknown**)aPtrPtrOutputNode);
                }
            }

        }
    }

}

VideoSampleGrabberCallbackViewer::~VideoSampleGrabberCallbackViewer()
{
}

void VideoSampleGrabberCallbackViewer::refresh(
        /* [in] */ LPVOID aPtrSampleBuffer,
        /* [in] */ DWORD aSampleSize)
{
    memcpy(mData.get(), aPtrSampleBuffer, aSampleSize);

    update();
}

void VideoSampleGrabberCallbackViewer::reloadVideoFrame()
{
    glBindTexture(GL_TEXTURE_2D, mDrawTextureID);

    glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, mWidth, mHeight, GL_BGR, GL_UNSIGNED_BYTE, mData.get());
}

