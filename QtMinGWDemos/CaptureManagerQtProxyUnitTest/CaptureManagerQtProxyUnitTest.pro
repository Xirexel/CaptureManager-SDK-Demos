#-------------------------------------------------
#
# Project created by QtCreator 2016-03-14T17:39:38
#
#-------------------------------------------------

QT       += testlib

QT       -= gui

TARGET = tst_capturemanagerqtproxyunittesttest
CONFIG   += console
CONFIG   -= app_bundle
CONFIG += c++11

TEMPLATE = app


SOURCES += tst_capturemanagerqtproxyunittesttest.cpp
DEFINES += SRCDIR=\\\"$$PWD/\\\"

win32:CONFIG(release, debug|release): LIBS += -L$$OUT_PWD/../CaptureManagerQtProxy/release/ -lCaptureManagerQtProxy
else:win32:CONFIG(debug, debug|release): LIBS += -L$$OUT_PWD/../CaptureManagerQtProxy/debug/ -lCaptureManagerQtProxy

INCLUDEPATH += $$PWD/../CaptureManagerQtProxy
DEPENDPATH += $$PWD/../CaptureManagerQtProxy

HEADERS += \
    ComPtrCustom.h
