#include "openglvideoviewer.h"
#include <QOpenGLShader>
#include <QOpenGLShaderProgram>
#include <QTimer>


float halfQuadWidth = 0.75;
float halfQuadHeight = 0.75;

OpenGLVideoViewer::OpenGLVideoViewer(QWidget *parent):
    QOpenGLWidget(parent)
{
}

void OpenGLVideoViewer::initializeGL()
{

#define PROGRAM_VERTEX_ATTRIBUTE 0
#define PROGRAM_TEXCOORD_ATTRIBUTE 1

    initializeOpenGLFunctions();

    QOpenGLShader *vshader = new QOpenGLShader(QOpenGLShader::Vertex, this);
    const char *vsrc =
        "attribute highp vec4 vertex;\n"
        "attribute mediump vec4 texCoord;\n"
        "varying mediump vec4 texc;\n"
        "uniform mediump mat4 matrix;\n"
        "void main(void)\n"
        "{\n"
        "    gl_Position = matrix * vertex;\n"
        "    texc = texCoord;\n"
        "}\n";
    vshader->compileSourceCode(vsrc);

    QOpenGLShader *fshader = new QOpenGLShader(QOpenGLShader::Fragment, this);
    const char *fsrc =
        "uniform sampler2D texture;\n"
        "varying mediump vec4 texc;\n"
        "void main(void)\n"
        "{\n"
        "    gl_FragColor = texture2D(texture, texc.st);\n"
        "}\n";
    fshader->compileSourceCode(fsrc);

    program = new QOpenGLShaderProgram;
    program->addShader(vshader);
    program->addShader(fshader);
    program->bindAttributeLocation("vertex", PROGRAM_VERTEX_ATTRIBUTE);
    program->bindAttributeLocation("texCoord", PROGRAM_TEXCOORD_ATTRIBUTE);
    program->link();

    program->bind();
    program->setUniformValue("texture", 0);


    QVector<GLfloat> vertData;


    // Buttom Left
    vertData.append(-halfQuadWidth);

    vertData.append(-halfQuadHeight);

    vertData.append(0);

    vertData.append(1);


    // Buttom right
    vertData.append(halfQuadWidth);

    vertData.append(-halfQuadHeight);

    vertData.append(1);

    vertData.append(1);


    // Top right
    vertData.append(halfQuadWidth);

    vertData.append(halfQuadHeight);

    vertData.append(1);

    vertData.append(0);


    // Top Left
    vertData.append(-halfQuadWidth);

    vertData.append(halfQuadHeight);

    vertData.append(0);

    vertData.append(0);


    vbo.create();
    vbo.bind();
    vbo.allocate(vertData.constData(), vertData.count() * sizeof(GLfloat));

    glEnable(GL_TEXTURE_2D);

    glGenTextures(1, &mDrawTextureID);

    glBindTexture(GL_TEXTURE_2D, mDrawTextureID);



    // create the image to OpenGL
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, mWidth, mHeight, 0, GL_BGR, GL_UNSIGNED_BYTE, nullptr);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);


    glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
}

void OpenGLVideoViewer::paintGL()
{
    glClear(GL_COLOR_BUFFER_BIT);


    reloadVideoFrame();

    QMatrix4x4 matrix;
    matrix.setToIdentity();

    program->setUniformValue("matrix", matrix);
    program->enableAttributeArray(PROGRAM_VERTEX_ATTRIBUTE);
    program->enableAttributeArray(PROGRAM_TEXCOORD_ATTRIBUTE);
    program->setAttributeBuffer(PROGRAM_VERTEX_ATTRIBUTE, GL_FLOAT, 0, 2, 4 * sizeof(GLfloat));
    program->setAttributeBuffer(PROGRAM_TEXCOORD_ATTRIBUTE, GL_FLOAT, 2 * sizeof(GLfloat), 2, 4 * sizeof(GLfloat));


    glDrawArrays(GL_QUADS, 0, 4);
}

void OpenGLVideoViewer::resizeGL(int w, int h)
{
    glViewport(0, 0, (GLint)w, (GLint)h);
}

