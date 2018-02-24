#include "webserver.h"
#include <QIODevice>
#include <QTcpServer>
#include <QTcpSocket>
#include <QHostAddress>
#include <QMessageBox>

WebServer::WebServer(WebServerConfiguration webServerConf, QObject *parent) : QObject(parent), mHeaderMemoryLength(0)
{
    this->Configuration = webServerConf;

    tcpServer = new QTcpServer(this);

    connect(tcpServer, SIGNAL(newConnection()), this, SLOT(makeConnection()));
}


bool WebServer::Start()
{
    bool lresult = false;

    do
    {
        if (!tcpServer->listen(
                    Configuration._IPAddress,
                    (quint16)Configuration._port))
        {
            QString lTitle("WebServer Server");

            QString lmessage = QString("Unable to start the server: %1.").arg(tcpServer->errorString());

            QMessageBox::critical(nullptr, lTitle,lmessage);

            break;
        }


    }while(false);

    return lresult;
}

void WebServer::Stop()
{
    try
    {
        tcpServer->deleteLater();
    }
    catch (...)
    {
    }
}


void WebServer::makeConnection()
{
    do
    {
        QTcpSocket *clientConnection = tcpServer->nextPendingConnection();

        if(clientConnection == nullptr)
            break;

        connect(clientConnection, SIGNAL(readyRead()),
                this, SLOT(readReady()));


        clientConnection->open(QIODevice::OpenModeFlag::ReadWrite);

    }while(false);
}

void WebServer::readReady()
{
    QTcpSocket* lPtrQTcpSocket = qobject_cast<QTcpSocket*>(sender());

    QTextStream lTextStream(lPtrQTcpSocket);

    QString receivedData = lTextStream.readAll();

    if (!SuportedMethod(receivedData))
    {

    }
    else
    {
        SendHeader(StatusCode::OK, lPtrQTcpSocket);

        std::lock_guard<std::mutex> lLock(mLockHeaderMemoryMutex);

        mClients.push_back(lPtrQTcpSocket);

        if(mHeaderMemoryLength > 0)
            lPtrQTcpSocket->write((char*)mHeaderMemory.get(), mHeaderMemoryLength);
    }
}

bool WebServer::SuportedMethod(QString request)
{
    if (request.isNull() || request.length() < 3)
        return false;

    return request.startsWith("GET");
}

void WebServer::SendHeader(StatusCode statusCode, QTcpSocket *aTcpClient)
{
    QString header;
    header.append(QString("HTTP/1.1 %1\r\n").arg(GetStatusCode(statusCode)));
    header.append(QString("Content-Type: %1\r\n").arg(Configuration._MIME));
    header.append(QString("Server: %1\r\n").arg(Configuration._serverName));
    header.append(QString("Accept-Ranges: none\r\n"));
    header.append(QString("TransferMode.DLNA.ORG: Streaming\r\n"));
    header.append(QString("Connection: keep-alive\r\n"));
    header.append("\r\n");

    SendToClient(header, aTcpClient);
}

QString WebServer::GetStatusCode(StatusCode statusCode)
{
    QString code;

    switch (statusCode)
    {
        case StatusCode::OK: code = "200 OK"; break;
        case StatusCode::BadRequest: code = "400 Bad Request"; break;
        case StatusCode::Forbiden: code = "403 Forbidden"; break;
        default: code = "202 Accepted"; break;
    }

    return code;
}

void WebServer::SendToClient(QString data, QTcpSocket *aTcpClient)
{
    qDebug() << data;

    aTcpClient->write(data.toUtf8());
}

void WebServer::writeHeaderMemory(const unsigned char* aPtrData, uint aLength)
{
    mHeaderMemory.reset(new unsigned char[aLength]);

    std::lock_guard<std::mutex> lLock(mLockHeaderMemoryMutex);

    memcpy(mHeaderMemory.get(), aPtrData, aLength);

    mHeaderMemoryLength = aLength;

    sendRawData(mHeaderMemory.get(), mHeaderMemoryLength);
}

void WebServer::sendRawData(const unsigned char* pb, qint64 cb)
{

    do
    {
        QList<QTcpSocket*> lremoveList;

        for(int lClientIndex = 0;
            lClientIndex < mClients.length();
            lClientIndex++)
        {

            QTcpSocket *lPtrClient = mClients.at(lClientIndex);

            if(lPtrClient->isOpen())
            {
                lPtrClient->write((const char*)pb, cb);

                lPtrClient->flush();
            }
            else
            {
                lremoveList.push_back(lPtrClient);
            }

        }

        for(int lClientIndex = 0;
            lClientIndex < lremoveList.length();
            lClientIndex++)
        {

            QTcpSocket *lPtrClient = lremoveList.at(lClientIndex);

            mClients.removeOne(lPtrClient);

            lPtrClient->deleteLater();
        }


    } while (false);
}
