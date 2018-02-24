import Tkinter
from Tkconstants import *
from Tix import ComboBox, TixWidget

import Tix
from CaptureManagerPythonProxy import CaptureManager, ISourceControl, SinkFactories, SessionControl, WebCamControl
import WebCamProperties

import xml.etree.ElementTree as ElementTree

import xml.etree.ElementPath as ElementPath


class Viewer(object):
    """description of class"""
    def __init__(self, widget, captureManager):  
        self.captureManager = captureManager
        self.sourcesList = [] 
        self.prevSelectedSourceIndex = -1
        self.prevSelectedMediaTypeIndex = -1
        self.sourceComboBox = Tix.ComboBox(widget, label="Source: ", command=self.selectSource)
        self.sourceComboBox.pack(side=Tix.TOP, fill=Tix.X)        
        self.sourceControl = self.captureManager.createSourceControl()
        self.sinkControl = self.captureManager.createSinkControl()
        self.sessionControl = self.captureManager.createSessionControl()
        self.session = -1
        xmlstring = self.sourceControl.getCollectionOfSources()
        selfSources = ElementTree.fromstring(xmlstring)
        self.sources = ElementPath.findall(selfSources, "Source") 
        for source in self.sources:
            friendlyname = ''
            state = False
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME'):          
                        friendlyname = attribute.find('SingleValue').get('Value')
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_MEDIA_TYPE'):          
                        parts = attribute.find('Value.ValueParts').findall("ValuePart")
                        for part in parts:
                            if(part.get('Value') == 'MFMediaType_Video'):
                                state = True

            if(state):
                self.sourceComboBox.insert(Tix.END, friendlyname)
                self.sourcesList.append(source)
        self.mediaTypeComboBox = Tix.ComboBox(widget, label="MediaTypes: ", state='disabled', command=self.selectMediaType)
        self.mediaTypeComboBox.pack(side=Tix.TOP, fill=Tix.X)
        self.controlBtnbox = Tix.ButtonBox(widget, orientation=Tix.HORIZONTAL)
        self.startBtn = self.controlBtnbox.add('start', text='Start', underline=0, width=6, state='disabled',
                command=lambda widget=widget: self.startCaptureSession())
        self.optionsBtn = self.controlBtnbox.add('options', text='Options', underline=0, width=6, state='disabled',
                command=lambda widget=widget: self.options())
        self.stopBtn = self.controlBtnbox.add('stop', text='Stop', underline=0, width=6, state='disabled',
                command=lambda widget=widget: self.stopCaptureSession())
        self.controlBtnbox.pack(side=Tix.TOP, fill=Tix.X)

        self.frame = Tkinter.Frame(widget, relief=RIDGE, borderwidth=2)
        self.frame.pack(fill=BOTH,expand=1)
       
    def selectSource(self, event): 
        selectedSourceIndex = self.sourceComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        if(self.prevSelectedSourceIndex != selectedSourceIndex[0]):
            self.prevSelectedSourceIndex = selectedSourceIndex[0] 
            source = self.sourcesList[int(selectedSourceIndex[0])]  
            for attributes in ElementPath.findall(source, "Source.Attributes"):
                for attribute in ElementPath.findall(attributes, "Attribute"):
                    if(attribute.get('Name') == 'MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK'):          
                        self.symbolicLink = attribute.find('SingleValue').get('Value')                    

            self.populateMediaTypeComboBox(int(selectedSourceIndex[0]))  

    def populateMediaTypeComboBox(self, selectedSourceIndex):
        if self.mediaTypeComboBox.size() > 0:
            self.mediaTypeComboBox.slistbox.listbox.delete(0, Tix.END)
        self.mediaTypeComboBox.config(state="normal")
        source = self.sourcesList[selectedSourceIndex]
        presentationDescriptor = source.find('PresentationDescriptor')
        streamdescriptor = presentationDescriptor.find('StreamDescriptor')
        mediatypes = streamdescriptor.find('MediaTypes').findall('MediaType')
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

            self.mediaTypeComboBox.insert(Tix.END, width + ' x ' + height + ', ' + framerate + ', ' + mediaformat)
       
    def selectMediaType(self, event): 
        self.controlBtnbox.subwidget('start').config(state="normal")        
        selectedMediaTypeIndex = self.mediaTypeComboBox.subwidget_list['slistbox'].subwidget_list['listbox'].curselection()
        if(self.prevSelectedMediaTypeIndex != selectedMediaTypeIndex[0]):
            self.prevSelectedMediaTypeIndex = selectedMediaTypeIndex[0] 
  
    def startCaptureSession(self):                
        XMLstring = self.sinkControl.getCollectionOfSinks()
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
        
        EVRSinkFactory = SinkFactories.EVRSinkFactory(self.sinkControl.createSinkFactory(defaultContainerGUID, EVRSinkFactoreGUID))

        indexOfStream = 0
        indexOfMediaType = int(self.prevSelectedMediaTypeIndex)
        outputNode = EVRSinkFactory.createOutputNode(self.frame.winfo_id())

        sourceNode = self.sourceControl.createSourceNodeWithDownStreamConnection(self.symbolicLink, indexOfStream, indexOfMediaType, outputNode)

        sources = []
        sources.append(sourceNode)
        self.session = self.sessionControl.createSession(sources)
    
        self.session.startSession(0, '{00000000-0000-0000-0000-000000000000}')
        self.controlBtnbox.subwidget('start').config(state='disabled') 
        self.controlBtnbox.subwidget('stop').config(state='normal') 
        self.controlBtnbox.subwidget('options').config(state='normal') 



    def stopCaptureSession(self):               
        self.session.stopSession()
        self.controlBtnbox.subwidget('start').config(state='normal') 
        self.controlBtnbox.subwidget('stop').config(state='disabled') 
        self.controlBtnbox.subwidget('options').config(state='disabled') 

    def options(self):    


        webCamControl = self.sourceControl.createWebCamControl(self.symbolicLink)

        root = Tix.Tk()
        WebCamProperties.WebCamProperties(root, webCamControl)
        root.mainloop()   
        