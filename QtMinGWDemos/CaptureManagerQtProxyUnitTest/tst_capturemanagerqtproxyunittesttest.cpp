#include <QString>
#include <QtTest>
#include <QCoreApplication>


#include "ComPtrCustom.h"
#include "capturemanagerqtproxy.h"
#include "CaptureManagerTypeInfo.h"



class CaptureManagerQtProxyUnitTestTest : public QObject
{
    Q_OBJECT

public:
    CaptureManagerQtProxyUnitTestTest();

private Q_SLOTS:
    void initTestCase();
    void cleanupTestCase();
    void test_init();
    void test_getILogPrintOutControl();
    void test_getISourceControl();
    void test_getISinkControl();



};

CaptureManagerQtProxyUnitTestTest::CaptureManagerQtProxyUnitTestTest()
{
}

void CaptureManagerQtProxyUnitTestTest::initTestCase()
{
    CaptureManagerQtProxy::getInstance();
}

void CaptureManagerQtProxyUnitTestTest::cleanupTestCase()
{
    CaptureManagerQtProxy::getInstance().release();
}

void CaptureManagerQtProxyUnitTestTest::test_init()
{
    auto lresult = CaptureManagerQtProxy::getInstance().isInitialized();

    QVERIFY2(lresult, "Failure");
}

void CaptureManagerQtProxyUnitTestTest::test_getILogPrintOutControl()
{
    CComPtrCustom<ILogPrintOutControl> lILogPrintOutControl;

    auto lresult = CaptureManagerQtProxy::getInstance().getILogPrintOutControl(&lILogPrintOutControl);

    QVERIFY2(lresult, "Failure");
}

void CaptureManagerQtProxyUnitTestTest::test_getISourceControl()
{
    CComPtrCustom<ISourceControl> lISourceControl;

    auto lresult = CaptureManagerQtProxy::getInstance().getISourceControl(&lISourceControl);

    QVERIFY2(lresult, "Failure");
}

void CaptureManagerQtProxyUnitTestTest::test_getISinkControl()
{
    CComPtrCustom<ISinkControl> lISinkControl;

    auto lresult = CaptureManagerQtProxy::getInstance().getISinkControl(&lISinkControl);

    QVERIFY2(lresult, "Failure");
}

QTEST_MAIN(CaptureManagerQtProxyUnitTestTest)

#include "tst_capturemanagerqtproxyunittesttest.moc"
