import sys, os
import ctypes
import comtypes
import WebCamControl

class SourceControl(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def getCollectionOfSources(self):
        method = "getCollectionOfSources"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        return self.IDispatch.Invoke(id[0], *arg)

    def getSourceOutputMediaType(self, aSymbolicLink, streamIndex, mediaTypeIndex):
        method = "getSourceOutputMediaType"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aSymbolicLink)
        arg.append(streamIndex)
        arg.append(mediaTypeIndex)
        return self.IDispatch.Invoke(id[0], *arg)

    def createSourceNode(self, aSymbolicLink, streamIndex, mediaTypeIndex):
        method = "createSourceNode"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aSymbolicLink)
        arg.append(streamIndex)
        arg.append(mediaTypeIndex)
        return self.IDispatch.Invoke(id[0], *arg)

    def createSourceNodeWithDownStreamConnection(self, aSymbolicLink, aIndexStream, aIndexMediaType, aPtrDownStreamTopologyNode):
        method = "createSourceNodeWithDownStreamConnection"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aSymbolicLink)
        arg.append(aIndexStream)
        arg.append(aIndexMediaType)
        arg.append(aPtrDownStreamTopologyNode)
        return self.IDispatch.Invoke(id[0], *arg)

    def createSource(self, aSymbolicLink):
        method = "createSource"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aSymbolicLink)
        return self.IDispatch.Invoke(id[0], *arg)

    def createSourceFromCaptureProcessor(self, captureProcessor):
        method = "createSourceFromCaptureProcessor"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(captureProcessor)
        return self.IDispatch.Invoke(id[0], *arg)

    def createSourceNodeFromExternalSource(self, aMediaSource, aIndexStream, aIndexMediaType):
        method = "createSourceNodeFromExternalSource"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aMediaSource)
        arg.append(aIndexStream)
        arg.append(aIndexMediaType)
        return self.IDispatch.Invoke(id[0], *arg)

    def getSourceOutputMediaTypeFromMediaSource(self, aMediaSource, aIndexStream, aIndexMediaType):
        method = "getSourceOutputMediaTypeFromMediaSource"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aMediaSource)
        arg.append(aIndexStream)
        arg.append(aIndexMediaType)
        return self.IDispatch.Invoke(id[0], *arg)

    def createSourceNodeFromExternalSourceWithDownStreamConnection(self, aMediaSource, aIndexStream, aIndexMediaType, aPtrDownStreamTopologyNode):
        method = "createSourceNodeFromExternalSourceWithDownStreamConnection"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aMediaSource)
        arg.append(aIndexStream)
        arg.append(aIndexMediaType)
        arg.append(aPtrDownStreamTopologyNode)
        return self.IDispatch.Invoke(id[0], *arg)

    def createWebCamControl(self, aSymbolicLink):
        method = "createSourceControl"
        webCamGUID = '{3BD92C4C-5E06-4901-AE0B-D97E3902EAFC}'
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aSymbolicLink)
        arg.append(webCamGUID)
        return WebCamControl.WebCamControl(self.IDispatch.Invoke(id[0], *arg))


        
