#ifndef RECORDINGWIDGET_H
#define RECORDINGWIDGET_H

#include <memory>
#include <QWidget>

#include "AbstractSink.h"

namespace Ui {
class RecordingWidget;
}

class RecordingWidget : public QWidget
{
    Q_OBJECT

public:
    explicit RecordingWidget(QWidget *parent = 0);
    ~RecordingWidget();

private slots:

    void recieveSourceCurrentIndex(int index);

    void recieveStreamCurrentIndex(int index);

    void recieveMediaTypeCurrentIndex(int index);

    void recieveEncoderCurrentIndex(int index);

    void recieveEncoderModeCurrentIndex(int index);

    void recieveCompressedMediaTypeCurrentIndex(int index);

    void recieveSinkFactoryCurrentIndex(int index);

    void recieveContainerCurrentIndex(int index);

    void startRecording();



private:

    std::unique_ptr<AbstractSink> mSink;

    Ui::RecordingWidget *ui;

    void readSources();

    void fillSourcesCombo(QString &aRefQStringXMLDocSources);
};

#endif // RECORDINGWIDGET_H
