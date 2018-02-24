import sys, os
import ctypes
import comtypes

class VersionControl(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}  
        
    def getXMLStringVersion(self):
        method = "getXMLStringVersion"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        return self.IDispatch.Invoke(id[0], *arg)
        
    def checkVersion(self, aMAJOR, aMINOR, aPATCH):
        method = "checkVersion"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aMAJOR)
        arg.append(aMINOR)
        arg.append(aPATCH)
        return self.IDispatch.Invoke(id[0], *arg)



