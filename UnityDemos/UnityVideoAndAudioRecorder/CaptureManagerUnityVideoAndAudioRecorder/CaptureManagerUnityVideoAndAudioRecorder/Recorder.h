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

	virtual void setSourceMediaInfoIndex(
		unsigned int aVideoStreamIndex,
		unsigned int aVideoMediaTypeIndex,
		unsigned int aAudioStreamIndex,
		unsigned int aAudioMediaTypeIndex);
		
	virtual void setCompressionMediaInfoIndex(
		std::wstring aVideoEncodeModeIID,
		unsigned int aVideoMediaTypeIndex,
		std::wstring aAudioEncodeModeIID,
		unsigned int aAudioMediaTypeIndex);

	virtual void setMediaInfoIndex(unsigned int aMediaIndex);

	virtual void startPreview(void* aPtrWindow, bool aEnableInnerRenderer);

	virtual void startPreviewAndRecording(
		void* aPtrWindow,
		bool aEnableInnerRenderer, 
		std::wstring aFilePath);

	virtual void renderToTarget(void* aPtrRenderTarget);

	void init(std::wstring a_VideoSymbolicLink,
		std::wstring a_AudioSymbolicLink,
		ICaptureManagerControl* aPtrICaptureManagerControl,
		std::wstring aVideoEncoderIID,
		std::wstring aAudioEncoderIID,
		std::wstring aFileFormatIID);

	void init(IUnknown* a_UnkSource,
		ICaptureManagerControl* aPtrICaptureManagerControl,
		std::wstring aVideoEncoderIID,
		std::wstring aAudioEncoderIID,
		std::wstring aFileFormatIID);

	

	virtual void closeRecorder();

private:

	unsigned int mMediaIndex;

	unsigned int mVideoStreamIndex;
	unsigned int mVideoMediaTypeIndex;
	unsigned int mAudioStreamIndex;
	unsigned int mAudioMediaTypeIndex;


	std::wstring mVideoCompressionEncodeModeIID;
	unsigned int mVideoCompressionMediaTypeIndex;
	std::wstring mAudioCompressionEncodeModeIID;
	unsigned int mAudioCompressionMediaTypeIndex;


	std::vector<std::wstring> mMediaInfo;

	std::wstring m_VideoSymbolicLink;

	std::wstring m_AudioSymbolicLink;

	ICaptureManagerControl* mPtrICaptureManagerControl;

	ISession* mPtrISession;

};

