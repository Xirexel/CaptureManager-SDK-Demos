#include "networkstreamsink.h"

#include <atomic>
#include <QUuid>
#include <QApplication>
#include <QDesktopWidget>
#include <QFileDialog>
#include <QMessageBox>
#include <QThreadPool>

#include "CaptureManagerQtProxy.h"
#include "ComPtrCustom.h"
#include "WebServer.h"

typedef ULONGLONG QWORD;

GUID lIUnknownGUID = QUuid("{00000000-0000-0000-C000-000000000046}");

GUID lIObjectContainerGUID = QUuid("{C18CCF77-3C8A-4753-8730-6036EBD38C4D}");

enum class MFByteStreamCapabilities
{
    None = 0x00000000,
    IsReadable = 0x00000001,
    IsWritable = 0x00000002,
    IsSeekable = 0x00000004,
    IsRemote = 0x00000008,
    IsDirectory = 0x00000080,
    HasSlowSeek = 0x00000100,
    IsPartiallyDownloaded = 0x00000200,
    ShareWrite = 0x00000400,
    DoesNotUseNetwork = 0x00000800,
};

typedef enum class _MFBYTESTREAM_SEEK_ORIGIN
{
    Begin	= 0,
    Current	= ( Begin + 1 )
} MFBYTESTREAM_SEEK_ORIGIN;

enum class MFByteStreamSeekingFlags
{
    None = 0,
    CancelPendingIO = 1
};

enum class MFASync
{
    None = 0,
    FastIOProcessingCallback = 0x00000001,
    SignalCallback = 0x00000002,
    BlockingCallback = 0x00000004,
    ReplyCallback = 0x00000008,
    LocalizeRemoteCallback = 0x00000010,
};

enum class MFAsyncCallbackQueue
{
    Undefined = 0x00000000,
    Standard = 0x00000001,
    RT = 0x00000002,
    IO = 0x00000003,
    Timer = 0x00000004,
    MultiThreaded = 0x00000005,
    LongFunction = 0x00000007,
    PrivateMask = (int)0xFFFF0000,
    All = (int)0xFFFFFFFF
};

struct IMFAsyncResult : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE GetState(
        /* [out] */ __RPC__deref_out_opt IUnknown **ppunkState) = 0;

    virtual HRESULT STDMETHODCALLTYPE GetStatus( void) = 0;

    virtual HRESULT STDMETHODCALLTYPE SetStatus(
        /* [in] */ HRESULT hrStatus) = 0;

    virtual HRESULT STDMETHODCALLTYPE GetObject(
        /* [out] */ __RPC__deref_out_opt IUnknown **ppObject) = 0;

    virtual /* [local] */ IUnknown *STDMETHODCALLTYPE GetStateNoAddRef( void) = 0;

};

struct IMFAsyncCallback : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE GetParameters(
        /* [out] */ __RPC__out DWORD *pdwFlags,
        /* [out] */ __RPC__out DWORD *pdwQueue) = 0;

    virtual HRESULT STDMETHODCALLTYPE Invoke(
        /* [in] */ __RPC__in_opt IMFAsyncResult *pAsyncResult) = 0;

};



struct IMFByteStream : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE GetCapabilities(
        /* [out] */ __RPC__out DWORD *pdwCapabilities) = 0;

    virtual HRESULT STDMETHODCALLTYPE GetLength(
        /* [out] */ __RPC__out QWORD *pqwLength) = 0;

    virtual HRESULT STDMETHODCALLTYPE SetLength(
        /* [in] */ QWORD qwLength) = 0;

    virtual HRESULT STDMETHODCALLTYPE GetCurrentPosition(
        /* [out] */ __RPC__out QWORD *pqwPosition) = 0;

    virtual HRESULT STDMETHODCALLTYPE SetCurrentPosition(
        /* [in] */ QWORD qwPosition) = 0;

    virtual HRESULT STDMETHODCALLTYPE IsEndOfStream(
        /* [out] */ __RPC__out BOOL *pfEndOfStream) = 0;

    virtual HRESULT STDMETHODCALLTYPE Read(
        /* [size_is][out] */ __RPC__out_ecount_full(cb) BYTE *pb,
        /* [in] */ ULONG cb,
        /* [out] */ __RPC__out ULONG *pcbRead) = 0;

    virtual /* [local] */ HRESULT STDMETHODCALLTYPE BeginRead(
        /* [annotation][out] */
        BYTE *pb,
        /* [in] */ ULONG cb,
        /* [in] */ IMFAsyncCallback *pCallback,
        /* [in] */ IUnknown *punkState) = 0;

    virtual /* [local] */ HRESULT STDMETHODCALLTYPE EndRead(
        /* [in] */ IMFAsyncResult *pResult,
        /* [annotation][out] */
        _Out_  ULONG *pcbRead) = 0;

    virtual HRESULT STDMETHODCALLTYPE Write(
        /* [size_is][in] */ __RPC__in_ecount_full(cb) const BYTE *pb,
        /* [in] */ ULONG cb,
        /* [out] */ __RPC__out ULONG *pcbWritten) = 0;

    virtual /* [local] */ HRESULT STDMETHODCALLTYPE BeginWrite(
        /* [annotation][in] */
         const BYTE *pb,
        /* [in] */ ULONG cb,
        /* [in] */ IMFAsyncCallback *pCallback,
        /* [in] */ IUnknown *punkState) = 0;

    virtual /* [local] */ HRESULT STDMETHODCALLTYPE EndWrite(
        /* [in] */ IMFAsyncResult *pResult,
        /* [annotation][out] */
        _Out_  ULONG *pcbWritten) = 0;

    virtual HRESULT STDMETHODCALLTYPE Seek(
        /* [in] */ MFBYTESTREAM_SEEK_ORIGIN SeekOrigin,
        /* [in] */ LONGLONG llSeekOffset,
        /* [in] */ DWORD dwSeekFlags,
        /* [out] */ __RPC__out QWORD *pqwCurrentPosition) = 0;

    virtual HRESULT STDMETHODCALLTYPE Flush( void) = 0;

    virtual HRESULT STDMETHODCALLTYPE Close( void) = 0;

};

class HttpOutputByteStream: public IMFByteStream
{

    QUuid mIMFByteStreamGUID;

    WebServer* mWebServer;

public:


signals:


public:

    HttpOutputByteStream(WebServerConfiguration aWebServerConfiguration): mCount(1), mIMFByteStreamGUID("{ad4c1b00-4bf7-422f-9175-756693d9130d}")
    {
        mWebServer = new WebServer(aWebServerConfiguration);

        mWebServer->Start();
    }

    virtual HRESULT STDMETHODCALLTYPE GetCapabilities(
        /* [out] */ __RPC__out DWORD *pdwCapabilities)
    {
        *pdwCapabilities = (DWORD)MFByteStreamCapabilities::IsWritable;

        return S_OK;
    }

    virtual HRESULT STDMETHODCALLTYPE GetLength(
        /* [out] */ __RPC__out QWORD *pqwLength)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE SetLength(
        /* [in] */ QWORD qwLength)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE GetCurrentPosition(
        /* [out] */ __RPC__out QWORD *pqwPosition)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE SetCurrentPosition(
        /* [in] */ QWORD qwPosition)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE IsEndOfStream(
        /* [out] */ __RPC__out BOOL *pfEndOfStream)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE Read(
        /* [size_is][out] */ __RPC__out_ecount_full(cb) BYTE *pb,
        /* [in] */ ULONG cb,
        /* [out] */ __RPC__out ULONG *pcbRead)
    {
        return E_NOTIMPL;
    }

    virtual /* [local] */ HRESULT STDMETHODCALLTYPE BeginRead(
        /* [annotation][out] */
        BYTE *pb,
        /* [in] */ ULONG cb,
        /* [in] */ IMFAsyncCallback *pCallback,
        /* [in] */ IUnknown *punkState)
    {
        return E_NOTIMPL;
    }

    virtual /* [local] */ HRESULT STDMETHODCALLTYPE EndRead(
        /* [in] */ IMFAsyncResult *pResult,
        /* [annotation][out] */
        _Out_  ULONG *pcbRead)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE Write(
        /* [size_is][in] */ __RPC__in_ecount_full(cb) const BYTE *pb,
        /* [in] */ ULONG cb,
        /* [out] */ __RPC__out ULONG *pcbWritten)
    {
        mWebServer->writeHeaderMemory(pb, cb);

        *pcbWritten = cb;

        return S_OK;
    }

    struct IObjectContainer : public IUnknown
    {
        virtual uint getDataLength() = 0;
    };

    class ObjectContainer: public IObjectContainer
    {
        uint mDataLength;

    public:

        ObjectContainer(uint aDataLength):mCount(1), mDataLength(aDataLength)
        {

        }

        virtual uint getDataLength()
        {
            return mDataLength;
        }


    public:
      virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void **ppvObject)
        {
            HRESULT lhresult = E_NOINTERFACE;

            do
            {

                if(riid == lIUnknownGUID)
                {
                    *ppvObject = static_cast<IUnknown*>(this);
                }
                else if(riid == lIObjectContainerGUID)
                {
                    *ppvObject = static_cast<IObjectContainer*>(this);
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

    };

    class AsyncResult : public IMFAsyncResult
    {
    public:

        AsyncResult():mCount(1)
        {

        }

        int mHRStatus;

        CComPtrCustom<IUnknown> punkState;

        CComPtrCustom<IUnknown> pCallback;


        virtual HRESULT STDMETHODCALLTYPE GetState(
            /* [out] */ __RPC__deref_out_opt IUnknown **ppunkState)
        {
            punkState->QueryInterface(lIUnknownGUID, (void**)ppunkState);

            return S_OK;
        }

        virtual HRESULT STDMETHODCALLTYPE GetStatus( void)
        {
            return mHRStatus;
        }

        virtual HRESULT STDMETHODCALLTYPE SetStatus(
            /* [in] */ HRESULT hrStatus)
        {
            mHRStatus = hrStatus;

            return S_OK;
        }

        virtual HRESULT STDMETHODCALLTYPE GetObject(
            /* [out] */ __RPC__deref_out_opt IUnknown **ppObject)
        {
            pCallback->QueryInterface(lIUnknownGUID, (void**)ppObject);

            return S_OK;
        }

        virtual /* [local] */ IUnknown *STDMETHODCALLTYPE GetStateNoAddRef( void)
        {
            return punkState;
        }





    public:
      virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void **ppvObject)
        {
            HRESULT lhresult = E_NOINTERFACE;

            do
            {

                GUID lIMFAsyncResultGUID = QUuid("{AC6B7889-0740-4D51-8619-905994A55CC6}");

                if(riid == lIUnknownGUID)
                {
                    *ppvObject = static_cast<IUnknown*>(this);
                }
                else if(riid == lIMFAsyncResultGUID)
                {
                    *ppvObject = static_cast<IMFAsyncResult*>(this);
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
    };

    class AsyncWriteData : public QRunnable
    {
    public:


        CComPtrCustom<IMFAsyncCallback> pCallback;

        CComPtrCustom<IUnknown> punkState;

        uint cb;

        void run()
        {
            AsyncResult* lAsyncResult = new AsyncResult();

            lAsyncResult->mHRStatus = 0;

            lAsyncResult->pCallback = new ObjectContainer(cb);

            punkState->QueryInterface(lIUnknownGUID, (void**)&lAsyncResult->punkState);

            pCallback->Invoke(lAsyncResult);

            lAsyncResult->Release();
        }
    };



    virtual /* [local] */ HRESULT STDMETHODCALLTYPE BeginWrite(
        /* [annotation][in] */
        const BYTE *pb,
        /* [in] */ ULONG cb,
        /* [in] */ IMFAsyncCallback *pCallback,
        /* [in] */ IUnknown *punkState)
    {
        mWebServer->sendRawData(pb, cb);

        AsyncWriteData *lPtrAsyncWriteData = new AsyncWriteData;

        lPtrAsyncWriteData->setAutoDelete(true);

        GUID lIMFAsyncCallbackGUID = QUuid("{A27003CF-2354-4F2A-8D6A-AB7CFF15437E}");

        pCallback->QueryInterface(lIMFAsyncCallbackGUID, (void**)&lPtrAsyncWriteData->pCallback);

        punkState->QueryInterface(lIUnknownGUID, (void**)&lPtrAsyncWriteData->punkState);

        lPtrAsyncWriteData->cb = cb;

        QThreadPool::globalInstance()->start(lPtrAsyncWriteData);

        return S_OK;
    }

    virtual /* [local] */ HRESULT STDMETHODCALLTYPE EndWrite(
        /* [in] */ IMFAsyncResult *pResult,
        /* [annotation][out] */
        _Out_  ULONG *pcbWritten)
    {

        *pcbWritten = 0;

        do
        {
            if (pResult == nullptr)
                break;

            auto lstatus = pResult->GetStatus();

            if (lstatus != 0)
                break;


            CComPtrCustom<IUnknown> lIUnknow;


            auto lresult = pResult->GetObject(&lIUnknow);

            if (lresult != 0)
                break;

            CComPtrCustom<IObjectContainer> lIObjectContainer;

            lIUnknow->QueryInterface(lIObjectContainerGUID, (void**)&lIObjectContainer);

            if (lIObjectContainer == nullptr)
                break;

            *pcbWritten = lIObjectContainer->getDataLength();

        } while (false);

        return S_OK;
    }

    virtual HRESULT STDMETHODCALLTYPE Seek(
        /* [in] */ MFBYTESTREAM_SEEK_ORIGIN SeekOrigin,
        /* [in] */ LONGLONG llSeekOffset,
        /* [in] */ DWORD dwSeekFlags,
        /* [out] */ __RPC__out QWORD *pqwCurrentPosition)
    {
        return E_NOTIMPL;
    }

    virtual HRESULT STDMETHODCALLTYPE Flush( void)
    {
        return S_OK;
    }

    virtual HRESULT STDMETHODCALLTYPE Close( void)
    {
        mWebServer->Stop();

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
                else if(riid == mIMFByteStreamGUID)
                {
                    *ppvObject = (this);
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

};


WebServerConfiguration gWebServerConfiguration;


NetworkStreamSink::NetworkStreamSink(
        QString aGUIDValue,
        QString aMIME,
        QObject *parent) : QObject(parent), mGUIDValue(aGUIDValue)
{
    gWebServerConfiguration._MIME = aMIME;
}

void NetworkStreamSink::setOptions()
{
    do
    {


    }while(false);
}

IUnknown* NetworkStreamSink::getOutputNode(IUnknown* aUpStreamMediaType)
{
    IUnknown* lptrResult = nullptr;

    HttpOutputByteStream* lPtrHttpOutputByteStream = new HttpOutputByteStream(gWebServerConfiguration);

    do
    {
        CComPtrCustom<ISinkControl> lISinkControl;

        CComPtrCustom<IByteStreamSinkFactory> lIByteStreamSinkFactory;

        if(CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl))
        {
            QUuid lIID_IByteStreamSinkFactory(0x2E891049,0x964A,0x4D08,0x8F,0x36,0x95,0xCE,0x8C,0xB0,0xDE,0x9B);

            auto lhresult = lISinkControl->createSinkFactory(
                        QUuid(mGUIDValue),
                        lIID_IByteStreamSinkFactory,
                        (IUnknown**)&lIByteStreamSinkFactory);

            if(lhresult != 0)
            {
                break;
            }

            QList<IUnknown*> lCompressedMediaTypeList;

            lCompressedMediaTypeList.push_back(aUpStreamMediaType);

            QList<IUnknown*> lOutputNodeList;

            if(CaptureManagerQtProxy::getInstance().getOutputNodes(
                        lPtrHttpOutputByteStream,
                        lCompressedMediaTypeList,
                        lOutputNodeList,
                        lIByteStreamSinkFactory))
            {
                if(lOutputNodeList.length() == 0)
                    break;

                GUID lIID_IUnknown = QUuid("{00000000-0000-0000-C000-000000000046}");

                lhresult = lOutputNodeList.at(0)->QueryInterface(
                            lIID_IUnknown,
                            (void**)&lptrResult);

                if(lhresult != 0)
                {
                    break;
                }

            }
        }

    }while(false);

    lPtrHttpOutputByteStream->Release();

    return lptrResult;
}
