import sys, os
import ctypes
import comtypes
import Session


class EncoderControl(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def getCollectionOfEncoders(self):
        method = "getCollectionOfEncoders"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        return self.IDispatch.Invoke(id[0], *arg)
        
    def getMediaTypeCollectionOfEncoder(self, UncompressedMediaType, encoderCLSID):
        method = "getMediaTypeCollectionOfEncoder"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(UncompressedMediaType)
        arg.append(encoderCLSID)
        return self.IDispatch.Invoke(id[0], *arg)
        
    def createEncoderNodeFactory(self, encoderCLSID):
        method = "createEncoderNodeFactory"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        aIID = '{A56E11D8-D602-4792-8570-38C283FC0AA3}'	
        arg = []
        arg.append(encoderCLSID)
        arg.append(aIID)
        return EncoderNodeFactory(self.IDispatch.Invoke(id[0], *arg))



class EncoderNodeFactory(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0} 
        
    def createCompressedMediaType(self, aUncompressedMediaType, aEncodingModeGUID, aEncodingModeValue, aIndexCompressedMediaType):
        method = "createCompressedMediaType"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aUncompressedMediaType)
        arg.append(aEncodingModeGUID)
        arg.append(aEncodingModeValue)
        arg.append(aIndexCompressedMediaType)
        return self.IDispatch.Invoke(id[0], *arg)  
        
    def createEncoderNode(self, aUncompressedMediaType, aEncodingModeGUID, aEncodingModeValue, aIndexCompressedMediaType, aDownStreamNode):
        method = "createEncoderNode"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aUncompressedMediaType)
        arg.append(aEncodingModeGUID)
        arg.append(aEncodingModeValue)
        arg.append(aIndexCompressedMediaType)
        arg.append(aDownStreamNode)
        return self.IDispatch.Invoke(id[0], *arg) 



