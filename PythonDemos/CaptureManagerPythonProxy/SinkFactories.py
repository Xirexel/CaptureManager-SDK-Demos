import sys, os
import ctypes
import comtypes
from comtypes import automation
import SampleGrabberCallSink

class FileSinkFactory(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def createOutputNodes(self, aArrayPtrCompressedMediaTypes, aPtrFileName):
        method = "createOutputNodes"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aArrayPtrCompressedMediaTypes)
        arg.append(aPtrFileName)
        return self.IDispatch.Invoke(id[0], *arg)

class EVRSinkFactory(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def createOutputNode(self, aHWND):
        method = "createOutputNode"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aHWND)
        return self.IDispatch.Invoke(id[0], *arg)

class SampleGrabberCallSinkFactory(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def createOutputNode(self, aMajorType, aSubType, aSampleByteSize, aIID):
        method = "createOutputNode"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aMajorType)
        arg.append(aSubType)
        arg.append(aSampleByteSize)
        arg.append(aIID)
        return SampleGrabberCallSink.SampleGrabberCallSink(self.IDispatch.Invoke(id[0], *arg))


