#include "RecorderFactory.h"
#include <tchar.h>
#include "../Common/ComPtrCustom.h"
#include "../Common/pugixml.hpp"
#include "../Common/CaptureManagerTypeInfo.h"
#include "../Common/CaptureManagerLoader.h"
#include "Recorder.h"

#define IID_PPV_ARGSIUnknown(ppType) __uuidof(**(ppType)), (IUnknown**)(ppType)



using namespace pugi;



void collectSources(xml_document& lSourcesXmlDoc, std::vector<SourceData>& a_refVideoSources);

RecorderFactory::RecorderFactory()
{
}

RecorderFactory::~RecorderFactory()
{
}

std::vector<SourceData> RecorderFactory::getVideoSourceData()
{
	std::vector<SourceData> l_VideoSources;

	do
	{
		HRESULT lhresult(E_FAIL);

		//CComPtrCustom<IClassFactory> lCoLogPrintOut;

		//lhresult = CaptureManagerLoader::getInstance().createCalssFactory(__uuidof(CoLogPrintOut), &lCoLogPrintOut);

		//if (FAILED(lhresult))
		//	break;

		//CComPtrCustom<ILogPrintOutControl> lLogPrintOutControl;

		//lCoLogPrintOut->LockServer(true);

		//lhresult = lCoLogPrintOut->CreateInstance(
		//	nullptr,
		//	IID_PPV_ARGS(&lLogPrintOutControl));

		//if (FAILED(lhresult))
		//	break;

		//// set log file for info
		//lhresult = lLogPrintOutControl->addPrintOutDestination(
		//	(DWORD)INFO_LEVEL,
		//	L"Log.txt");

		//// set log file for info
		//lhresult = lLogPrintOutControl->addPrintOutDestination(
		//	(DWORD)ERROR_LEVEL,
		//	L"Log.txt");

		//if (FAILED(lhresult))
		//	break;

		/* initialisation CaptureManager */

		CComPtrCustom<IClassFactory> lCoCaptureManager;

		lhresult = CaptureManagerLoader::getInstance().createCalssFactory(__uuidof(CoCaptureManager), &lCoCaptureManager);

		if (FAILED(lhresult))
			break;
		
		CComPtrCustom<ICaptureManagerControl> l_CaptureManagerControl;

		// get ICaptureManagerControl interfrace
		lhresult = lCoCaptureManager->CreateInstance(
			nullptr,
			IID_PPV_ARGS(&l_CaptureManagerControl));

		if (FAILED(lhresult))
			break;


		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = l_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			break;

		BSTR lXMLString = nullptr;

		lhresult = lSourceControl->getCollectionOfSources(&lXMLString);

		if (FAILED(lhresult))
			break;


		xml_document lSourcesXmlDoc;

		lSourcesXmlDoc.load_string(lXMLString);


		// Get source collection.
		collectSources(lSourcesXmlDoc, l_VideoSources);

		SysFreeString(lXMLString);
		

	} while (false);

	return l_VideoSources;
}

BSTR RecorderFactory::getSourceXML()
{
	BSTR lXMLString = nullptr;

	do
	{
		HRESULT lhresult(E_FAIL);

		/* initialisation CaptureManager */

		CComPtrCustom<IClassFactory> lCoCaptureManager;

		lhresult = CaptureManagerLoader::getInstance().createCalssFactory(__uuidof(CoCaptureManager), &lCoCaptureManager);

		if (FAILED(lhresult))
			break;

		CComPtrCustom<ICaptureManagerControl> l_CaptureManagerControl;

		// get ICaptureManagerControl interfrace
		lhresult = lCoCaptureManager->CreateInstance(
			nullptr,
			IID_PPV_ARGS(&l_CaptureManagerControl));

		if (FAILED(lhresult))
			break;


		// get ISourceControl inetrface
		CComPtrCustom<ISourceControl> lSourceControl;

		lhresult = l_CaptureManagerControl->createControl(
			IID_PPV_ARGSIUnknown(&lSourceControl));

		if (FAILED(lhresult))
			break;


		lhresult = lSourceControl->getCollectionOfSources(&lXMLString);

		if (FAILED(lhresult))
			break;
		
	} while (false);

	return lXMLString;
}

IRecorder* RecorderFactory::createRecorder(std::wstring a_SymbolicLink)
{
	IRecorder* lIRecorder = nullptr;

	do
	{
		HRESULT lhresult(E_FAIL);

		CComPtrCustom<IClassFactory> lCoCaptureManager;

		lhresult = CaptureManagerLoader::getInstance().createCalssFactory(__uuidof(CoCaptureManager), &lCoCaptureManager);

		if (FAILED(lhresult))
			break;

		CComPtrCustom<ICaptureManagerControl> l_CaptureManagerControl;

		// get ICaptureManagerControl interfrace
		lhresult = lCoCaptureManager->CreateInstance(
			nullptr,
			IID_PPV_ARGS(&l_CaptureManagerControl));

		if (FAILED(lhresult))
			break;

		Recorder* lRecorder = new Recorder();
		
		lRecorder->init(a_SymbolicLink, l_CaptureManagerControl);

		lIRecorder = lRecorder;

	} while (false);

	return lIRecorder;
}

void collectSources(xml_document& lSourcesXmlDoc, std::vector<SourceData>& a_refVideoSources)
{
	// find symbolic link for Video Source
	auto lFindVideoSymbolicLink = [&](const xml_node &node)
	{
		bool lresult = false;


		if (lstrcmpW(node.name(), L"Source.Attributes") == 0)
		{

			// name 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' has only video source
			xml_node lVideoSymbolicLinkAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK");

			if (!lVideoSymbolicLinkAttrNode.empty())
			{
				xml_node lFriendlyNameAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME");

				if (!lFriendlyNameAttrNode.empty())
				{
					auto lFirstValueAttrNode = lVideoSymbolicLinkAttrNode.first_child().attribute(L"Value");

					auto lSecondValueAttrNode = lFriendlyNameAttrNode.first_child().attribute(L"Value");


					if (!lFirstValueAttrNode.empty() && !lSecondValueAttrNode.empty())
					{
						SourceData lSourceData;

						lSourceData.m_SymbolicLink = lFirstValueAttrNode.as_string();

						lSourceData.m_FriendlyName = lSecondValueAttrNode.as_string();

						a_refVideoSources.push_back(lSourceData);
					}

					lresult = true;
				}
			}
			//else
			//{
			//	// name 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK' has only audio source
			//	lVideoSymbolicLinkAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK");

			//	if (!lVideoSymbolicLinkAttrNode.empty())
			//	{
			//		xml_node lFriendlyNameAttrNode = node.find_child_by_attribute(L"Name", L"MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME");

			//		if (!lFriendlyNameAttrNode.empty())
			//		{
			//			auto lFirstValueAttrNode = lVideoSymbolicLinkAttrNode.first_child().attribute(L"Value");

			//			auto lSecondValueAttrNode = lFriendlyNameAttrNode.first_child().attribute(L"Value");


			//			if (!lFirstValueAttrNode.empty() && !lSecondValueAttrNode.empty())
			//			{
			//				SourceData lSourceData;

			//				lSourceData.m_SymbolicLink = lFirstValueAttrNode.as_string();

			//				lSourceData.m_FriendlyName = lSecondValueAttrNode.as_string();

			//				g_AudioSources.push_back(lSourceData);
			//			}

			//			lresult = true;
			//		}
			//	}
			//}
		}

		return lresult;
	};

	// find first Video Source
	auto lFindFirstVideoSource = [lFindVideoSymbolicLink](const xml_node &node)
	{
		bool lresult = false;

		if ((lstrcmpW(node.name(), L"Source") == 0))
		{
			xml_node lAttrNode = node.find_node(lFindVideoSymbolicLink);
		}

		return lresult;
	};

	xml_node lVideoSourceXMLNode = lSourcesXmlDoc.find_node(lFindFirstVideoSource);
}