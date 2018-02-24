import sys, os
import ctypes
import comtypes

class SinkControl(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def getCollectionOfSinks(self):
        method = "getCollectionOfSinks"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        return self.IDispatch.Invoke(id[0], *arg)
        
    def createSinkFactory(self, asfContainerGUID, fileSinkFactoreGUID):
        method = "createSinkFactory"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(asfContainerGUID)
        arg.append(fileSinkFactoreGUID)
        return self.IDispatch.Invoke(id[0], *arg)


