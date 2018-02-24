#ifndef EVRVIEWER_H
#define EVRVIEWER_H

#include <QWindow>
#include <QUuid>
#include <unknwnbase.h>

class EVRViewer : public QWindow
{
    Q_OBJECT
public:
    explicit EVRViewer(
            QUuid aContainerTypeQUuid,
            uint aWidth,
            uint aHeight,
            IUnknown **aPtrPtrOutputNode,QWidget *parent = 0);

signals:

public slots:
};

#endif // EVRVIEWER_H
