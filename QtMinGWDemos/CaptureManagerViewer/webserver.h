#ifndef WEBSERVER_H
#define WEBSERVER_H

#include <QObject>
#include <QString>
#include <mutex>
#include <memory>
#include <QHostAddress>
#include <QList>


/// <summary>
/// HTTPS StatusCodes
/// </summary>
enum class StatusCode
{
    /// <summary>
    /// 200 OK
    /// </summary>
    OK,
    /// <summary>
    /// 400 Bad Request
    /// </summary>
    BadRequest,
    /// <summary>
    /// 403 Access Forbidden
    /// </summary>
    Forbiden
};

struct WebServerConfiguration
{
    QString _MIME = "";
    int _port = 8080;
    QString _serverName = "HttpOutputByteStreamWebServer";
    QHostAddress _IPAddress;// QHostAddress::LocalHost
};

class QTcpServer;

class QTcpSocket;

class WebServer : public QObject
{
    Q_OBJECT
public:
    explicit WebServer(
            WebServerConfiguration webServerConf,
            QObject *parent = 0);

    bool Start();

    void Stop();

    void writeHeaderMemory(const unsigned char* aPtrData, uint aLength);

    void sendRawData(const unsigned char* pb, qint64 cb);

signals:

public slots:

private slots:

    void makeConnection();

    void readReady();

private:

    WebServerConfiguration Configuration;

    QTcpServer* tcpServer;

    std::unique_ptr<unsigned char> mHeaderMemory;

    qint64 mHeaderMemoryLength;

    std::mutex mLockHeaderMemoryMutex;

    QList<QTcpSocket *> mClients;




    bool SuportedMethod(QString request);

    void SendHeader(StatusCode statusCode, QTcpSocket *aTcpClient);

    QString GetStatusCode(StatusCode statusCode);

    void SendToClient(QString data, QTcpSocket *aTcpClient);
};

#endif // WEBSERVER_H
