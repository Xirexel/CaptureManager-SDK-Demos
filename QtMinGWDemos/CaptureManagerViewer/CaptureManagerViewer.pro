#-------------------------------------------------
#
# Project created by QtCreator 2016-03-14T19:32:47
#
#-------------------------------------------------

QT       += core gui xml xmlpatterns opengl network

greaterThan(QT_MAJOR_VERSION, 4): QT += widgets

QT       += opengl

TARGET = CaptureManagerViewer
TEMPLATE = app
CONFIG += c++11

win32:DESTDIR = ../../Bin
else:unix{
DESTDIR = ../../Bin

}

SOURCES += main.cpp\
        widget.cpp \
    videosamplegrabbercallviewer.cpp \
    webcameraviewer.cpp \
    CaptureManagerTypeInfo_i.c \
    openglvideoviewer.cpp \
    videosamplegrabbercallbackviewer.cpp \
    evrviewer.cpp \
    webcamcontrolwidget.cpp \
    recordingwidget.cpp \
    filesinkwidget.cpp \
    networkstreamsink.cpp \
    webserver.cpp

HEADERS  += widget.h \
    ComPtrCustom.h \
    videosamplegrabbercallviewer.h \
    webcameraviewer.h \
    openglvideoviewer.h \
    videosamplegrabbercallbackviewer.h \
    evrviewer.h \
    webcamcontrolwidget.h \
    recordingwidget.h \
    abstractsink.h \
    filesinkwidget.h \
    networkstreamsink.h \
    webserver.h

FORMS    += widget.ui \
    recordingwidget.ui \
    filesinkwidget.ui

win32:CONFIG(release, debug|release): LIBS += -L$$OUT_PWD/../CaptureManagerQtProxy/release/ -lCaptureManagerQtProxy
else:win32:CONFIG(debug, debug|release): LIBS += -L$$OUT_PWD/../CaptureManagerQtProxy/debug/ -lCaptureManagerQtProxy
else:macx: LIBS += -L$$OUT_PWD/../CaptureManagerQtProxy/ -lCaptureManagerQtProxy

INCLUDEPATH += $$PWD/../CaptureManagerQtProxy
DEPENDPATH += $$PWD/../CaptureManagerQtProxy
