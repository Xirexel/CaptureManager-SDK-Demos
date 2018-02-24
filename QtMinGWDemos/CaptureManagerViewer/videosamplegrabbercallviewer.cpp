#include "videosamplegrabbercallviewer.h"
#include <QOpenGLContext>
#include <QMatrix4x4>
#include <QOpenGLShader>
#include <QOpenGLShaderProgram>
#include <QTimer>




VideoSampleGrabberCallViewer::VideoSampleGrabberCallViewer(
        QUuid aContainerTypeQUuid,
        uint aWidth,
        uint aHeight,
        IUnknown** aPtrPtrOutputNode,
        QWidget *parent) : OpenGLVideoViewer(parent)
{
    mWidth = aWidth;

    mHeight = aHeight;

    CComPtrCustom<ISinkControl> lISinkControl;

    if(CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl))
    {
        auto lhresult = lISinkControl->createSinkFactory(
                    aContainerTypeQUuid,
                    IID_ISampleGrabberCallSinkFactory,
                    (IUnknown**)&mISampleGrabberCallSinkFactory);

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

                    lhresult = mISampleGrabberCallSinkFactory->createOutputNode(
                                MFMediaType_Video,
                                MFVideoFormat_RGB24,
                                lSampleByteSize,
                                IID_ISampleGrabberCall,
                                (IUnknown**)&mISampleGrabberCall);


                    if(lhresult == 0)
                        mISampleGrabberCall->QueryInterface(aPtrPtrOutputNode);
                }
            }

        }
    }

    QTimer *timer = new QTimer(this);
    connect(timer, SIGNAL(timeout()), SLOT(update()));
    timer->start(33);
}

VideoSampleGrabberCallViewer::~VideoSampleGrabberCallViewer()
{
    mISampleGrabberCallSinkFactory.Release();

    mISampleGrabberCall.Release();
}

void VideoSampleGrabberCallViewer::reloadVideoFrame()
{
    DWORD lDataLength;

    auto lReadState = mISampleGrabberCall->readData(mData.get(), &lDataLength);

    glBindTexture(GL_TEXTURE_2D, mDrawTextureID);

    if (SUCCEEDED(lReadState))
        glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, mWidth, mHeight, GL_BGR, GL_UNSIGNED_BYTE, mData.get());
}
