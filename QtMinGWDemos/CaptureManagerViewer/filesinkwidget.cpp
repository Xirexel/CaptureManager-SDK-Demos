#include "filesinkwidget.h"
#include "ui_filesinkwidget.h"

#include <QUuid>
#include <QApplication>
#include <QDesktopWidget>
#include <QFileDialog>

#include "CaptureManagerQtProxy.h"
#include "ComPtrCustom.h"

FileSinkWidget::FileSinkWidget(QString aGUIDValue, QWidget *parent) :
    QWidget(parent, Qt::Dialog),
    ui(new Ui::FileSinkWidget),
    mGUIDValue(aGUIDValue)
{
    setWindowModality(Qt::ApplicationModal);

    ui->setupUi(this);

    connect(ui->mSelectFilePushButton, SIGNAL(clicked(bool)), SLOT(selectFile()));
}

FileSinkWidget::~FileSinkWidget()
{
    delete ui;
}

void FileSinkWidget::selectFile()
{
    mfilePath = QFileDialog::getSaveFileName(this,
                                 "Save media file",
                                 "/home"
                                 );

}

void FileSinkWidget::setOptions()
{
    do
    {

        selectFile();

    }while(false);
}

IUnknown* FileSinkWidget::getOutputNode(IUnknown* aUpStreamMediaType)
{
    IUnknown* lptrResult = nullptr;

    do
    {
        CComPtrCustom<ISinkControl> lISinkControl;

        CComPtrCustom<IFileSinkFactory> lIFileSinkFactory;

        if(CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl))
        {
            QUuid lIID_IFileSinkFactory(0xD6E342E3,0x7DDD,0x4858,0xAB,0x91,0x42,0x53,0x64,0x38,0x64,0xC2);

            auto lhresult = lISinkControl->createSinkFactory(
                        QUuid(mGUIDValue),
                        lIID_IFileSinkFactory,
                        (IUnknown**)&lIFileSinkFactory);

            if(lhresult != 0)
            {
                break;
            }

            QList<IUnknown*> lCompressedMediaTypeList;

            lCompressedMediaTypeList.push_back(aUpStreamMediaType);

            QList<IUnknown*> lOutputNodeList;

            if(CaptureManagerQtProxy::getInstance().getOutputNodes(
                        mfilePath,
                        lCompressedMediaTypeList,
                        lOutputNodeList,
                        lIFileSinkFactory))
            {
                if(lOutputNodeList.length() == 0)
                    break;

                GUID lIID_IUnknown = QUuid("{00000000-0000-0000-C000-000000000046}");

                lhresult = lOutputNodeList.at(0)->QueryInterface(
                            lIID_IUnknown,
                            (void**)&lptrResult);

                if(lhresult != 0)
                {
                    break;
                }

            }
        }

    }while(false);

    return lptrResult;
}
