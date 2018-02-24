#ifndef WEBCAMERAVIEWER_H
#define WEBCAMERAVIEWER_H

#include <QWidget>
#include <QUuid>

#include <unknwnbase.h>

#include "ComPtrCustom.h"

//struct IUnknown;

class WebCameraViewer
{
public:

    static CComPtrCustom<IUnknown> getViewer(QUuid aGUIDSinkFactory, QUuid aQUuidModeSinkFactory, uint aWidth, uint aHeight, QWidget *parent);

private:
    WebCameraViewer() = delete;
};

#endif // WEBCAMERAVIEWER_H
