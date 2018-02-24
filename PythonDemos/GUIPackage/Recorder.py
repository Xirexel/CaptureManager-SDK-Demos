import Tkinter
from Tkconstants import *
from Tix import ComboBox, TixWidget
from FileDialog import SaveFileDialog

import Tix
from CaptureManagerPythonProxy import CaptureManager, ISourceControl, SinkFactories, SessionControl, WebCamControl
import WebCamProperties

import xml.etree.ElementTree as ElementTree

import xml.etree.ElementPath as ElementPath

class Recorder(object):
    """description of class"""
    def __init__(self, widget, captureManager):  
        self.captureManager = captureManager
        self.sourcesList = [] 
        self.prevSelectedSourceIndex = -1
        self.prevSelectedMediaTypeIndex = -1
        self.prevSelectedEncoderIndex = -1
        self.streamdescriptor = -1
        self.prevSelectedEncoderModeIndex = -1
        self.prevSelectedOutputContainerTypeIndex = -1
        self.sourceComboBox = Tix.ComboBox(widget, label="Source: ", command=self.selectSource)
        self.sourceComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.sourceControl = self.captureManager.createSourceControl()
        self.sinkControl = self.captureManager.createSinkControl()
        self.encoderControl = self.captureManager.createEncoderControl()
        self.sessionControl = self.captureManager.createSessionControl()
        self.session = None 
        self.encoderCLSIDList = [] 
        self.selectedEncoderCLSID = -1;
        selfSources = ElementTree.fromstring(self.sourceControl.getCollectionOfSources())
        self.sources = ElementPath.findall(selfSources, "Source") 
        for source in self.sources:
            friendlyname = ''
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME'):          
                        friendlyname = attribute.find('SingleValue').get('Value')
            self.sourceComboBox.insert(Tix.END, friendlyname)
            self.sourcesList.append(source)
        self.mediaTypeComboBox = Tix.ComboBox(widget, label="MediaType: ", state='disabled', command=self.selectMediaType)
        self.mediaTypeComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.encoderComboBox = Tix.ComboBox(widget, label="Encoder: ", state='disabled', command=self.selectEncoder)
        self.encoderComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.encoderModeComboBox = Tix.ComboBox(widget, label="EncoderMode: ", state='disabled', command=self.selectEncoderMode)
        self.encoderModeComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.encoderOutputMediaTypeComboBox = Tix.ComboBox(widget, label="EncoderOutputMediaType: ", state='disabled', command=self.selectEncoderOutputMediaType)
        self.encoderOutputMediaTypeComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.outputSinkTypeComboBox = Tix.ComboBox(widget, label="OutputSinkType: ", state='disabled', command=self.selectOutputSinkType)
        self.outputSinkTypeComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.outputContainerTypeComboBox = Tix.ComboBox(widget, label="OutputContainerType: ", state='disabled', command=self.selectOutputContainerType)
        self.outputContainerTypeComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.optionsBtn = Tkinter.Button(widget, text='Options', state='disabled', command=self.showOptions)
        self.optionsBtn.pack(side=Tix.TOP, fill=Tix.X)
        self.sessionBtn = Tkinter.Button(widget, text='Session', state='disabled', command=self.initSession)
        self.sessionBtn.pack(side=Tix.TOP, fill=Tix.X)
     
    def selectSource(self, event): 
        selectedSourceIndex = self.sourceComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        if(self.prevSelectedSourceIndex != selectedSourceIndex[0]):
            self.prevSelectedSourceIndex = selectedSourceIndex[0] 
            source = self.sourcesList[int(selectedSourceIndex[0])]  
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        self.symbolicLink = attribute.find('SingleValue').get('Value')  
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK'):          
                        self.symbolicLink = attribute.find('SingleValue').get('Value')                    

            self.populateMediaTypeComboBox(int(selectedSourceIndex[0]))  

    def populateMediaTypeComboBox(self, selectedSourceIndex):
        if self.mediaTypeComboBox.size() > 0:
            self.mediaTypeComboBox.slistbox.listbox.delete(0, Tix.END)
        self.mediaTypeComboBox.config(state='normal')
        source = self.sourcesList[selectedSourceIndex]
        presentationDescriptor = source.find('PresentationDescriptor')
        self.streamdescriptor = presentationDescriptor.find('StreamDescriptor')
        mediatypes = self.streamdescriptor.find('MediaTypes').findall('MediaType')
        for mediatype in mediatypes:
            width = 0
            height = 0
            framerate = 0
            mediaformat = ''
            mediatypeitems = mediatype.findall('MediaTypeItem')
            for mediatypeitem in mediatypeitems:                
                if mediatypeitem.get('Name') == 'MF_MT_FRAME_SIZE':
                    valueParts = mediatypeitem.find('Value.ValueParts').findall('ValuePart')
                    width = valueParts[0].get('Value')
                    height = valueParts[1].get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_FRAME_RATE':
                        framerate = mediatypeitem.find('RatioValue').get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_SUBTYPE':
                        mediaformat = mediatypeitem.find('SingleValue').get('Value')
                
                if mediatypeitem.get('Name') == 'MF_MT_AUDIO_NUM_CHANNELS':
                    width = mediatypeitem.find('SingleValue').get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_AUDIO_SAMPLES_PER_SECOND':
                    height = mediatypeitem.find('SingleValue').get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_AUDIO_BITS_PER_SAMPLE':
                        framerate = mediatypeitem.find('SingleValue').get('Value')

            self.mediaTypeComboBox.insert(Tix.END, width + ' x ' + height + ', ' + framerate + ', ' + mediaformat)
       
    def selectMediaType(self, event):      
        selectedMediaTypeIndex = self.mediaTypeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        if(self.prevSelectedMediaTypeIndex != selectedMediaTypeIndex[0]):
            self.prevSelectedMediaTypeIndex = selectedMediaTypeIndex[0]   
            self.populateEncoderComboBox(int(selectedMediaTypeIndex[0]))  

    def populateEncoderComboBox(self, selectedSourceIndex):
        if self.encoderComboBox.size() > 0:
            self.encoderComboBox.slistbox.listbox.delete(0, Tix.END)
        self.encoderCLSIDList = []
        self.encoderComboBox.config(state='normal')
        encodersXML = ElementTree.fromstring(self.encoderControl.getCollectionOfEncoders())
        groups = ElementPath.findall(encodersXML, "Group") 
        for group in groups:
            if(group.get('GUID') == self.streamdescriptor.get('MajorTypeGUID')):
                for encoderFactory in ElementPath.findall(group, "EncoderFactory"):    
                    self.encoderComboBox.insert(Tix.END, encoderFactory.get('Name'))
                    self.encoderCLSIDList.append(encoderFactory.get('CLSID'))

  
    def selectEncoder(self, event):    
        selectedEncoderIndex = self.encoderComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        if(self.prevSelectedEncoderIndex != selectedEncoderIndex[0]):
            self.prevSelectedEncoderIndex = selectedEncoderIndex[0] 
            self.encoderCLSID = self.encoderCLSIDList[int(selectedEncoderIndex[0])]   
            self.populateEncoderModeComboBox(self.encoderCLSID)  

    def populateEncoderModeComboBox(self, encoderCLSID):
        if self.encoderModeComboBox.size() > 0:
            self.encoderModeComboBox.slistbox.listbox.delete(0, Tix.END)

        self.encoderModeComboBox.config(state='normal')
        self.encoderModelist = []
        self.encoderModeGUIDlist = []
        selectedMediaTypeIndex = self.mediaTypeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        outputMediaType = self.sourceControl.getSourceOutputMediaType(self.symbolicLink, 0, int(selectedMediaTypeIndex[0]));
     
        encoderOutputMedaiTypesXml = ElementTree.fromstring(self.encoderControl.getMediaTypeCollectionOfEncoder(outputMediaType, encoderCLSID))
        groups = ElementPath.findall(encoderOutputMedaiTypesXml, "Group") 
        for group in groups:
            self.encoderModeComboBox.insert(Tix.END, group.get('Title'))
            self.encoderModelist.append(group)  
            self.encoderModeGUIDlist.append(group.get('GUID'))        
        
    def selectEncoderMode(self, event):    
        selectedEncoderModeIndex = self.encoderModeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        if(self.prevSelectedEncoderModeIndex != selectedEncoderModeIndex[0]):
            self.prevSelectedEncoderModeIndex = selectedEncoderModeIndex[0] 
            encoderModeGroup = self.encoderModelist[int(selectedEncoderModeIndex[0])]   
            self.populateEncoderOutputMediaTypeComboBox(encoderModeGroup)  

    def populateEncoderOutputMediaTypeComboBox(self, encoderModeGroup):
        if self.encoderOutputMediaTypeComboBox.size() > 0:
            self.encoderOutputMediaTypeComboBox.slistbox.listbox.delete(0, Tix.END)
        self.encoderOutputMediaTypeComboBox.config(state='normal')
        mediatypes = encoderModeGroup.find('MediaTypes').findall('MediaType')
        for mediatype in mediatypes:
            width = 0
            height = 0
            framerate = 0
            mediaformat = ''
            mediatypeitems = mediatype.findall('MediaTypeItem')
            for mediatypeitem in mediatypeitems:                
                if mediatypeitem.get('Name') == 'MF_MT_FRAME_SIZE':
                    valueParts = mediatypeitem.find('Value.ValueParts').findall('ValuePart')
                    width = valueParts[0].get('Value')
                    height = valueParts[1].get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_FRAME_RATE':
                        framerate = mediatypeitem.find('RatioValue').get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_SUBTYPE':
                        mediaformat = mediatypeitem.find('SingleValue').get('Value')
                
                if mediatypeitem.get('Name') == 'MF_MT_AUDIO_NUM_CHANNELS':
                    width = mediatypeitem.find('SingleValue').get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_AUDIO_SAMPLES_PER_SECOND':
                    height = mediatypeitem.find('SingleValue').get('Value')
                elif mediatypeitem.get('Name') == 'MF_MT_AUDIO_AVG_BYTES_PER_SECOND':
                        framerate = str(int(mediatypeitem.find('SingleValue').get('Value')) * 8 / 1000) + ' kb/s'
    
                if(framerate == 0):
                    framerate = ''


            self.encoderOutputMediaTypeComboBox.insert(Tix.END, width + ' x ' + height + ', ' + framerate + ', ' + mediaformat)

    def selectEncoderOutputMediaType(self, event):    
        if self.outputSinkTypeComboBox.size() > 0:
            self.outputSinkTypeComboBox.slistbox.listbox.delete(0, Tix.END)
        self.outputSinkTypeComboBox.config(state='normal')
        self.outputSinkTypeComboBox.insert(Tix.END, 'File')

    def selectOutputSinkType(self, event):    
        self.populateOutputContainerTypeComboBox()

    def populateOutputContainerTypeComboBox(self):
        if self.outputContainerTypeComboBox.size() > 0:
            self.outputContainerTypeComboBox.slistbox.listbox.delete(0, Tix.END)

        self.outputContainerTypeComboBox.config(state='normal')
        self.outputContainerTypeGUIDList = []
        self.outputContainerTypeList = []
        XMLstring = self.sinkControl.getCollectionOfSinks()
        root = ElementTree.fromstring(XMLstring)
        sinkFactories = ElementPath.findall(root, "SinkFactory")
        for sinkFactory in sinkFactories:
            if(sinkFactory.get('GUID') == '{D6E342E3-7DDD-4858-AB91-4253643864C2}'):
                for valuePart in ElementPath.findall(sinkFactory, 'Value.ValueParts/ValuePart'):
                    self.outputContainerTypeComboBox.insert(Tix.END, valuePart.get('Value'))
                    self.outputContainerTypeGUIDList.append(valuePart.get('GUID'))
                    self.outputContainerTypeList.append(valuePart.get('Value'))
  
    def selectOutputContainerType(self, event):  
        self.optionsBtn.config(state='normal')
        selectedOutputContainerTypeIndex = self.outputContainerTypeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        if(self.prevSelectedOutputContainerTypeIndex != selectedOutputContainerTypeIndex[0]):            
            self.prevSelectedOutputContainerTypeIndex = selectedOutputContainerTypeIndex[0] 
            self.outputContainerTypeGUID = self.outputContainerTypeGUIDList[int(selectedOutputContainerTypeIndex[0])]  
            self.outputContainerType = self.outputContainerTypeList[int(selectedOutputContainerTypeIndex[0])]  
             
          
    def showOptions(self):     
        saveFileDialog = SaveFileDialog(self.optionsBtn)
        self.filePath = saveFileDialog.go(pattern='*.' + self.outputContainerType)
        if(self.filePath != None):
            self.sessionBtn.config(state='normal')
            
    def initSession(self): 
    
        if(self.session == None):
            fileSinkFactoreGUID = '{D6E342E3-7DDD-4858-AB91-4253643864C2}'
            fileSinkFactory = SinkFactories.FileSinkFactory(self.sinkControl.createSinkFactory(self.outputContainerTypeGUID, fileSinkFactoreGUID))
            selectedMediaTypeIndex = self.mediaTypeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
            inputMediaType = self.sourceControl.getSourceOutputMediaType(self.symbolicLink, 0, int(selectedMediaTypeIndex[0]));
            encoderNodeFactory = self.encoderControl.createEncoderNodeFactory(self.encoderCLSID)
            encoderModeGUID = self.encoderModeGUIDlist[int(self.encoderModeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()[0])]
            compressedMediaTypeIndex = int(self.encoderOutputMediaTypeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()[0])

            compressedMediaType = encoderNodeFactory.createCompressedMediaType(inputMediaType, encoderModeGUID, 80, compressedMediaTypeIndex)

            arrayPtrCompressedMediaTypes = []     
            arrayPtrCompressedMediaTypes.append(compressedMediaType) 

            outputNode = fileSinkFactory.createOutputNodes(arrayPtrCompressedMediaTypes, self.filePath)

            encoderNode = encoderNodeFactory.createEncoderNode(inputMediaType, encoderModeGUID, 80, compressedMediaTypeIndex, outputNode[0])

            sourceNode = self.sourceControl.createSourceNodeWithDownStreamConnection(self.symbolicLink, 0, int(selectedMediaTypeIndex[0]), encoderNode)

            sources = []
            sources.append(sourceNode)
            self.session = self.sessionControl.createSession(sources)
    
            self.session.startSession(0, '{00000000-0000-0000-0000-000000000000}')
            
            self.sessionBtn.config(text='stop')
        else:
            self.session.stopSession()
            self.session.closeSession()
