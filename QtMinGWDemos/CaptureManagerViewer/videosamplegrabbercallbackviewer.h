#ifndef VIDEOSAMPLEGRABBERCALLBACKVIEWER_H
#define VIDEOSAMPLEGRABBERCALLBACKVIEWER_H

#include <memory>
#include "capturemanagerqtproxy.h"
#include "ComPtrCustom.h"
#include "openglvideoviewer.h"

class VideoSampleGrabberCallbackViewer : public OpenGLVideoViewer
{
public:
    VideoSampleGrabberCallbackViewer(QUuid aContainerTypeQUuid, uint aWidth, uint aHeight, IUnknown **aPtrPtrOutputNode, QWidget *parent = 0);

    virtual ~VideoSampleGrabberCallbackViewer();

    void refresh(/* [in] */ LPVOID aPtrSampleBuffer,
                 /* [in] */ DWORD aSampleSize);

protected:

    virtual void reloadVideoFrame();

signals:

public slots:

private:

    std::unique_ptr<unsigned char> mData;

};

#endif // VIDEOSAMPLEGRABBERCALLBACKVIEWER_H
