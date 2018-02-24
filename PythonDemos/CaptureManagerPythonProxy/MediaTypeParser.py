import sys, os
import ctypes
import comtypes

class MediaTypeParser(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}  
        
    def parse(self, MediaType):
        method = "parse"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(MediaType)
        return self.IDispatch.Invoke(id[0], *arg)


