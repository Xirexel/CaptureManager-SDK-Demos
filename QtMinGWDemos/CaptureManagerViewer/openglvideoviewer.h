#ifndef OPENGLVIDEOVIEWER_H
#define OPENGLVIDEOVIEWER_H

#include <QUuid>
#include <QOpenGLWidget>
#include <QOpenGLFunctions>
#include <QOpenGLBuffer>



static QUuid MFMediaType_Video(
0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

static QUuid MFVideoFormat_RGB24(
20, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

static QUuid MFVideoFormat_RGB32(
22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

class QOpenGLShaderProgram;

class OpenGLVideoViewer: public QOpenGLWidget, public QOpenGLFunctions
{
    Q_OBJECT
public:
    explicit OpenGLVideoViewer(QWidget *parent = 0);

protected:
    virtual void initializeGL();
    virtual void paintGL();
    virtual void resizeGL(int w, int h);

    virtual void reloadVideoFrame() = 0;

    GLuint mDrawTextureID;

    uint mWidth;

    uint mHeight;

signals:

public slots:

private:

    QOpenGLShaderProgram *program;
    QOpenGLBuffer vbo;
};

#endif // OPENGLVIDEOVIEWER_H
