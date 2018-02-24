#-------------------------------------------------
#
# Project created by QtCreator 2016-03-14T17:37:35
#
#-------------------------------------------------

QT       -= gui
QT       += core axcontainer

TARGET = CaptureManagerQtProxy
TEMPLATE = lib
CONFIG += c++11

#win32:DESTDIR = ../../Bin
#else:unix{
#DESTDIR = ../../Bin
#}

DEFINES += CAPTUREMANAGERQTPROXY_LIBRARY

SOURCES += capturemanagerqtproxy.cpp \
    CaptureManagerTypeInfo_i.c

HEADERS += capturemanagerqtproxy.h\
        capturemanagerqtproxy_global.h \
    CaptureManagerTypeInfo.h

unix {
    target.path = /usr/lib
    INSTALLS += target
}
