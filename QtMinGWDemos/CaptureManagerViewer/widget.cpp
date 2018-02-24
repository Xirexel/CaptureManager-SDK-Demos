#include "widget.h"
#include "ui_widget.h"

#include <memory>
#include <QDomDocument>
#include <QXmlQuery>
#include <QXmlResultItems>
#include <QBuffer>
#include <QTextStream>
#include <QUuid>
#include <QMap>


#include "ComPtrCustom.h"
#include "capturemanagerqtproxy.h"
#include "WebCameraViewer.h"
#include "WebCamControlWidget.h"
#include "RecordingWidget.h"

Widget::Widget(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::Widget),
    mWebViewerState(WebViewerState::None)
{
    ui->setupUi(this);


    if(CaptureManagerQtProxy::getInstance().isInitialized())
    {
        initLogOut();

        readSources();

        readSinks();

        QVBoxLayout* lPtrRecordLayout = new QVBoxLayout;

        RecordingWidget* lPtrRecordingWidget = new RecordingWidget;

        lPtrRecordLayout->addWidget(lPtrRecordingWidget);

        ui->tab_2->setLayout(lPtrRecordLayout);
    }

    ui->mRenderTypeComboBox->setCurrentIndex(-1);

    connect(ui->mVideoSourcesComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveSourceCurrentIndex(int)));

    connect(ui->mMediaTypeComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveMediaTypeCurrentIndex(int)));

    connect(ui->mRenderTypeComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveRenderTypeCurrentIndex(int)));

    connect(ui->mPlayPausePushButton, SIGNAL(clicked(bool)), SLOT(play_pauseSlot()));

    connect(ui->mStopPushButton, SIGNAL(clicked(bool)), SLOT(stopSlot()));

    connect(ui->mClosePushButton, SIGNAL(clicked(bool)), SLOT(closeSlot()));

}

Widget::~Widget()
{
    if(mIUnknownISession)
    {
        CComPtrCustom<ISession> lISession;

        mIUnknownISession->QueryInterface(IID_ISession, (void**)&lISession);

        if(lISession)
        {
            lISession->stopSession();

            lISession->closeSession();
        }

        mIUnknownISession.Release();
    }

    QWidget *list = ui->mVideoFrame->findChild<QWidget *>();

    if(list != nullptr)
    {
        list->setParent(nullptr);

        delete list;
    }

    delete ui;

    mOutputNode.Release();

    CaptureManagerQtProxy::getInstance().release();
}

void Widget::initLogOut()
{
    CComPtrCustom<ILogPrintOutControl> lLogPrintOutControl;

    if(CaptureManagerQtProxy::getInstance().getILogPrintOutControl(&lLogPrintOutControl))
    {
        auto lQstringFilePath = QString(QApplication::applicationDirPath() + "/Log.txt");

        auto lSize = lQstringFilePath.toStdWString().size();

        std::unique_ptr<OLECHAR> lFilePath(new OLECHAR[lSize + 1]);

        memcpy(lFilePath.get(),
                lQstringFilePath.toStdWString().c_str(),
                (lSize + 1)*sizeof(OLECHAR));

        lFilePath.get()[lSize] = '\0';

        auto lhresult = lLogPrintOutControl->addPrintOutDestination(
                    1,
                    lFilePath.get());

        if(lhresult == 0)
        {
            lhresult = 0;
        }
    }
}

void Widget::readSources()
{
    CComPtrCustom<ISourceControl> lISourceControl;

    do
    {
        if(CaptureManagerQtProxy::getInstance().getISourceControl(&lISourceControl))
        {
           BSTR lBSTRXMLDocSources = nullptr;

           auto lhresult = lISourceControl->getCollectionOfSources(
                        &lBSTRXMLDocSources);

           if(lhresult != 0)
           {
               delete[] lBSTRXMLDocSources;

               break;
           }

           auto lQstringXMLDocSources = QString().fromWCharArray(lBSTRXMLDocSources);

           fillSourcesCombo(lQstringXMLDocSources);

           delete[] lBSTRXMLDocSources;

        }
    }while(false);
}

void Widget::fillSourcesCombo(QString &aRefQStringXMLDocSources)
{

    do
    {
        QDomDocument ldoc;

        ldoc.setContent(aRefQStringXMLDocSources);

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lSourceNodeList = lrootDocElem.childNodes();

        for(int lSourceIndex = 0;
            lSourceIndex < lSourceNodeList.length();
            lSourceIndex++)
        {
            auto lSourceNode = lSourceNodeList.at(lSourceIndex);

            auto lAttributes = lSourceNode.firstChildElement("Source.Attributes");

            if(lAttributes.isNull())
                continue;

           auto lAttributeNodeList = lAttributes.childNodes();

           bool lIsVideoSource = false;

           QString lFriendlyName;

           for(int lAttrIndex = 0;
               lAttrIndex < lAttributeNodeList.length();
               lAttrIndex++)
           {
               auto lAttribute = lAttributeNodeList.at(lAttrIndex);

               if(lAttribute.attributes().namedItem("Name").nodeValue() == "MF_DEVSOURCE_ATTRIBUTE_MEDIA_TYPE")
               {
                    if(lAttribute.firstChild().firstChild().attributes().namedItem("Value").nodeValue() == "MFMediaType_Video")
                    {
                        lIsVideoSource = true;
                    }
               }
               else if(lAttribute.attributes().namedItem("Name").nodeValue() == "MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME")
               {
                    lFriendlyName = lAttribute.firstChild().attributes().namedItem("Value").nodeValue();
               }
           }

           if(lIsVideoSource)
           {
               QBuffer lbuffer;

               lbuffer.open(QBuffer::ReadWrite);

               QTextStream lTextStream(&lbuffer);

               lSourceNode.save(lTextStream, 1);

               ui->mVideoSourcesComboBox->addItem(lFriendlyName, lbuffer.data());
           }

        }

        ui->mVideoSourcesComboBox->setCurrentIndex(-1);

    }while(false);
}

void Widget::recieveSourceCurrentIndex(int index)
{
    do
    {
        if(index < 0)
            break;

        ui->mMediaTypeComboBox->setEnabled(true);

        ui->mMediaTypeComboBox->clear();

        QDomDocument ldoc;

        ldoc.setContent(ui->mVideoSourcesComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lPresentationDescriptorNode = lrootDocElem.firstChildElement("PresentationDescriptor");

        if(lPresentationDescriptorNode.isNull())
            break;

        auto lStreamDescriptorNode = lPresentationDescriptorNode.firstChildElement("StreamDescriptor");

        if(lStreamDescriptorNode.isNull())
            break;

        auto lMediaTypesNode = lStreamDescriptorNode.firstChildElement("MediaTypes");

        if(lMediaTypesNode.isNull())
            break;

        auto llMediaTypeNodeList = lMediaTypesNode.childNodes();

        for(int lMediaTypeIndex = 0;
            lMediaTypeIndex < llMediaTypeNodeList.length();
            lMediaTypeIndex++)
        {
            auto lMediaTypeNode = llMediaTypeNodeList.at(lMediaTypeIndex);

            if(lMediaTypeNode.isNull())
                continue;

            auto lMediaTypeItemNodeList = lMediaTypeNode.childNodes();

            uint lWidth;

            uint lHeight;

            float lFrameRate;

            QString lSubType;

            QUuid lQUuidSubType;

            auto lMediaTypeInnerIndex = lMediaTypeNode.attributes().namedItem("Index").nodeValue().toUInt();


            for(int llMediaTypeItemIndex = 0;
                llMediaTypeItemIndex < lMediaTypeItemNodeList.length();
                llMediaTypeItemIndex++)
            {
                auto lMediaTypeItemNode = lMediaTypeItemNodeList.at(llMediaTypeItemIndex);

                if(lMediaTypeItemNode.isNull())
                    continue;

                if(lMediaTypeItemNode.attributes().namedItem("Name").nodeValue() == "MF_MT_FRAME_SIZE")
                {
                    auto lValuePartsNode = lMediaTypeItemNode.firstChild();

                    if(!lValuePartsNode.isNull())
                    {
                        auto lFrameSizeNodeList = lValuePartsNode.childNodes();

                        bool lOK = false;

                        auto lValue = lFrameSizeNodeList.at(0).attributes().namedItem("Value").nodeValue().toUInt(&lOK);

                        if(lOK)
                        {
                            lWidth = lValue;
                        }

                        lValue = lFrameSizeNodeList.at(1).attributes().namedItem("Value").nodeValue().toUInt(&lOK);

                        if(lOK)
                        {
                            lHeight = lValue;
                        }
                    }
                }
                else if(lMediaTypeItemNode.attributes().namedItem("Name").nodeValue() == "MF_MT_FRAME_RATE")
                {
                    auto lValuePartsNode = lMediaTypeItemNode.firstChild();

                    bool lOK = false;

                    auto lValue = lValuePartsNode.attributes().namedItem("Value").nodeValue().toFloat(&lOK);

                    if(lOK)
                    {
                        lFrameRate = lValue;
                    }
                }
                else if(lMediaTypeItemNode.attributes().namedItem("Name").nodeValue() == "MF_MT_SUBTYPE")
                {
                    auto lValuePartsNode = lMediaTypeItemNode.firstChild();

                    lSubType = lValuePartsNode.attributes().namedItem("Value").nodeValue();

                    lQUuidSubType = lValuePartsNode.attributes().namedItem("GUID").nodeValue();
                }
            }

            auto lTitle = QString("%1 x %2, %3, %4").arg(lWidth).arg(lHeight).arg(lFrameRate).arg(lSubType);


            QMap<QString, QVariant> lMediaTypeData;

            lMediaTypeData["lWidth"] = lWidth;

            lMediaTypeData["lHeight"] = lHeight;

            lMediaTypeData["lFrameRate"] = lFrameRate;

            lMediaTypeData["lSubType"] = lSubType;

            lMediaTypeData["lQUuidSubType"] = lQUuidSubType;

            lMediaTypeData["lMediaTypeInnerIndex"] = lMediaTypeInnerIndex;

            ui->mMediaTypeComboBox->addItem(lTitle, lMediaTypeData);
        }

        checkAndInitOptions();

    }while(false);
}

void Widget::recieveMediaTypeCurrentIndex(int index)
{
    do
    {
        if(index < 0)
            break;

        ui->mRenderTypeComboBox->setEnabled(true);

        ui->mRenderTypeComboBox->setCurrentIndex(-1);

    }while(false);
}

void Widget::recieveRenderTypeCurrentIndex(int index)
{
    do
    {
        if(index < 0)
            break;

        ui->mControlFrame->setEnabled(true);

        auto lSinkFactoryData = ui->mRenderTypeComboBox->currentData().toMap();

        auto lGUIDSinkFactory = lSinkFactoryData["lGUIDSinkFactory"].toUuid();

        auto lQUuidModeSinkFactory = lSinkFactoryData["lQUuidModeSinkFactory"].toUuid();


        auto lMediaTypeData = ui->mMediaTypeComboBox->currentData().toMap();

        mOutputNode = WebCameraViewer::getViewer(lGUIDSinkFactory,
                                   lQUuidModeSinkFactory,
                                   lMediaTypeData["lWidth"].toUInt(),
                                   lMediaTypeData["lHeight"].toUInt(),
                                   ui->mVideoFrame);


        if(mOutputNode)
        {
            setState(WebViewerState::ReadyToPlay);
        }




    }while(false);
}

void Widget::setState(WebViewerState aWebViewerState)
{
    switch (aWebViewerState) {
    case WebViewerState::ReadyToPlay:
    {
        bool lResult = resolveTopology();

        if(lResult)
        {
            ui->mPlayPausePushButton->setEnabled(true);

            ui->mStopPushButton->setEnabled(false);

            ui->mClosePushButton->setEnabled(false);
        }
    }

        break;
    case WebViewerState::Play:
    {
        {
            ui->mPlayPausePushButton->setText("Pause");

            ui->mPlayPausePushButton->setEnabled(true);

            ui->mStopPushButton->setEnabled(true);

            ui->mClosePushButton->setEnabled(true);

            ui->mVideoSourcesComboBox->setEnabled(false);

            ui->mMediaTypeComboBox->setEnabled(false);

            ui->mRenderTypeComboBox->setEnabled(false);
        }
    }

        break;
    case WebViewerState::Stoped:
    case WebViewerState::Paused:
    {
        {
            ui->mPlayPausePushButton->setText("Play");

            ui->mPlayPausePushButton->setEnabled(true);

            ui->mStopPushButton->setEnabled(true);

            ui->mClosePushButton->setEnabled(true);
        }
    }
         break;
    case WebViewerState::Closed:
    {
        {
            ui->mPlayPausePushButton->setText("Play");

            ui->mPlayPausePushButton->setEnabled(false);

            ui->mStopPushButton->setEnabled(false);

            ui->mClosePushButton->setEnabled(false);

            ui->mVideoSourcesComboBox->setEnabled(true);

            ui->mMediaTypeComboBox->setEnabled(true);

            ui->mRenderTypeComboBox->setEnabled(true);
        }
    }

        break;
    default:
        break;
    }

    mWebViewerState = aWebViewerState;
}

void Widget::readSinks()
{
    do
    {

        CComPtrCustom<ISinkControl> lISinkControl;

        if(CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl))
        {
            BSTR lBSTRXMLDocSinks = nullptr;

            auto lhresult = lISinkControl->getCollectionOfSinks(
                         &lBSTRXMLDocSinks);

            if(lhresult != 0)
            {
                delete[] lBSTRXMLDocSinks;

                break;
            }

            auto lQstringXMLDocSinks = QString().fromWCharArray(lBSTRXMLDocSinks);

            fillSinkCollection(lQstringXMLDocSinks);

            delete[] lBSTRXMLDocSinks;
        }

    }while(false);

}

void Widget::fillSinkCollection(QString &aRefQStringXMLDocSinks)
{

    do
    {
        QDomDocument ldoc;

        ldoc.setContent(aRefQStringXMLDocSinks);

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lSinkFactoryNodeList = lrootDocElem.childNodes();

        for(int lSinkFactoryIndex = 0;
            lSinkFactoryIndex < lSinkFactoryNodeList.length();
            lSinkFactoryIndex++)
        {
            auto lSinkFactoryNode = lSinkFactoryNodeList.at(lSinkFactoryIndex);

            auto lGUIDSinkFactory = lSinkFactoryNode.attributes().namedItem("GUID").nodeValue();



            if (lGUIDSinkFactory == "{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}" ||
                lGUIDSinkFactory == "{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}" ||
                lGUIDSinkFactory == "{2F34AF87-D349-45AA-A5F1-E4104D5C458E}")
            {
                QUuid lQUuidModeSinkFactory = lSinkFactoryNode.firstChild().firstChild().attributes().namedItem("GUID").nodeValue();

                QMap<QString, QVariant> lSinkFactoryData;

                lSinkFactoryData["lGUIDSinkFactory"] = QUuid(lGUIDSinkFactory);

                lSinkFactoryData["lQUuidModeSinkFactory"] = lQUuidModeSinkFactory;

                ui->mRenderTypeComboBox->addItem(
                            lSinkFactoryNode.attributes().namedItem("Title").nodeValue(),
                            lSinkFactoryData);
            }
        }

    }while(false);
}

void Widget::play_pauseSlot()
{
    switch (mWebViewerState) {

    case WebViewerState::ReadyToPlay:
    case WebViewerState::Paused:
    case WebViewerState::Stoped:
    {
        if(mIUnknownISession)
        {
            CComPtrCustom<ISession> lISession;

            auto lhresult = mIUnknownISession->QueryInterface(IID_ISession, (void**)&lISession);

            if(lISession)
            {
                GUID lGUID_NULL = QUuid("{00000000-0000-0000-0000-000000000000}");

                lhresult = lISession->startSession(0, lGUID_NULL);
            }

            if(lhresult != 0)
            {
                lhresult = -1;
            }
        }

        setState(WebViewerState::Play);
    }
        break;
    case WebViewerState::Play:
    {
        if(mIUnknownISession)
        {
            CComPtrCustom<ISession> lISession;

            mIUnknownISession->QueryInterface(IID_ISession, (void**)&lISession);

            if(lISession)
            {
                lISession->pauseSession();
            }
        }

        setState(WebViewerState::Paused);
    }
        break;
    default:
        break;
    }

}

bool Widget::resolveTopology()
{
    bool lresult = false;

    do
    {


        QDomDocument ldoc;

        ldoc.setContent(ui->mVideoSourcesComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lAttributesNode = lrootDocElem.firstChildElement("Source.Attributes");

        if(lAttributesNode.isNull())
            break;

        auto lAttributeNodeList = lAttributesNode.childNodes();

        QString lSymbolicLink;

        for(int lAttrIndex = 0;
            lAttrIndex < lAttributeNodeList.length();
            lAttrIndex++)
        {
            auto lAttributeNode = lAttributeNodeList.at(lAttrIndex);

            if(lAttributeNode.isNull())
                continue;

            if(lAttributeNode.attributes().namedItem("Name").nodeValue() == "MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK")
            {
                lSymbolicLink = lAttributeNode.firstChild().attributes().namedItem("Value").nodeValue();
            }
        }

        if(lSymbolicLink.isNull())
            break;

        auto lMediaTypeData = ui->mMediaTypeComboBox->currentData().toMap();

        auto lMediaTypeInnerIndex = lMediaTypeData["lMediaTypeInnerIndex"].toUInt();

        uint lStreamIndex = 0;


        CComPtrCustom<ISourceControl> lISourceControl;

        if(CaptureManagerQtProxy::getInstance().getISourceControl(&lISourceControl))
        {
            CComPtrCustom<IUnknown> lISourceTopologyNode;

            auto lSize = lSymbolicLink.toStdWString().size();

            std::unique_ptr<OLECHAR> lBSTRSymbolicLink(new OLECHAR[lSize + 1]);

            memcpy(lBSTRSymbolicLink.get(),
                    lSymbolicLink.toStdWString().c_str(),
                    (lSize + 1)*sizeof(OLECHAR));

            lBSTRSymbolicLink.get()[lSize] = '\0';

           auto lhresult = lISourceControl->createSourceNodeWithDownStreamConnection(
                       lBSTRSymbolicLink.get(),
                       lStreamIndex,
                       lMediaTypeInnerIndex,
                       this->mOutputNode,
                       &lISourceTopologyNode);

           if(lhresult != 0)
               break;

           CComPtrCustom<ISession> lISession;

           QList<IUnknown*> lSourcesList;

           lSourcesList.push_back(lISourceTopologyNode);

           if(CaptureManagerQtProxy::getInstance().getISession(lSourcesList, &lISession))
           {
               GUID l = QUuid("{00000000-0000-0000-C000-000000000046}");

               lISession->QueryInterface(l, (void**)&mIUnknownISession);

               lresult = true;
           }



        }

    }while(false);

    return lresult;
}

void Widget::checkAndInitOptions()
{
    do
    {

        QDomDocument ldoc;

        ldoc.setContent(ui->mVideoSourcesComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lAttributesNode = lrootDocElem.firstChildElement("Source.Attributes");

        if(lAttributesNode.isNull())
            break;

        auto lAttributeNodeList = lAttributesNode.childNodes();

        QString lSymbolicLink;

        for(int lAttrIndex = 0;
            lAttrIndex < lAttributeNodeList.length();
            lAttrIndex++)
        {
            auto lAttributeNode = lAttributeNodeList.at(lAttrIndex);

            if(lAttributeNode.isNull())
                continue;

            if(lAttributeNode.attributes().namedItem("Name").nodeValue() == "MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK")
            {
                lSymbolicLink = lAttributeNode.firstChild().attributes().namedItem("Value").nodeValue();
            }
        }

        if(lSymbolicLink.isNull())
            break;

        CComPtrCustom<IWebCamControl> lIWebCamControl;

        if(CaptureManagerQtProxy::getInstance().getIWebCamControl(lSymbolicLink, &lIWebCamControl))
        {
            ui->mOptionsPushButton->setEnabled(true);

            auto lControlDialog = WebCamControlWidget::createWebCamControlWidget(lIWebCamControl);

           // lControlDialog->setParent(ui->mOptionsPushButton);

            connect(ui->mOptionsPushButton, SIGNAL(clicked(bool)), lControlDialog, SLOT(show()));

        }
        else
        {
            ui->mOptionsPushButton->setEnabled(false);
        }




    }while(false);

}

void Widget::stopSlot()
{
    switch (mWebViewerState) {

    case WebViewerState::ReadyToPlay:
    case WebViewerState::Play:
    case WebViewerState::Paused:
    {
        if(mIUnknownISession)
        {
            CComPtrCustom<ISession> lISession;

            auto lhresult = mIUnknownISession->QueryInterface(IID_ISession, (void**)&lISession);

            if(lISession)
            {
                lhresult = lISession->stopSession();
            }

            if(lhresult != 0)
            {
                lhresult = -1;
            }
        }

        setState(WebViewerState::Stoped);
    }
        break;
    default:
        break;
    }


}

void Widget::closeSlot()
{
    switch (mWebViewerState) {

    case WebViewerState::ReadyToPlay:
    case WebViewerState::Play:
    case WebViewerState::Paused:
    case WebViewerState::Stoped:
    {
        if(mIUnknownISession)
        {
            CComPtrCustom<ISession> lISession;

            auto lhresult = mIUnknownISession->QueryInterface(IID_ISession, (void**)&lISession);

            if(lISession)
            {
                lhresult = lISession->closeSession();
            }

            if(lhresult != 0)
            {
                lhresult = -1;
            }
        }

        setState(WebViewerState::Closed);
    }
        break;
    default:
        break;
    }
}
