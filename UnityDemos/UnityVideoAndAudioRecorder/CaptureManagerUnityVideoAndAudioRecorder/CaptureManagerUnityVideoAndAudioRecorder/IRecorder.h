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

	virtual void setSourceMediaInfoIndex(
		unsigned int aVideoStreamIndex,
		unsigned int aVideoMediaTypeIndex,
		unsigned int aAudioStreamIndex,
		unsigned int aAudioMediaTypeIndex) = 0;

	virtual void setCompressionMediaInfoIndex(
		std::wstring aVideoEncodeModeIID,
		unsigned int aVideoMediaTypeIndex,
		std::wstring aAudioEncodeModeIID,
		unsigned int aAudioMediaTypeIndex) = 0;

	virtual void setMediaInfoIndex(unsigned int aMediaIndex) = 0;

	virtual void startPreview(void* aPtrWindow, bool aEnableInnerRenderer) = 0;

	virtual void startPreviewAndRecording(
		void* aPtrWindow,
		bool aEnableInnerRenderer,
		std::wstring aFilePath) = 0;

	virtual void renderToTarget(void* aPtrRenderTarget) = 0;

	virtual void closeRecorder() = 0;
};

