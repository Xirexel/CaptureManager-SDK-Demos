#pragma once

#include <string>
#include <vector>

class IRecorder
{
public:

	IRecorder()
	{
	}

	virtual ~IRecorder()
	{
	}

	virtual std::vector<std::wstring> getMediaInfo() = 0;

	virtual void setMediaInfoIndex(unsigned int aMediaIndex) = 0;

	virtual void startPreview(void* aPtrWindow, bool aEnableInnerRenderer) = 0;

	virtual void startPreviewAndRecording(void* aPtrWindow, std::wstring aFilePath) = 0;

	virtual void renderToTarget(void* aPtrRenderTarget) = 0;

	virtual void closeRecorder() = 0;
};

