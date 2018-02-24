#ifndef ABSTRACTSINK
#define ABSTRACTSINK

struct IUnknown;

class AbstractSink
{
public:
    virtual void setOptions() = 0;

    virtual IUnknown* getOutputNode(IUnknown* aUpStreamMediaType) = 0;

    virtual ~AbstractSink(){}
};

#endif // ABSTRACTSINK

