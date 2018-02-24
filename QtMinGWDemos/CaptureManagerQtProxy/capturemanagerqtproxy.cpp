#include <QUuid>
#include <Objbase.h>
#include <memory>
#include <unknwnbase.h>

#include "capturemanagerqtproxy.h"

typedef HRESULT(STDAPICALLTYPE *PDllGetClassObject) (REFCLSID, REFIID, void**);

CaptureManagerQtProxy::CaptureManagerQtProxy()
{
    CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

    bool lresult = false;

    do
    {
        do
        {

            PDllGetClassObject lPDllGetClassObject;

            HMODULE lmodule = LoadLibraryA("CaptureManager.dll");

            if(lmodule == nullptr)
                break;

            lPDllGetClassObject = (PDllGetClassObject)GetProcAddress(lmodule, "DllGetClassObject");

            if (lPDllGetClassObject == nullptr)
            {
                break;
            }

            IClassFactory* lPtrIClassFactoryLogPrintOut = nullptr;

            IClassFactory* lPtrIClassFactoryCaptureManagerOut = nullptr;



            IUnknown* lPtrIUnkLogPrintOut = nullptr;

            IUnknown* lPtrIUnkCaptureManagerOut = nullptr;


            QUuid lIClassFactoryQUuid("{00000001-0000-0000-C000-000000000046}");

            GUID lIUnknownGUID = QUuid("{00000000-0000-0000-C000-000000000046}");

            QUuid lCLSID_CoLogPrintOutQUuid("{4563EE3E-DA1E-4911-9F40-88A284E2DD69}");

            QUuid lCLSID_CoCaptureManagerQUuid("{D5F07FB8-CE60-4017-B215-95C8A0DDF42A}");

            auto lhresult = lPDllGetClassObject(
                        lCLSID_CoLogPrintOutQUuid,
                        lIClassFactoryQUuid,
                        (void**)&lPtrIClassFactoryLogPrintOut);

            if(lhresult != 0)
                break;

            lhresult = lPtrIClassFactoryLogPrintOut->CreateInstance(
                        nullptr,
                        lIUnknownGUID,
                        (void**)&lPtrIUnkLogPrintOut);

            if(lhresult != 0)
                break;

            mCoLogPrintOutObject.reset(new QAxObject(lPtrIUnkLogPrintOut));

            lhresult = lPDllGetClassObject(
                        lCLSID_CoCaptureManagerQUuid,
                        lIClassFactoryQUuid,
                        (void**)&lPtrIClassFactoryCaptureManagerOut);

            if(lhresult != 0)
                break;


            lhresult = lPtrIClassFactoryCaptureManagerOut->CreateInstance(
                        nullptr,
                        lIUnknownGUID,
                        (void**)&lPtrIUnkCaptureManagerOut);

            if(lhresult != 0)
                break;

            mCaptureManagerObject.reset(new QAxObject(lPtrIUnkCaptureManagerOut));

            lresult = true;

        }while(false);

        if(lresult)
            break;

        mCoLogPrintOutObject.reset(new QAxObject);

        mCaptureManagerObject.reset(new QAxObject);

        lresult = mCoLogPrintOutObject->setControl ("{4563EE3E-DA1E-4911-9F40-88A284E2DD69}");

        if(!lresult)
        {

        }

        lresult = mCaptureManagerObject->setControl("{D5F07FB8-CE60-4017-B215-95C8A0DDF42A}");

        if(!lresult)
        {
            break;
        }

        lresult = true;

    }while(false);

    mIsInitialized = lresult;
}

CaptureManagerQtProxy::~CaptureManagerQtProxy()
{

}

CaptureManagerQtProxy& CaptureManagerQtProxy::getInstance()
{
    static CaptureManagerQtProxy lInstance;

    return lInstance;
}

bool CaptureManagerQtProxy::isInitialized()
{
    return mIsInitialized;
}

bool CaptureManagerQtProxy::release()
{
    if(mIsInitialized)
    {
        mCoLogPrintOutObject->clear();

        mCaptureManagerObject->clear();

        CoUninitialize();

        mIsInitialized = false;
    }

    return mIsInitialized;
}

bool CaptureManagerQtProxy::getILogPrintOutControl(ILogPrintOutControl **aPtrPtrILogPrintOutControl)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrILogPrintOutControl == nullptr)
            break;

        auto lhresult = mCoLogPrintOutObject->queryInterface(
                    IID_ILogPrintOutControl,
                    (void**)aPtrPtrILogPrintOutControl);

        if(lhresult != 0)
            break;

        lresult = true;
    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getISourceControl(ISourceControl** aPtrPtrISourceControl)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrISourceControl == nullptr)
            break;

        ICaptureManagerControl* lPtrICaptureManagerControl = nullptr;

        auto lhresult = mCaptureManagerObject->queryInterface(
                    IID_ICaptureManagerControl,
                    (void**)&lPtrICaptureManagerControl);

        if(lhresult != 0)
            break;

        if(lPtrICaptureManagerControl == nullptr)
            break;

        lhresult = lPtrICaptureManagerControl->createControl(
                    IID_ISourceControl,
                    (IUnknown**)aPtrPtrISourceControl);

        lPtrICaptureManagerControl->Release();

        if(lhresult != 0)
            break;

        lresult = true;
    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getISinkControl(ISinkControl** aPtrPtrISinkControl)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrISinkControl == nullptr)
            break;

        ICaptureManagerControl* lPtrICaptureManagerControl = nullptr;

        auto lhresult = mCaptureManagerObject->queryInterface(
                    IID_ICaptureManagerControl,
                    (void**)&lPtrICaptureManagerControl);

        if(lhresult != 0)
            break;

        if(lPtrICaptureManagerControl == nullptr)
            break;

        lhresult = lPtrICaptureManagerControl->createControl(
                    IID_ISinkControl,
                    (IUnknown**)aPtrPtrISinkControl);

        lPtrICaptureManagerControl->Release();

        if(lhresult != 0)
            break;

        lresult = true;
    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getIStrideForBitmap(IStrideForBitmap** aPtrPtrIStrideForBitmap)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrIStrideForBitmap == nullptr)
            break;

        ICaptureManagerControl* lPtrICaptureManagerControl = nullptr;

        auto lhresult = mCaptureManagerObject->queryInterface(
                    IID_ICaptureManagerControl,
                    (void**)&lPtrICaptureManagerControl);

        if(lhresult != 0)
            break;

        if(lPtrICaptureManagerControl == nullptr)
            break;

        lhresult = lPtrICaptureManagerControl->createMisc(
                    IID_IStrideForBitmap,
                    (IUnknown**)aPtrPtrIStrideForBitmap);

        lPtrICaptureManagerControl->Release();

        if(lhresult != 0)
            break;

        lresult = true;
    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getISessionControl(ISessionControl** aPtrPtrISessionControl)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrISessionControl == nullptr)
            break;

        ICaptureManagerControl* lPtrICaptureManagerControl = nullptr;

        auto lhresult = mCaptureManagerObject->queryInterface(
                    IID_ICaptureManagerControl,
                    (void**)&lPtrICaptureManagerControl);

        if(lhresult != 0)
            break;

        if(lPtrICaptureManagerControl == nullptr)
            break;

        lhresult = lPtrICaptureManagerControl->createControl(
                    IID_ISessionControl,
                    (IUnknown**)aPtrPtrISessionControl);

        lPtrICaptureManagerControl->Release();

        if(lhresult != 0)
            break;

        lresult = true;
    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getISession(QList<IUnknown*> aSourcesList, ISession** aPtrPtrISession)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrISession == nullptr)
            break;

        if(aSourcesList.isEmpty())
            break;

        ISessionControl* lISessionControl;

        if(getISessionControl(&lISessionControl))
        {
            SAFEARRAY *lPtrSAFEARRAY = nullptr;

            SAFEARRAYBOUND lbound[1];

            lbound[0].lLbound = 0;

            lbound[0].cElements = aSourcesList.length();

            lPtrSAFEARRAY = SafeArrayCreate(
                        VT_VARIANT,
                        1,
                        lbound);


            for(LONG lIndex = 0;
                lIndex < aSourcesList.length();
                lIndex++)
            {
                auto lISourceTopologyNode = aSourcesList.at(lIndex);

                VARIANT lVar;

                VariantInit(&lVar);

                lVar.vt = VT_UNKNOWN;

                lVar.punkVal = lISourceTopologyNode;

                SafeArrayPutElement(lPtrSAFEARRAY, &lIndex, &lVar);

            }


             VARIANT lsourceNodesSAFEARRAY;

             VariantInit(&lsourceNodesSAFEARRAY);

             lsourceNodesSAFEARRAY.vt = VT_UNKNOWN | VT_SAFEARRAY;

             lsourceNodesSAFEARRAY.parray = lPtrSAFEARRAY;

             auto lhresult = lISessionControl->createSession(
                         lsourceNodesSAFEARRAY,
                         IID_ISession,
                         (IUnknown**)aPtrPtrISession);

             SafeArrayDestroy(lPtrSAFEARRAY);


             if(lhresult != 0)
                 break;

             lresult = true;

        }

    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getIWebCamControl(QString aSymbolicLink, IWebCamControl** aPtrPtrIWebCamControl)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrIWebCamControl == nullptr)
            break;

        if(aSymbolicLink.isEmpty())
            break;

        ISourceControl* lISourceControl;

        if(getISourceControl(&lISourceControl))
        {
            auto lSize = aSymbolicLink.toStdWString().size();

            std::unique_ptr<OLECHAR> lBSTRSymbolicLink(new OLECHAR[lSize + 1]);

            memcpy(lBSTRSymbolicLink.get(),
                    aSymbolicLink.toStdWString().c_str(),
                    (lSize + 1)*sizeof(OLECHAR));

            lBSTRSymbolicLink.get()[lSize] = '\0';

            auto lhresult= lISourceControl->createSourceControl(
                        lBSTRSymbolicLink.get(),
                        IID_IWebCamControl,
                        (IUnknown**)aPtrPtrIWebCamControl);


             if(lhresult != 0)
                 break;

             lresult = true;

        }

    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getIEncoderControl(IEncoderControl** aPtrPtrIEncoderControl)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrIEncoderControl == nullptr)
            break;

        ICaptureManagerControl* lPtrICaptureManagerControl = nullptr;

        auto lhresult = mCaptureManagerObject->queryInterface(
                    IID_ICaptureManagerControl,
                    (void**)&lPtrICaptureManagerControl);

        if(lhresult != 0)
            break;

        if(lPtrICaptureManagerControl == nullptr)
            break;

        lhresult = lPtrICaptureManagerControl->createControl(
                    IID_IEncoderControl,
                    (IUnknown**)aPtrPtrIEncoderControl);

        lPtrICaptureManagerControl->Release();

        if(lhresult != 0)
            break;

        lresult = true;
    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getIEncoderNodeFactory(IID aEncoderCLSID, IEncoderNodeFactory** aPtrPtrIEncoderNodeFactory)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrPtrIEncoderNodeFactory == nullptr)
            break;

        IEncoderControl* lPtrIEncoderControl = nullptr;

        if(getIEncoderControl(&lPtrIEncoderControl))
        {
            auto lhresult = lPtrIEncoderControl->createEncoderNodeFactory(
                        aEncoderCLSID,
                        IID_IEncoderNodeFactory,
                        (IUnknown**)aPtrPtrIEncoderNodeFactory);

            lPtrIEncoderControl->Release();

            if(lhresult != 0)
                break;
        }


        lresult = true;
    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getOutputNodes(
        QString aFilePath,
        QList<IUnknown*> aCompressedMediaTypeList,
        QList<IUnknown*>& aOutputNodeList,
        IFileSinkFactory* aPtrIFileSinkFactory)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrIFileSinkFactory == nullptr)
            break;

        if(aCompressedMediaTypeList.isEmpty())
            break;

        ISessionControl* lISessionControl;

        if(getISessionControl(&lISessionControl))
        {
            SAFEARRAY *lPtrSAFEARRAY = nullptr;

            SAFEARRAYBOUND lbound[1];

            lbound[0].lLbound = 0;

            lbound[0].cElements = aCompressedMediaTypeList.length();

            lPtrSAFEARRAY = SafeArrayCreate(
                        VT_VARIANT,
                        1,
                        lbound);


            for(LONG lIndex = 0;
                lIndex < aCompressedMediaTypeList.length();
                lIndex++)
            {
                auto lISourceTopologyNode = aCompressedMediaTypeList.at(lIndex);

                VARIANT lVar;

                VariantInit(&lVar);

                lVar.vt = VT_UNKNOWN;

                lVar.punkVal = lISourceTopologyNode;

                SafeArrayPutElement(lPtrSAFEARRAY, &lIndex, &lVar);

            }


             VARIANT lCompressedMediaTypeSAFEARRAY;

             VariantInit(&lCompressedMediaTypeSAFEARRAY);

             lCompressedMediaTypeSAFEARRAY.vt = VT_UNKNOWN | VT_SAFEARRAY;

             lCompressedMediaTypeSAFEARRAY.parray = lPtrSAFEARRAY;

             VARIANT lOutputNodeSAFEARRAY;




             auto lSize = aFilePath.toStdWString().size();

             std::unique_ptr<OLECHAR> lBSTRFilePath(new OLECHAR[lSize + 1]);

             memcpy(lBSTRFilePath.get(),
                     aFilePath.toStdWString().c_str(),
                     (lSize + 1)*sizeof(OLECHAR));

             lBSTRFilePath.get()[lSize] = '\0';


             auto lhresult = aPtrIFileSinkFactory->createOutputNodes(
                         lCompressedMediaTypeSAFEARRAY,
                         lBSTRFilePath.get(),
                         &lOutputNodeSAFEARRAY);

             SafeArrayDestroy(lPtrSAFEARRAY);


             if(lhresult != 0)
                 break;

             if(lOutputNodeSAFEARRAY.vt & VT_UNKNOWN &&
                     lOutputNodeSAFEARRAY.vt & VT_SAFEARRAY)
             {

                 SAFEARRAY* lPtrSAoutputNodes;

                 lPtrSAoutputNodes = lOutputNodeSAFEARRAY.parray;

                 LONG lBoundSourcesNodes;

                 LONG uBoundSourcesNodes;

                 SafeArrayGetUBound(lPtrSAoutputNodes, 1, &uBoundSourcesNodes);

                 SafeArrayGetLBound(lPtrSAoutputNodes, 1, &lBoundSourcesNodes);

                 for (LONG lIndex = lBoundSourcesNodes; lIndex <= uBoundSourcesNodes; lIndex++)
                 {
                     VARIANT lVar;

                     SafeArrayGetElement(lPtrSAoutputNodes, &lIndex, &lVar);

                     if (lVar.vt == VT_UNKNOWN)
                     {
                         aOutputNodeList.push_back(lVar.punkVal);
                     }
                 }

             }

             VariantClear(&lOutputNodeSAFEARRAY);


             lresult = true;

        }

    }
    while(false);

    return lresult;
}

bool CaptureManagerQtProxy::getOutputNodes(
        IUnknown* aPtrByteStream,
        QList<IUnknown*> aCompressedMediaTypeList,
        QList<IUnknown*>& aOutputNodeList,
        IByteStreamSinkFactory* aPtrIByteStreamSinkFactory)
{
    bool lresult = false;

    do
    {
        if(!mIsInitialized)
            break;

        if(aPtrIByteStreamSinkFactory == nullptr)
            break;

        if(aCompressedMediaTypeList.isEmpty())
            break;

        ISessionControl* lISessionControl;

        if(getISessionControl(&lISessionControl))
        {
            SAFEARRAY *lPtrSAFEARRAY = nullptr;

            SAFEARRAYBOUND lbound[1];

            lbound[0].lLbound = 0;

            lbound[0].cElements = aCompressedMediaTypeList.length();

            lPtrSAFEARRAY = SafeArrayCreate(
                        VT_VARIANT,
                        1,
                        lbound);


            for(LONG lIndex = 0;
                lIndex < aCompressedMediaTypeList.length();
                lIndex++)
            {
                auto lISourceTopologyNode = aCompressedMediaTypeList.at(lIndex);

                VARIANT lVar;

                VariantInit(&lVar);

                lVar.vt = VT_UNKNOWN;

                lVar.punkVal = lISourceTopologyNode;

                SafeArrayPutElement(lPtrSAFEARRAY, &lIndex, &lVar);

            }


             VARIANT lCompressedMediaTypeSAFEARRAY;

             VariantInit(&lCompressedMediaTypeSAFEARRAY);

             lCompressedMediaTypeSAFEARRAY.vt = VT_UNKNOWN | VT_SAFEARRAY;

             lCompressedMediaTypeSAFEARRAY.parray = lPtrSAFEARRAY;

             VARIANT lOutputNodeSAFEARRAY;



             auto lhresult = aPtrIByteStreamSinkFactory->createOutputNodes(
                         lCompressedMediaTypeSAFEARRAY,
                         aPtrByteStream,
                         &lOutputNodeSAFEARRAY);

             SafeArrayDestroy(lPtrSAFEARRAY);


             if(lhresult != 0)
                 break;

             if(lOutputNodeSAFEARRAY.vt & VT_UNKNOWN &&
                     lOutputNodeSAFEARRAY.vt & VT_SAFEARRAY)
             {

                 SAFEARRAY* lPtrSAoutputNodes;

                 lPtrSAoutputNodes = lOutputNodeSAFEARRAY.parray;

                 LONG lBoundSourcesNodes;

                 LONG uBoundSourcesNodes;

                 SafeArrayGetUBound(lPtrSAoutputNodes, 1, &uBoundSourcesNodes);

                 SafeArrayGetLBound(lPtrSAoutputNodes, 1, &lBoundSourcesNodes);

                 for (LONG lIndex = lBoundSourcesNodes; lIndex <= uBoundSourcesNodes; lIndex++)
                 {
                     VARIANT lVar;

                     SafeArrayGetElement(lPtrSAoutputNodes, &lIndex, &lVar);

                     if (lVar.vt == VT_UNKNOWN)
                     {
                         aOutputNodeList.push_back(lVar.punkVal);
                     }
                 }

             }

             VariantClear(&lOutputNodeSAFEARRAY);


             lresult = true;

        }

    }
    while(false);

    return lresult;
}
