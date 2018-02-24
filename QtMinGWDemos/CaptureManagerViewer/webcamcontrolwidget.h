#ifndef WEBCAMCONTROLWIDGET_H
#define WEBCAMCONTROLWIDGET_H

#include <QWidget>
#include <QString>
#include <QDomNode>

class IWebCamControl;

class WebCamControlWidget : public QWidget
{
    Q_OBJECT
public:

    static QWidget* createWebCamControlWidget(IWebCamControl* aPtrIWebCamControl);

    virtual ~WebCamControlWidget();

signals:

public slots:

private slots:

    void setFlag(bool aFlagState);

    void sliderValueChanged(int value);

protected:

    virtual void showEvent(QShowEvent * event);

private:

    IWebCamControl *mPtrIWebCamControl;


    explicit WebCamControlWidget(IWebCamControl *aPtrIWebCamControl, QWidget *parent = 0);

    void fillOptions();

    void fillParamentrs(QString aQstringXMLDocCamParametrs);

    QLayout* makeParametrControl(QDomNode aParametrNode);

};

#endif // WEBCAMCONTROLWIDGET_H
