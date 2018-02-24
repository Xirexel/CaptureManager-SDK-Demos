#ifndef CAPTUREMANAGERQTPROXY_H
#define CAPTUREMANAGERQTPROXY_H

#include <ActiveQt/QAxObject>
#include <QList>
#include <QString>
#include <memory>

#include "capturemanagerqtproxy_global.h"
#include "CaptureManagerTypeInfo.h"

class CAPTUREMANAGERQTPROXYSHARED_EXPORT CaptureManagerQtProxy
{


public:

    static CaptureManagerQtProxy& getInstance();

    bool isInitialized();

    bool release();

    bool getILogPrintOutControl(ILogPrintOutControl** aPtrPtrILogPrintOutControl);

    bool getISourceControl(ISourceControl** aPtrPtrISourceControl);

    bool getISinkControl(ISinkControl** aPtrPtrISinkControl);

    bool getIStrideForBitmap(IStrideForBitmap** aPtrPtrIStrideForBitmap);

    bool getISessionControl(ISessionControl** aPtrPtrISessionControl);

    bool getISession(QList<IUnknown*> aSourcesList, ISession** aPtrPtrISession);

    bool getIWebCamControl(QString aSymbolicLink, IWebCamControl** aPtrPtrIWebCamControl);

    bool getIEncoderControl(IEncoderControl** aPtrPtrIEncoderControl);

    bool getIEncoderNodeFactory(IID aEncoderCLSID, IEncoderNodeFactory** aPtrPtrIEncoderNodeFactory);

    bool getOutputNodes(QString aFilePath, QList<IUnknown*> aCompressedMediaTypeList, QList<IUnknown*>& aOutputNodeList, IFileSinkFactory* aPtrIFileSinkFactory);

    bool getOutputNodes(IUnknown* aPtrByteStream, QList<IUnknown*> aCompressedMediaTypeList, QList<IUnknown*>& aOutputNodeList, IByteStreamSinkFactory* aPtrIByteStreamSinkFactory);



private:

    std::unique_ptr<QAxObject> mCoLogPrintOutObject;
    std::unique_ptr<QAxObject> mCaptureManagerObject;

    bool mIsInitialized;


    CaptureManagerQtProxy();
    ~CaptureManagerQtProxy();
    CaptureManagerQtProxy(const CaptureManagerQtProxy&) = delete;
    CaptureManagerQtProxy& operator=(const CaptureManagerQtProxy&) = delete;
};

#endif // CAPTUREMANAGERQTPROXY_H
