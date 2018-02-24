#pragma once

#include <string>
#include <vector>

#include "IRecorder.h"

struct ICaptureManagerControl;

struct ISession;

struct IUnknown;

class Recorder :
	public IRecorder
{
public:
	Recorder();
	virtual ~Recorder();

	virtual std::vector<std::wstring> getMediaInfo();

	virtual void setMediaInfoIndex(unsigned int aMediaIndex);

	virtual void startPreview(void* aPtrWindow, bool aEnableInnerRenderer);

	virtual void startPreviewAndRecording(
		void* aPtrWindow,
		bool aEnableInnerRenderer, 
		std::wstring aFilePath);

	virtual void renderToTarget(void* aPtrRenderTarget);

	void init(std::wstring a_SymbolicLink,
		ICaptureManagerControl* aPtrICaptureManagerControl);

	void init(IUnknown* a_UnkSource,
		ICaptureManagerControl* aPtrICaptureManagerControl,
		std::wstring aVideoEncoderIID,
		std::wstring aAudioEncoderIID,
		std::wstring aFileFormatIID);

	

	virtual void closeRecorder();

private:

	unsigned int mMediaIndex;

	std::vector<std::wstring> mMediaInfo;

	std::wstring m_SymbolicLink;

	ICaptureManagerControl* mPtrICaptureManagerControl;

	ISession* mPtrISession;

};

