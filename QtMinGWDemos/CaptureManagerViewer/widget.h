#ifndef WIDGET_H
#define WIDGET_H

#include <QWidget>
#include "ComPtrCustom.h"

namespace Ui {
class Widget;
}

class Widget : public QWidget
{
    Q_OBJECT

public:
    explicit Widget(QWidget *parent = 0);
    ~Widget();

private:
    Ui::Widget *ui;


    enum class WebViewerState
    {
        None,
        ReadyToPlay,
        Play,
        Paused,
        Stoped,
        Closed
    };

    WebViewerState mWebViewerState;


    void initLogOut();
    void readSources();
    void fillSourcesCombo(QString& aRefQStringXMLDocSources);
    void fillSinkCollection(QString& aRefQstringXMLDocSinks);
    void readSinks();


    void setState(WebViewerState aWebViewerState);

    bool resolveTopology();

private slots:

    void recieveSourceCurrentIndex(int index);

    void recieveMediaTypeCurrentIndex(int index);

    void recieveRenderTypeCurrentIndex(int index);

    void play_pauseSlot();

    void stopSlot();

    void closeSlot();

private:

    CComPtrCustom<IUnknown> mOutputNode;

    CComPtrCustom<IUnknown> mIUnknownISession;


    void checkAndInitOptions();
};

#endif // WIDGET_H
