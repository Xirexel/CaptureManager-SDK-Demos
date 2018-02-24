#ifndef NETWORKSTREAMSINK_H
#define NETWORKSTREAMSINK_H

#include <QObject>
#include <QString>

#include "AbstractSink.h"

class NetworkStreamSink : public QObject, public AbstractSink
{
    Q_OBJECT
public:
    explicit NetworkStreamSink(QString aGUIDValue, QString aMIME, QObject *parent = 0);


    virtual void setOptions();

    virtual IUnknown* getOutputNode(IUnknown* aUpStreamMediaType);

signals:

public slots:

private:

    QString mGUIDValue;
};

#endif // NETWORKSTREAMSINK_H
