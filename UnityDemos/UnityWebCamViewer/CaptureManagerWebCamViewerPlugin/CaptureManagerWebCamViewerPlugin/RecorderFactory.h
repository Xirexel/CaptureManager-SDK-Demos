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

	IRecorder* createRecorder(std::wstring a_SymbolicLink);

private:
	
};

