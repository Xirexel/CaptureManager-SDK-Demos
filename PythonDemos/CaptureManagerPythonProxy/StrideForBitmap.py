import sys, os
import ctypes
import comtypes

class StrideForBitmap(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}  
        
    def getStrideForBitmap(self, aGUIDMFVideoFormat, aWidthInPixels):
        method = "getStrideForBitmap"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aGUIDMFVideoFormat)
        arg.append(aWidthInPixels)
        return self.IDispatch.Invoke(id[0], *arg)


