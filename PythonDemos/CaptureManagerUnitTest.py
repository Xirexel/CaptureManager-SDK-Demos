import unittest

from CaptureManagerPythonProxy import LogPrintOut
from CaptureManagerPythonProxy import CaptureManager, SourceControl, SinkFactories
import xml.etree.ElementTree as ElementTree
import xml.etree.ElementPath as ElementPath

captureManager = CaptureManager.CaptureManager()
logPrintOut = LogPrintOut.LogPrintOut()

class Test_LogPrintOut(unittest.TestCase):
    def test_LogPrintOut(self):        
        logPrintOut.addPrintOutDestination(0, 'test.txt')
        logPrintOut.setVerbose(0, 'test.txt', False)
        logPrintOut.removePrintOutDestination(0, 'test.txt')




class Test_CaptureManager(unittest.TestCase):
    def test_createSourceControl(self):        
        sourceControl = captureManager.createSourceControl()


class Test_SourceControl(unittest.TestCase):
    def test_getCollectionOfSources(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()

    def test_getSourceOutputMediaType(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')
        mediaType = sourceControl.getSourceOutputMediaType(symboliclink, 0, 0)

    def test_createSourceNode(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')
        sourceNode = sourceControl.createSourceNode(symboliclink, 0, 0)

    def test_createSourceNodeWithDownStreamConnection(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')        
        sinkControl = captureManager.createSinkControl()
        XMLstring = sinkControl.getCollectionOfSinks()
        root = ElementTree.fromstring(XMLstring)
        sinkFactories = ElementPath.findall(root, "SinkFactory") 
        fileSinkFactoreGUID = ''
        asfContainerGUID = ''
        for sinkFactory in sinkFactories:
            if(sinkFactory.get('GUID') == '{D6E342E3-7DDD-4858-AB91-4253643864C2}'):
                fileSinkFactoreGUID = '{D6E342E3-7DDD-4858-AB91-4253643864C2}'
                for valuePart in ElementPath.findall(sinkFactory, 'Value.ValueParts/ValuePart'):
                    if(valuePart.get('GUID') == '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'):
                        asfContainerGUID = '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'
        fileSinkFactory = SinkFactories.FileSinkFactory(sinkControl.createSinkFactory(asfContainerGUID, fileSinkFactoreGUID))

        indexOfStream = 0
        indexOfMediaType = 2

        outputMediaType = sourceControl.getSourceOutputMediaType(symboliclink, indexOfStream, indexOfMediaType)
        aArrayPtrCompressedMediaTypes = []
        aArrayPtrCompressedMediaTypes.append(outputMediaType)
        outputNodes = fileSinkFactory.createOutputNodes(aArrayPtrCompressedMediaTypes, 'test.asf')

        sourceNode = sourceControl.createSourceNodeWithDownStreamConnection(symboliclink, indexOfStream, indexOfMediaType, outputNodes[0])

    def test_createSource(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')
        mediaSource = sourceControl.createSource(symboliclink)
        return mediaSource

    def test_getSourceOutputMediaType(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')
        outputMediaType = sourceControl.getSourceOutputMediaType(symboliclink, 0, 2)
        return outputMediaType

    def test_createSourceNodeFromExternalSource(self):  
        sourceControl = captureManager.createSourceControl()   
        mediaSource = self.test_createSource()
        sourceNode = sourceControl.createSourceNodeFromExternalSource(mediaSource, 0, 0)


    def test_createSourceNodeFromExternalSourceWithDownStreamConnection(self):        
        sourceControl = captureManager.createSourceControl()   
        mediaSource = self.test_createSource()
        
        sinkControl = captureManager.createSinkControl()
        XMLstring = sinkControl.getCollectionOfSinks()
        root = ElementTree.fromstring(XMLstring)
        sinkFactories = ElementPath.findall(root, "SinkFactory") 
        fileSinkFactoreGUID = ''
        asfContainerGUID = ''
        for sinkFactory in sinkFactories:
            if(sinkFactory.get('GUID') == '{D6E342E3-7DDD-4858-AB91-4253643864C2}'):
                fileSinkFactoreGUID = '{D6E342E3-7DDD-4858-AB91-4253643864C2}'
                for valuePart in ElementPath.findall(sinkFactory, 'Value.ValueParts/ValuePart'):
                    if(valuePart.get('GUID') == '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'):
                        asfContainerGUID = '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'
        fileSinkFactory = SinkFactories.FileSinkFactory(sinkControl.createSinkFactory(asfContainerGUID, fileSinkFactoreGUID))

        indexOfStream = 0
        indexOfMediaType = 2

        outputMediaType = sourceControl.getSourceOutputMediaTypeFromMediaSource(mediaSource, indexOfStream, indexOfMediaType)
        aArrayPtrCompressedMediaTypes = []
        aArrayPtrCompressedMediaTypes.append(outputMediaType)
        outputNodes = fileSinkFactory.createOutputNodes(aArrayPtrCompressedMediaTypes, 'test.asf')

        sourceNode = sourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(mediaSource, indexOfStream, indexOfMediaType, outputNodes[0])

    def test_createWebCamControl(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):   
                        if(symboliclink ==''):       
                            symboliclink = attribute.find('SingleValue').get('Value')
        webCamControl = sourceControl.createWebCamControl(symboliclink)

class Test_WebCamControl(unittest.TestCase):    
    def test_createWebCamControl(self):        
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):   
                        if(symboliclink ==''):       
                            symboliclink = attribute.find('SingleValue').get('Value')

        return sourceControl.createWebCamControl(symboliclink)
    
    def test_getCamParametrs(self):  
        self.test_createWebCamControl().getCamParametrs()
    
    def test_getCamParametr(self):  
        self.test_createWebCamControl().getCamParametr(0)
    
    def test_setCamParametr(self):  
        self.test_createWebCamControl().setCamParametr(0, 10, 1)

class Test_SinkControl(unittest.TestCase):
    def test_getCollectionOfSinks(self):        
        sinkControl = captureManager.createSinkControl()
        XMLstring = sinkControl.getCollectionOfSinks()
    def test_createFileSinkFactory(self):        
        sinkControl = captureManager.createSinkControl()
        XMLstring = sinkControl.getCollectionOfSinks()
        root = ElementTree.fromstring(XMLstring)
        sinkFactories = ElementPath.findall(root, "SinkFactory") 
        fileSinkFactoreGUID = ''
        asfContainerGUID = ''
        for sinkFactory in sinkFactories:
            if(sinkFactory.get('GUID') == '{D6E342E3-7DDD-4858-AB91-4253643864C2}'):
                fileSinkFactoreGUID = '{D6E342E3-7DDD-4858-AB91-4253643864C2}'
                for valuePart in ElementPath.findall(sinkFactory, 'Value.ValueParts/ValuePart'):
                    if(valuePart.get('GUID') == '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'):
                        asfContainerGUID = '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'
        fileSinkFactory = SinkFactories.FileSinkFactory(sinkControl.createSinkFactory(asfContainerGUID, fileSinkFactoreGUID))
        return fileSinkFactory
    def test_createEVRSinkFactory(self):        
        sinkControl = captureManager.createSinkControl()
        XMLstring = sinkControl.getCollectionOfSinks()
        #print XMLstring
        root = ElementTree.fromstring(XMLstring)
        sinkFactories = ElementPath.findall(root, "SinkFactory") 
        EVRSinkFactoreGUID = ''
        defaultContainerGUID = ''
        for sinkFactory in sinkFactories:
            if(sinkFactory.get('GUID') == '{2F34AF87-D349-45AA-A5F1-E4104D5C458E}'):
                EVRSinkFactoreGUID = '{2F34AF87-D349-45AA-A5F1-E4104D5C458E}'
                for valuePart in ElementPath.findall(sinkFactory, 'Value.ValueParts/ValuePart'):
                    if(valuePart.get('GUID') == '{71FBA544-3A8E-4D6C-B322-98184BC8DCEA}'):
                        defaultContainerGUID = '{71FBA544-3A8E-4D6C-B322-98184BC8DCEA}'
        
        EVRSinkFactory = SinkFactories.EVRSinkFactory(sinkControl.createSinkFactory(defaultContainerGUID, EVRSinkFactoreGUID))
        return EVRSinkFactory
    def test_createSampleGrabberCallSinkFactory(self):        
        sinkControl = captureManager.createSinkControl()
        XMLstring = sinkControl.getCollectionOfSinks()
        root = ElementTree.fromstring(XMLstring)
        sinkFactories = ElementPath.findall(root, "SinkFactory") 
        SampleGrabberCallSinkFactoryGUID = ''
        defaultContainerGUID = ''
        for sinkFactory in sinkFactories:
            if(sinkFactory.get('GUID') == '{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}'):
                SampleGrabberCallSinkFactoryGUID = '{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}'
                for valuePart in ElementPath.findall(sinkFactory, 'Value.ValueParts/ValuePart'):
                    if(valuePart.get('GUID') == '{C1864678-66C7-48EA-8ED4-48EF37054990}'):
                        defaultContainerGUID = '{C1864678-66C7-48EA-8ED4-48EF37054990}'
        
        SampleGrabberCallSinkFactory = SinkFactories.SampleGrabberCallSinkFactory(sinkControl.createSinkFactory(defaultContainerGUID, SampleGrabberCallSinkFactoryGUID))
        return SampleGrabberCallSinkFactory
    def test_createOutputNodesOfFileSinkFactory(self):
        fileSinkFactory = self.test_createFileSinkFactory()
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')
        outputMediaType = sourceControl.getSourceOutputMediaType(symboliclink, 0, 2)
        aArrayPtrCompressedMediaTypes = []
        aArrayPtrCompressedMediaTypes.append(outputMediaType)
        outputNodes = fileSinkFactory.createOutputNodes(aArrayPtrCompressedMediaTypes, 'test.asf')
        return outputNodes
    def test_createOutputNodeOfSampleGrabberCallSinkFactory(self):
        sampleGrabberCallSinkFactory = self.test_createSampleGrabberCallSinkFactory()        
        outputNodes = sampleGrabberCallSinkFactory.createOutputNode('{73646976-0000-0010-8000-00AA00389B71}', '{20-0000-0010-8000-00AA00389B71}', 10000, '{118AD3F7-D9A3-4146-AB35-F16421DC995E}')
        return outputNodes
        
class Test_StreamControl(unittest.TestCase):
    def test_getCollectionOfStreamControlNodeFactories(self):        
        streamControl = captureManager.createStreamControl()
        XMLstring = streamControl.getCollectionOfStreamControlNodeFactories()

    def test_createSpreaderNodeFactory(self):        
        streamControl = captureManager.createStreamControl()
        return streamControl.createSpreaderNodeFactory()

    def test_createFileSinkFactory(self):        
        sinkControl = captureManager.createSinkControl()
        XMLstring = sinkControl.getCollectionOfSinks()
        root = ElementTree.fromstring(XMLstring)
        sinkFactories = ElementPath.findall(root, "SinkFactory") 
        fileSinkFactoreGUID = ''
        asfContainerGUID = ''
        for sinkFactory in sinkFactories:
            if(sinkFactory.get('GUID') == '{D6E342E3-7DDD-4858-AB91-4253643864C2}'):
                fileSinkFactoreGUID = '{D6E342E3-7DDD-4858-AB91-4253643864C2}'
                for valuePart in ElementPath.findall(sinkFactory, 'Value.ValueParts/ValuePart'):
                    if(valuePart.get('GUID') == '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'):
                        asfContainerGUID = '{A2A56DA1-EB84-460E-9F05-FEE51D8C81E3}'
        fileSinkFactory = SinkFactories.FileSinkFactory(sinkControl.createSinkFactory(asfContainerGUID, fileSinkFactoreGUID))
        return fileSinkFactory

    def test_createSpreaderNode(self):        
        fileSinkFactory = self.test_createFileSinkFactory()
        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')
        outputMediaType = sourceControl.getSourceOutputMediaType(symboliclink, 0, 0)
        aArrayPtrCompressedMediaTypes = []
        aArrayPtrCompressedMediaTypes.append(outputMediaType)
        aArrayPtrCompressedMediaTypes.append(outputMediaType)
        outputNodes = fileSinkFactory.createOutputNodes(aArrayPtrCompressedMediaTypes, 'test.asf')        
        spreaderNodeFactory = self.test_createSpreaderNodeFactory()
        spreadNode = spreaderNodeFactory.createSpreaderNode(outputNodes)

class Test_MediaTypeParser(unittest.TestCase):
    def test_parse(self):        
        mediaTypeParser = captureManager.createMediaTypeParser()

        sourceControl = captureManager.createSourceControl()
        XMLstring = sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(XMLstring)
        sources = ElementPath.findall(selfSources, "Source") 
        symboliclink = ''
        for source in sources:
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        symboliclink = attribute.find('SingleValue').get('Value')
        outputMediaType = sourceControl.getSourceOutputMediaType(symboliclink, 0, 0)

        XMLstring = mediaTypeParser.parse(outputMediaType)
      

class Test_StrideForBitmap(unittest.TestCase):
    def test_getStrideForBitmap(self):        
        strideForBitmap = captureManager.createStrideForBitmap()    
        XMLstring = strideForBitmap.getStrideForBitmap('{20-0000-0010-8000-00AA00389B71}', 640)

class Test_VersionControl(unittest.TestCase):
    def test_getXMLStringVersion(self):        
        versionControl = captureManager.createVersionControl()   
        versionControl.getXMLStringVersion()
    def test_checkVersion(self):        
        versionControl = captureManager.createVersionControl()   
        versionControl.checkVersion(1,2,0)
     



       




if __name__ == '__main__':
    unittest.main()
