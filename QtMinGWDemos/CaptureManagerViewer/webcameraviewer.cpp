#include <QGridLayout>


#include "webcameraviewer.h"
#include "VideoSampleGrabberCallViewer.h"
#include "VideoSampleGrabberCallbackViewer.h"
#include "evrviewer.h"

CComPtrCustom<IUnknown> WebCameraViewer::getViewer(
        QUuid aGUIDSinkFactory,
        QUuid aQUuidModeSinkFactory,
        uint aWidth,
        uint aHeight,
        QWidget *parent)
{
    CComPtrCustom<IUnknown> lresult;

    QWidget* lWebViewerWidget = nullptr;

    do
    {

        auto lPrevLayout = parent->layout();

        if(lPrevLayout != nullptr)
        {
            delete lPrevLayout;

            QList<QWidget *> lwidgets = parent->findChildren<QWidget*>("EVRViewer");

            for(auto& lItem: lwidgets)
            {
                lItem->setParent(nullptr);

                delete lItem;
            }
        }

        if (aGUIDSinkFactory == "{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}")
        {
            lWebViewerWidget = new VideoSampleGrabberCallViewer(
                        aQUuidModeSinkFactory,
                        aWidth,
                        aHeight,
                        &lresult);

        }
        else if (aGUIDSinkFactory == "{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}")
        {
            lWebViewerWidget = new VideoSampleGrabberCallbackViewer(
                        aQUuidModeSinkFactory,
                        aWidth,
                        aHeight,
                        &lresult);

        }
        else if (aGUIDSinkFactory == "{2F34AF87-D349-45AA-A5F1-E4104D5C458E}")
        {

            QWindow *lEVRWindow = new EVRViewer(
                        aQUuidModeSinkFactory,
                        aWidth,
                        aHeight,
                        &lresult,
                        parent);


            lWebViewerWidget = QWidget::createWindowContainer( lEVRWindow,  parent);

            lWebViewerWidget->setObjectName("EVRViewer");

        }
        if(lWebViewerWidget == nullptr)
            break;

        QVBoxLayout *layout = new QVBoxLayout;

        layout->addWidget(lWebViewerWidget);

        parent->setLayout(layout);

    }while(false);

    return lresult;
}

