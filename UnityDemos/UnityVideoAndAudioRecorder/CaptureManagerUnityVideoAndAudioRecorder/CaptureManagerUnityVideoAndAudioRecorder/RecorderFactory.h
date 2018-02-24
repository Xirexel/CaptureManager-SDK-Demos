#pragma once

#include <string>
#include <vector>
#include <Unknwn.h>

struct SourceData
{
	std::wstring m_SymbolicLink;

	std::wstring m_FriendlyName;
};

class IRecorder;

class RecorderFactory
{
public:
	RecorderFactory();
	~RecorderFactory();

	std::vector<SourceData> getVideoSourceData();

	BSTR getSourceXML();

	BSTR getEncoderXML();

	BSTR getFileFormatXML();

	BSTR getEncoderMediaTypes(
		BSTR aSymbolicLink,
		int aStreamIndex,
		int aMediaTypeIndex,
		BSTR aVideoEncoderIID);

	IRecorder* createRecorder(std::wstring a_VideoSymbolicLink,
		std::wstring a_AudioSymbolicLink,
		std::wstring aVideoEncoderIID,
		std::wstring aAudioEncoderIID,
		std::wstring aFileFormatIID);

	IRecorder* createRecorder(IUnknown* a_UnkSource,
		std::wstring aVideoEncoderIID,
		std::wstring aAudioEncoderIID,
		std::wstring aFileFormatIID);

	long getStrideForBitmapInfoHeader(REFGUID a_VideoFormat, int lwidth);

private:
	
};

