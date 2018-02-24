import sys, os
import ctypes
import comtypes

class Session(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def startSession(self, aStartPositionInHundredNanosecondUnits, aGUIDTimeFormat):
        method = "startSession"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aStartPositionInHundredNanosecondUnits)
        arg.append(aGUIDTimeFormat)
        self.IDispatch.Invoke(id[0], *arg)
        
    def stopSession(self):
        method = "stopSession"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        self.IDispatch.Invoke(id[0], *arg)
        
    def pauseSession(self):
        method = "pauseSession"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        self.IDispatch.Invoke(id[0], *arg)
        
    def closeSession(self):
        method = "closeSession"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        self.IDispatch.Invoke(id[0], *arg)
        
    def getSessionDescriptor(self):
        method = "getSessionDescriptor"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        return self.IDispatch.Invoke(id[0], *arg)

