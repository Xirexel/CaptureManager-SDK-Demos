#include "evrviewer.h"
#include "ComPtrCustom.h"
#include "capturemanagerqtproxy.h"
#include <QWidget>


EVRViewer::EVRViewer(QUuid aContainerTypeQUuid, uint aWidth, uint aHeight, IUnknown **aPtrPtrOutputNode, QWidget *parent)// : QWidget(parent)
{


    resize(parent->size());


    CComPtrCustom<ISinkControl> lISinkControl;

    if(CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl))
    {
        CComPtrCustom<IEVRSinkFactory> lIEVRSinkFactory;

        auto lhresult = lISinkControl->createSinkFactory(
                    aContainerTypeQUuid,
                    IID_IEVRSinkFactory,
                    (IUnknown**)&lIEVRSinkFactory);

        if(lhresult == 0)
        {
            lIEVRSinkFactory->createOutputNode(
                        (LPVOID)this->winId(),
                        aPtrPtrOutputNode);
        }
    }
}
