#include "recordingwidget.h"
#include "ui_recordingwidget.h"

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
#include "filesinkwidget.h"
#include "networkstreamsink.h"


CComPtrCustom<ISession> lISession;

RecordingWidget::RecordingWidget(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::RecordingWidget)
{
    ui->setupUi(this);

    readSources();

    connect(ui->mSourcesComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveSourceCurrentIndex(int)));

    connect(ui->mStreamsComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveStreamCurrentIndex(int)));

    connect(ui->mMediaTypeComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveMediaTypeCurrentIndex(int)));

    connect(ui->mEncoderComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveEncoderCurrentIndex(int)));

    connect(ui->mEncoderModeComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveEncoderModeCurrentIndex(int)));

    connect(ui->mCompressedMediaTypeComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveCompressedMediaTypeCurrentIndex(int)));

    connect(ui->mSinkComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveSinkFactoryCurrentIndex(int)));

    connect(ui->mContainerComboBox, SIGNAL(currentIndexChanged(int)), SLOT(recieveContainerCurrentIndex(int)));

    connect(ui->mStartPushButton, SIGNAL(clicked(bool)), SLOT(startRecording()));
}

RecordingWidget::~RecordingWidget()
{
    delete ui;
}

void RecordingWidget::readSources()
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

void RecordingWidget::fillSourcesCombo(QString &aRefQStringXMLDocSources)
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

           QString lFriendlyName;

           for(int lAttrIndex = 0;
               lAttrIndex < lAttributeNodeList.length();
               lAttrIndex++)
           {
               auto lAttribute = lAttributeNodeList.at(lAttrIndex);

               if(lAttribute.attributes().namedItem("Name").nodeValue() == "MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME")
               {
                    lFriendlyName = lAttribute.firstChild().attributes().namedItem("Value").nodeValue();
               }
           }

           QBuffer lbuffer;

           lbuffer.open(QBuffer::ReadWrite);

           QTextStream lTextStream(&lbuffer);

           lSourceNode.save(lTextStream, 1);

           ui->mSourcesComboBox->addItem(lFriendlyName, lbuffer.data());

        }

        ui->mSourcesComboBox->setCurrentIndex(-1);

        if(ui->mSourcesComboBox->count() > 0)
            ui->mSourcesComboBox->setEnabled(true);

    }while(false);
}

void RecordingWidget::recieveSourceCurrentIndex(int index)
{
    do
    {
        if(index < 0)
            break;

        ui->mStreamsComboBox->clear();

        QDomDocument ldoc;

        ldoc.setContent(ui->mSourcesComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lPresentationDescriptorNode = lrootDocElem.firstChildElement("PresentationDescriptor");

        if(lPresentationDescriptorNode.isNull())
            break;

        auto lChildNodeList = lPresentationDescriptorNode.childNodes();

        for(int lIndex = 0;
            lIndex < lChildNodeList.length();
            lIndex++)
        {
            auto lChildNode = lChildNodeList.at(lIndex);

            if(lChildNode.nodeName() == "StreamDescriptor")
            {
                auto lMajorTypeValue = lChildNode.attributes().namedItem("MajorType").nodeValue();

                auto lParts = lMajorTypeValue.split("_");

                QBuffer lbuffer;

                lbuffer.open(QBuffer::ReadWrite);

                QTextStream lTextStream(&lbuffer);

                lChildNode.save(lTextStream, 1);

                ui->mStreamsComboBox->addItem(lParts.at(lParts.length() - 1), lbuffer.data());
            }

        }

        ui->mStreamsComboBox->setCurrentIndex(-1);
        if(ui->mStreamsComboBox->count() > 0)
            ui->mStreamsComboBox->setEnabled(true);

    }while(false);
}

void RecordingWidget::recieveStreamCurrentIndex(int index)
{
    do
    {

        ui->mMediaTypeComboBox->clear();

        if(index < 0)
        {
            ui->mMediaTypeComboBox->setEnabled(false);

            break;
        }

        QDomDocument ldoc;

        ldoc.setContent(ui->mStreamsComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lMediaTypesNode = lrootDocElem.firstChildElement("MediaTypes");

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

        ui->mMediaTypeComboBox->setCurrentIndex(-1);

        if(ui->mMediaTypeComboBox->count() > 0)
            ui->mMediaTypeComboBox->setEnabled(true);


    }while(false);
}

void RecordingWidget::recieveMediaTypeCurrentIndex(int index)
{
    do
    {
        ui->mEncoderComboBox->clear();

        ui->mEncoderComboBox->setCurrentIndex(-1);

        if(index < 0)
        {

            ui->mEncoderComboBox->setEnabled(false);

            break;
        }

        CComPtrCustom<IEncoderControl> lIEncoderControl;

        if(CaptureManagerQtProxy::getInstance().getIEncoderControl(&lIEncoderControl))
        {
            BSTR lBSTRXMLDocEncoders = nullptr;

            auto lhresult = lIEncoderControl->getCollectionOfEncoders(&lBSTRXMLDocEncoders);

            if(lhresult != 0)
            {
                delete[] lBSTRXMLDocEncoders;

                break;
            }

             auto lQstringXMLDocEncoders = QString().fromWCharArray(lBSTRXMLDocEncoders);


             delete[] lBSTRXMLDocEncoders;




             QDomDocument ldoc;

             ldoc.setContent(ui->mStreamsComboBox->currentData().toByteArray());

             QDomElement lrootDocElem = ldoc.documentElement();

             auto lMajorTypeGUIDQString = lrootDocElem.attribute("MajorTypeGUID");

             if(lMajorTypeGUIDQString.isNull())
                 break;




             ldoc.setContent(lQstringXMLDocEncoders);

             lrootDocElem = ldoc.documentElement();

             auto lGroupNodeList = lrootDocElem.childNodes();

             for(int lGroupIndex = 0;
                 lGroupIndex < lGroupNodeList.length();
                 lGroupIndex++)
             {
                 auto lGroupNode = lGroupNodeList.at(lGroupIndex);

                 if(lGroupNode.isNull())
                     continue;

                 if(lGroupNode.attributes().namedItem("GUID").nodeValue() == lMajorTypeGUIDQString)
                 {
                    auto lEncoderFactoryNodeList = lGroupNode.childNodes();

                    for(int lEncoderIndex = 0;
                        lEncoderIndex < lEncoderFactoryNodeList.length();
                        lEncoderIndex++)
                    {
                        auto lEncoderFactoryNode = lEncoderFactoryNodeList.at(lEncoderIndex);

                        if(lEncoderFactoryNode.isNull())
                            continue;

                        auto lTitleValue = lEncoderFactoryNode.attributes().namedItem("Title").nodeValue();

                        auto lCLSIDValue = lEncoderFactoryNode.attributes().namedItem("CLSID").nodeValue();

                        ui->mEncoderComboBox->addItem(lTitleValue, lCLSIDValue);
                    }
                 }
             }

        }

        ui->mEncoderComboBox->setCurrentIndex(-1);

        if(ui->mEncoderComboBox->count() > 0)
            ui->mEncoderComboBox->setEnabled(true);

    }while(false);
}

void RecordingWidget::recieveEncoderCurrentIndex(int index)
{
    do
    {
        ui->mEncoderModeComboBox->clear();

        ui->mEncoderModeComboBox->setCurrentIndex(-1);

        if(index < 0)
        {

            ui->mEncoderModeComboBox->setEnabled(false);

            break;
        }

        CComPtrCustom<IEncoderControl> lIEncoderControl;

        if(CaptureManagerQtProxy::getInstance().getIEncoderControl(&lIEncoderControl))
        {
            auto lCLSIDValue = ui->mEncoderComboBox->currentData().toString();

            QUuid lCLSID(lCLSIDValue);

            CComPtrCustom<ISourceControl> lISourceControl;

            if(CaptureManagerQtProxy::getInstance().getISourceControl(&lISourceControl))
            {

                QDomDocument ldoc;

                ldoc.setContent(ui->mSourcesComboBox->currentData().toByteArray());

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

                        break;
                    }
                    else if(lAttributeNode.attributes().namedItem("Name").nodeValue() == "MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK")
                    {
                        lSymbolicLink = lAttributeNode.firstChild().attributes().namedItem("Value").nodeValue();

                        break;
                    }
                }

                if(lSymbolicLink.isNull())
                    break;

                auto lSize = lSymbolicLink.toStdWString().size();

                std::unique_ptr<OLECHAR> lBSTRSymbolicLink(new OLECHAR[lSize + 1]);

                memcpy(lBSTRSymbolicLink.get(),
                        lSymbolicLink.toStdWString().c_str(),
                        (lSize + 1)*sizeof(OLECHAR));

                lBSTRSymbolicLink.get()[lSize] = '\0';


                ldoc.setContent(ui->mStreamsComboBox->currentData().toByteArray());

                lrootDocElem = ldoc.documentElement();

                bool lOK = false;

                auto lStreamIndex = lrootDocElem.attribute("Index").toUInt(&lOK);

                if(!lOK)
                    break;


                auto lMediaTypeData = ui->mMediaTypeComboBox->currentData().toMap();

                auto lMediaTypeInnerIndex = lMediaTypeData["lMediaTypeInnerIndex"].toUInt(&lOK);

                if(!lOK)
                    break;

                CComPtrCustom<IUnknown> lOutputMediaType;

                auto lhresult = lISourceControl->getSourceOutputMediaType(
                            lBSTRSymbolicLink.get(),
                            (DWORD)lStreamIndex,
                            (DWORD)lMediaTypeInnerIndex,
                            &lOutputMediaType);

                if(lhresult != 0)
                {
                     break;
                }

                BSTR lBSTRXMLDocEncoderMediaTypeCollection = nullptr;

                lhresult = lIEncoderControl->getMediaTypeCollectionOfEncoder(
                            lOutputMediaType,
                            lCLSID,
                            &lBSTRXMLDocEncoderMediaTypeCollection);

                if(lhresult != 0)
                {
                    delete[] lBSTRXMLDocEncoderMediaTypeCollection;

                    break;
                }

                auto lQstringXMLDocEncoderMediaTypeCollection = QString().fromWCharArray(lBSTRXMLDocEncoderMediaTypeCollection);

                delete[] lBSTRXMLDocEncoderMediaTypeCollection;

                ldoc.setContent(lQstringXMLDocEncoderMediaTypeCollection);

                lrootDocElem = ldoc.documentElement();

                auto lGroupNodeList = lrootDocElem.childNodes();

                for(int lGroupIndex = 0;
                    lGroupIndex < lGroupNodeList.length();
                    lGroupIndex++)
                {
                    auto lGroupNode = lGroupNodeList.at(lGroupIndex);

                    auto lTitleValue = lGroupNode.attributes().namedItem("Title").nodeValue();



                    QBuffer lbuffer;

                    lbuffer.open(QBuffer::ReadWrite);

                    QTextStream lTextStream(&lbuffer);

                    lGroupNode.save(lTextStream, 1);


                    ui->mEncoderModeComboBox->addItem(lTitleValue, lbuffer.data());
                }

                ui->mEncoderModeComboBox->setCurrentIndex(-1);

                if(ui->mEncoderModeComboBox->count() > 0)
                    ui->mEncoderModeComboBox->setEnabled(true);
            }
        }


    }while(false);
}

void RecordingWidget::recieveEncoderModeCurrentIndex(int index)
{
    do
    {
        ui->mCompressedMediaTypeComboBox->clear();

        ui->mCompressedMediaTypeComboBox->setCurrentIndex(-1);

        if(index < 0)
        {

            ui->mCompressedMediaTypeComboBox->setEnabled(false);

            break;
        }


        QDomDocument ldoc;

        ldoc.setContent(ui->mEncoderModeComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lMediaTypesNode = lrootDocElem.firstChildElement("MediaTypes");

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

            ui->mCompressedMediaTypeComboBox->addItem(lTitle, lMediaTypeData);
        }



        ui->mCompressedMediaTypeComboBox->setCurrentIndex(-1);

        if(ui->mCompressedMediaTypeComboBox->count() > 0)
            ui->mCompressedMediaTypeComboBox->setEnabled(true);


    }while(false);
}

void RecordingWidget::recieveCompressedMediaTypeCurrentIndex(int index)
{
    do
    {
        ui->mSinkComboBox->clear();

        ui->mSinkComboBox->setCurrentIndex(-1);

        if(index < 0)
        {

            ui->mSinkComboBox->setEnabled(false);

            break;
        }

        CComPtrCustom<ISinkControl> lISinkControl;

        if(CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl))
        {

            BSTR lBSTRXMLDocSinkCollection = nullptr;

            auto lhresult = lISinkControl->getCollectionOfSinks(
                        &lBSTRXMLDocSinkCollection);

            if(lhresult != 0)
            {
                delete[] lBSTRXMLDocSinkCollection;

                break;
            }

            auto lQstringXMLDocSinkCollection = QString().fromWCharArray(lBSTRXMLDocSinkCollection);

            delete[] lBSTRXMLDocSinkCollection;


            QDomDocument ldoc;

            ldoc.setContent(lQstringXMLDocSinkCollection);

            QDomElement lrootDocElem = ldoc.documentElement();

            auto lSinkFactoryNodeList = lrootDocElem.childNodes();

            for(int lSinkFactoryIndex = 0;
                lSinkFactoryIndex < lSinkFactoryNodeList.length();
                lSinkFactoryIndex++)
            {
                auto lSinkFactoryNode = lSinkFactoryNodeList.at(lSinkFactoryIndex);

                if(lSinkFactoryNode.isNull())
                    continue;

                auto lGUIDSinkFactory = lSinkFactoryNode.attributes().namedItem("GUID").nodeValue();

                if (lGUIDSinkFactory == "{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}" ||
                    lGUIDSinkFactory == "{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}" ||
                    lGUIDSinkFactory == "{2F34AF87-D349-45AA-A5F1-E4104D5C458E}")
                {
                    continue;
                }

                auto lTitleValue = lSinkFactoryNode.attributes().namedItem("Title").nodeValue();


                QBuffer lbuffer;

                lbuffer.open(QBuffer::ReadWrite);

                QTextStream lTextStream(&lbuffer);

                lSinkFactoryNode.save(lTextStream, 1);

                ui->mSinkComboBox->addItem(lTitleValue, lbuffer.data());
            }

        }

        ui->mSinkComboBox->setCurrentIndex(-1);

        if(ui->mSinkComboBox->count() > 0)
            ui->mSinkComboBox->setEnabled(true);

    }while(false);
}

void RecordingWidget::recieveSinkFactoryCurrentIndex(int index)
{
    do
    {
        ui->mContainerComboBox->clear();

        ui->mContainerComboBox->setCurrentIndex(-1);

        if(index < 0)
        {

            ui->mContainerComboBox->setEnabled(false);

            break;
        }


        QDomDocument ldoc;

        ldoc.setContent(ui->mSinkComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto ValuePartsNode = lrootDocElem.firstChildElement("Value.ValueParts");

        auto lSinkFactoryNodeList = ValuePartsNode.childNodes();

        for(int llSinkFactoryIndex = 0;
            llSinkFactoryIndex < lSinkFactoryNodeList.length();
            llSinkFactoryIndex++)
        {
            auto lSinkFactoryNode = lSinkFactoryNodeList.at(llSinkFactoryIndex);

            auto lTitleValue = lSinkFactoryNode.attributes().namedItem("Value").nodeValue();





            QBuffer lbuffer;

            lbuffer.open(QBuffer::ReadWrite);

            QTextStream lTextStream(&lbuffer);

            lSinkFactoryNode.save(lTextStream, 1);

            ui->mContainerComboBox->addItem(lTitleValue, lbuffer.data());
        }


        ui->mContainerComboBox->setCurrentIndex(-1);

        if(ui->mContainerComboBox->count() > 0)
            ui->mContainerComboBox->setEnabled(true);

    }while(false);
}

void RecordingWidget::recieveContainerCurrentIndex(int index)
{
    do
    {
        if(index < 0)
        {

            ui->mStartPushButton->setEnabled(false);

            break;
        }


        QDomDocument ldoc;

        ldoc.setContent(ui->mSinkComboBox->currentData().toByteArray());

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lSinkGUIDValue = lrootDocElem.attribute("GUID");

        if(lSinkGUIDValue == "{D6E342E3-7DDD-4858-AB91-4253643864C2}")
        {

            ldoc.setContent(ui->mContainerComboBox->currentData().toByteArray());

            QDomElement lrootDocElem = ldoc.documentElement();

            if(!lrootDocElem.isNull())
            {
                auto lGUIDValue = lrootDocElem.attribute("GUID");

                mSink.reset(new FileSinkWidget(lGUIDValue, this));

                ui->mStartPushButton->setEnabled(true);
            }
        }
        else if(lSinkGUIDValue == "{2E891049-964A-4D08-8F36-95CE8CB0DE9B}")
        {

            ldoc.setContent(ui->mContainerComboBox->currentData().toByteArray());

            QDomElement lrootDocElem = ldoc.documentElement();

            if(!lrootDocElem.isNull())
            {
                auto lGUIDValue = lrootDocElem.attribute("GUID");

                auto lMIMEValue = lrootDocElem.attribute("MIME");

                mSink.reset(new NetworkStreamSink(lGUIDValue, lMIMEValue, this));

                ui->mStartPushButton->setEnabled(true);
            }
        }





    }while(false);
}


void RecordingWidget::startRecording()
{
    if(mSink)
    {
        if(ui->mStartPushButton->text() == "Close")
        {
            if(lISession)
                lISession->closeSession();

            lISession.Release();

            ui->mStartPushButton->setText("Select file");

            return;
        }

        mSink->setOptions();

        do
        {

            CComPtrCustom<IUnknown> lISourceTopologyNode;

            CComPtrCustom<IEncoderControl> lIEncoderControl;

            if(CaptureManagerQtProxy::getInstance().getIEncoderControl(&lIEncoderControl))
            {
                auto lCLSIDValue = ui->mEncoderComboBox->currentData().toString();

                QUuid lCLSID(lCLSIDValue);

                CComPtrCustom<ISourceControl> lISourceControl;

                if(CaptureManagerQtProxy::getInstance().getISourceControl(&lISourceControl))
                {

                    QDomDocument ldoc;

                    ldoc.setContent(ui->mSourcesComboBox->currentData().toByteArray());

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

                            break;
                        }
                        else if(lAttributeNode.attributes().namedItem("Name").nodeValue() == "MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK")
                        {
                            lSymbolicLink = lAttributeNode.firstChild().attributes().namedItem("Value").nodeValue();

                            break;
                        }
                    }

                    if(lSymbolicLink.isNull())
                        break;

                    auto lSize = lSymbolicLink.toStdWString().size();

                    std::unique_ptr<OLECHAR> lBSTRSymbolicLink(new OLECHAR[lSize + 1]);

                    memcpy(lBSTRSymbolicLink.get(),
                            lSymbolicLink.toStdWString().c_str(),
                            (lSize + 1)*sizeof(OLECHAR));

                    lBSTRSymbolicLink.get()[lSize] = '\0';


                    ldoc.setContent(ui->mStreamsComboBox->currentData().toByteArray());

                    lrootDocElem = ldoc.documentElement();

                    bool lOK = false;

                    auto lStreamIndex = lrootDocElem.attribute("Index").toUInt(&lOK);

                    if(!lOK)
                        break;


                    auto lMediaTypeData = ui->mMediaTypeComboBox->currentData().toMap();

                    auto lMediaTypeInnerIndex = lMediaTypeData["lMediaTypeInnerIndex"].toUInt(&lOK);

                    if(!lOK)
                        break;

                    CComPtrCustom<IUnknown> lOutputMediaType;

                    auto lhresult = lISourceControl->getSourceOutputMediaType(
                                lBSTRSymbolicLink.get(),
                                (DWORD)lStreamIndex,
                                (DWORD)lMediaTypeInnerIndex,
                                &lOutputMediaType);

                    if(lhresult != 0)
                    {
                         break;
                    }

                    CComPtrCustom<IUnknown> lCompressedMediaType;

                    CComPtrCustom<IEncoderNodeFactory> lIEncoderNodeFactory;

                    if(CaptureManagerQtProxy::getInstance().getIEncoderNodeFactory(
                                lCLSID,
                                &lIEncoderNodeFactory))
                    {

                        ldoc.setContent(ui->mEncoderModeComboBox->currentData().toByteArray());

                        QDomElement lrootDocElem = ldoc.documentElement();

                        auto lGUIDEncoderMode = lrootDocElem.attribute("GUID");

                        if(lGUIDEncoderMode.isNull())
                            break;




                        auto lMediaTypeData = ui->mCompressedMediaTypeComboBox->currentData().toMap();


                        auto lMediaTypeInnerIndex = lMediaTypeData["lMediaTypeInnerIndex"].toUInt();


                        lhresult = lIEncoderNodeFactory->createCompressedMediaType(
                                    lOutputMediaType,
                                    QUuid(lGUIDEncoderMode),
                                    75,
                                    lMediaTypeInnerIndex,
                                    &lCompressedMediaType);

                        if(lhresult != 0)
                        {
                             break;
                        }


                        CComPtrCustom<IUnknown> loutputnode(mSink->getOutputNode(lCompressedMediaType));

                        if(!loutputnode)
                            break;

                        CComPtrCustom<IUnknown> lEncoderNode;

                        lhresult = lIEncoderNodeFactory->createEncoderNode(
                                    lOutputMediaType,
                                    QUuid(lGUIDEncoderMode),
                                    75,
                                    lMediaTypeInnerIndex,
                                    loutputnode,
                                    &lEncoderNode);

                        if(lhresult != 0)
                        {
                             break;
                        }

                        auto lhresult = lISourceControl->createSourceNodeWithDownStreamConnection(
                                    lBSTRSymbolicLink.get(),
                                    (DWORD)lStreamIndex,
                                    (DWORD)lMediaTypeInnerIndex,
                                    lEncoderNode,
                                    &lISourceTopologyNode);

                        if(lhresult != 0)
                        {
                             break;
                        }
                    }
                }
            }




            QList<IUnknown*> lSourcesList;

            lSourcesList.push_back(lISourceTopologyNode);

            if(CaptureManagerQtProxy::getInstance().getISession(lSourcesList, &lISession))
            {
                GUID lGUID_NULL = QUuid("{00000000-0000-0000-0000-000000000000}");

                ui->mStartPushButton->setText("Close");

                lISession->startSession(0, lGUID_NULL);
            }

        }while(false);

    }
}


