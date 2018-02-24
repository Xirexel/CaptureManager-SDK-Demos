#ifndef VIDEOSAMPLEGRABBERCALLVIEWER_H
#define VIDEOSAMPLEGRABBERCALLVIEWER_H

#include <memory>
#include "capturemanagerqtproxy.h"
#include "ComPtrCustom.h"
#include "openglvideoviewer.h"

class VideoSampleGrabberCallViewer : public OpenGLVideoViewer
{
    Q_OBJECT
public:
    explicit VideoSampleGrabberCallViewer(QUuid aContainerTypeQUuid, uint aWidth, uint aHeight, IUnknown **aPtrPtrOutputNode, QWidget *parent = 0);

    virtual ~VideoSampleGrabberCallViewer();

protected:

    virtual void reloadVideoFrame();

signals:

public slots:

private:

    CComPtrCustom<ISampleGrabberCallSinkFactory> mISampleGrabberCallSinkFactory;

    CComPtrCustom<ISampleGrabberCall> mISampleGrabberCall;

    std::unique_ptr<unsigned char> mData;
};

#endif // VIDEOSAMPLEGRABBERCALLVIEWER_H
