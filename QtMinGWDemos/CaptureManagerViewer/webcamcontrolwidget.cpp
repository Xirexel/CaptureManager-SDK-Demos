#include "webcamcontrolwidget.h"
#include "capturemanagerqtproxy.h"
#include <QApplication>
#include <QStyle>
#include <QDesktopWidget>
#include <QShowEvent>
#include <QDomDocument>
#include <QGridLayout>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QLabel>
#include <QSlider>
#include <QCheckBox>
#include <QGroupBox>


WebCamControlWidget::WebCamControlWidget(
        IWebCamControl* aPtrIWebCamControl,
        QWidget *parent) : QWidget(parent, Qt::Tool)
{
    setWindowModality(Qt::ApplicationModal);

    if(aPtrIWebCamControl != nullptr)
    {
        auto lhresult = aPtrIWebCamControl->QueryInterface(
                    IID_IWebCamControl,
                    (void**)&mPtrIWebCamControl);

    }

    this->setWindowTitle("Web camera properties");
}

void WebCamControlWidget::showEvent(QShowEvent * event)
{
    if(event->type() == QEvent::Show)
    {
        if(this->layout() == nullptr)
            fillOptions();
    }

    resize(size().width(), sizeHint().height());

    move(QApplication::desktop()->screen()->rect().center() - rect().center());
}

WebCamControlWidget::~WebCamControlWidget()
{
    if(mPtrIWebCamControl != nullptr)
    {
        mPtrIWebCamControl->Release();
    }
}

QWidget* WebCamControlWidget::createWebCamControlWidget(IWebCamControl* aPtrIWebCamControl)
{
    return new WebCamControlWidget(aPtrIWebCamControl);
}

void WebCamControlWidget::fillOptions()
{

    do
    {
        if(mPtrIWebCamControl == nullptr)
        {
            break;
        }

        {

            BSTR lBSTRXMLDocCamParametrs = nullptr;

            auto lhresult = mPtrIWebCamControl->getCamParametrs(
                         &lBSTRXMLDocCamParametrs);

            if(lhresult != 0)
            {
                delete[] lBSTRXMLDocCamParametrs;

                break;
            }

            auto lQstringXMLDocCamParametrs = QString().fromWCharArray(lBSTRXMLDocCamParametrs);

            fillParamentrs(lQstringXMLDocCamParametrs);

            delete[] lBSTRXMLDocCamParametrs;
        }


    }while(false);
}

void WebCamControlWidget::fillParamentrs(QString aQstringXMLDocCamParametrs)
{
    do
    {
        QDomDocument ldoc;

        ldoc.setContent(aQstringXMLDocCamParametrs);

        QDomElement lrootDocElem = ldoc.documentElement();

        auto lGroupNodeList = lrootDocElem.childNodes();

        QVBoxLayout* lPtrGroupQVBoxLayout = new QVBoxLayout();

        for(int lGroupIndex = 0;
            lGroupIndex < lGroupNodeList.length();
            lGroupIndex++)
        {
            auto lGroupNode = lGroupNodeList.at(lGroupIndex);

            if(lGroupNode.isNull())
                continue;

            QString lTitleGroup = lGroupNode.attributes().namedItem("Title").nodeValue();

            auto lParametrsNodeList = lGroupNode.childNodes();


            QGridLayout* lPtrQGridLayout = new QGridLayout();

            for(int lParametrIndex = 0;
                lParametrIndex < lParametrsNodeList.length();
                lParametrIndex++)
            {
                auto lParametrNode = lParametrsNodeList.at(lParametrIndex);

                int lColumnIndex = lParametrIndex % 6;

                int lRowCount = lParametrIndex / 6;

                lPtrQGridLayout->addLayout(makeParametrControl(lParametrNode), lRowCount, lColumnIndex);


            }

            QGroupBox* lPtrQGroupBox = new QGroupBox();

            lPtrQGroupBox->setLayout(lPtrQGridLayout);

            lPtrQGroupBox->setTitle(lTitleGroup);

            QFont lQFont = lPtrQGroupBox->font();

            lQFont.setPointSize(10);

            lPtrQGroupBox->setFont(lQFont);

            lPtrGroupQVBoxLayout->addWidget(lPtrQGroupBox);

        }

        setLayout(lPtrGroupQVBoxLayout);


    }while(false);
}

QLayout* WebCamControlWidget::makeParametrControl(QDomNode aParametrNode)
{
    QLayout* lQLayout = nullptr;

    do
    {

        if(aParametrNode.isNull())
            continue;

        auto lParametrTitle = aParametrNode.attributes().namedItem("Title").nodeValue();

        auto lCurrentValue = aParametrNode.attributes().namedItem("CurrentValue").nodeValue().toInt();

        auto lMinValue = aParametrNode.attributes().namedItem("Min").nodeValue().toInt();

        auto lMaxValue = aParametrNode.attributes().namedItem("Max").nodeValue().toInt();

        auto lStepValue = aParametrNode.attributes().namedItem("Step").nodeValue().toInt();

        auto lFlagValue = aParametrNode.attributes().namedItem("Flag").nodeValue().toInt();

        auto lIndexValue = aParametrNode.attributes().namedItem("Index").nodeValue().toInt();



        QLabel* lPtrQLabel = new QLabel(lParametrTitle);

        QFont lQFont = lPtrQLabel->font();

        lQFont.setPointSize(12);

        lPtrQLabel->setFont(lQFont);

        QVBoxLayout *lPtrQVBoxLayout = new QVBoxLayout();

        lPtrQVBoxLayout->addWidget(lPtrQLabel);

        QSlider* lPtrQSlider = new QSlider;

        lPtrQSlider->setOrientation(Qt::Horizontal);

        lPtrQSlider->setMinimumWidth(60);

        lPtrQSlider->setValue(lCurrentValue);

        lPtrQSlider->setMinimum(lMinValue);

        lPtrQSlider->setMaximum(lMaxValue);

        lPtrQSlider->setSingleStep(lStepValue);

        lPtrQSlider->setTickInterval(0);

        QCheckBox* lQCheckBox = new QCheckBox;

        lQCheckBox->setObjectName(QString("%1").arg(lIndexValue));

        lPtrQSlider->setObjectName(QString("%1").arg(lIndexValue));

        connect(lQCheckBox, SIGNAL(toggled(bool)), lPtrQSlider, SLOT(setEnabled(bool)));

        connect(lQCheckBox, SIGNAL(toggled(bool)), lPtrQLabel, SLOT(setEnabled(bool)));

        lQCheckBox->setChecked(lFlagValue == WebCamParametrFlag::Auto? false: true);

        lQCheckBox->toggled(lFlagValue == WebCamParametrFlag::Auto? false: true);

        connect(lQCheckBox, SIGNAL(toggled(bool)), SLOT(setFlag(bool)));

        connect(lPtrQSlider, SIGNAL(valueChanged(int)), SLOT(sliderValueChanged(int)));


        QHBoxLayout *lPtrQHBoxLayout = new QHBoxLayout();

        lPtrQHBoxLayout->addWidget(lPtrQSlider);

        lPtrQHBoxLayout->addWidget(lQCheckBox);

        lPtrQVBoxLayout->addLayout(lPtrQHBoxLayout);

        lPtrQVBoxLayout->setMargin(10);

        lQLayout = lPtrQVBoxLayout;

    }while(false);

    return lQLayout;
}

void WebCamControlWidget::setFlag(bool aFlagState)
{
    do
    {
        if(mPtrIWebCamControl == nullptr)
        {
            break;
        }

        QCheckBox *lPtrFlagCheckBox= qobject_cast<QCheckBox *>(sender());

        if(lPtrFlagCheckBox == nullptr)
            break;

        auto lIndexValue = lPtrFlagCheckBox->objectName().toInt();

        if(lIndexValue < 0)
            break;


        LONG lCurrentValue;
        LONG lMin;
        LONG lMax;
        LONG lStep;
        LONG lDefault;
        LONG lFlag;

       auto lhresult =  mPtrIWebCamControl->getCamParametr(
                   (DWORD)lIndexValue,
                   &lCurrentValue,
                   &lMin,
                   &lMax,
                   &lStep,
                   &lDefault,
                   &lFlag);

       if(lhresult != 0)
       {
           break;
       }

        if(aFlagState)
        {

            lhresult = mPtrIWebCamControl->setCamParametr(
                        (DWORD)lIndexValue, lCurrentValue, WebCamParametrFlag::Manual);

            if(lhresult != 0)
            {
                break;
            }
        }
        else
        {

            lhresult = mPtrIWebCamControl->setCamParametr(
                        (DWORD)lIndexValue, lDefault, WebCamParametrFlag::Auto);

            if(lhresult != 0)
            {
                break;
            }

        }

    }while(false);
}

void WebCamControlWidget::sliderValueChanged(int value)
{
    do
    {
        if(mPtrIWebCamControl == nullptr)
        {
            break;
        }

        QSlider *lPtrSlider= qobject_cast<QSlider *>(sender());

        if(lPtrSlider == nullptr)
            break;

        auto lIndexValue = lPtrSlider->objectName().toInt();

        if(lIndexValue < 0)
            break;

        if(lPtrSlider->isEnabled())
        {
            auto lhresult = mPtrIWebCamControl->setCamParametr(
                        (DWORD)lIndexValue, (LONG)lPtrSlider->value(), WebCamParametrFlag::Manual);

            if(lhresult != 0)
            {
                break;
            }
        }

    }while(false);

}
