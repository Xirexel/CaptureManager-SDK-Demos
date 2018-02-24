#ifndef FILESINKWIDGET_H
#define FILESINKWIDGET_H

#include <QWidget>
#include <QString>


#include "AbstractSink.h"

namespace Ui {
class FileSinkWidget;
}

class FileSinkWidget : public QWidget, public AbstractSink
{
    Q_OBJECT

public:
    explicit FileSinkWidget(QString aGUIDValue, QWidget *parent = 0);
    virtual ~FileSinkWidget();


    virtual void setOptions();

    virtual IUnknown* getOutputNode(IUnknown* aUpStreamMediaType);

private slots:

    void selectFile();

private:

    QString mGUIDValue;

    QString mfilePath;

    Ui::FileSinkWidget *ui;
};

#endif // FILESINKWIDGET_H
