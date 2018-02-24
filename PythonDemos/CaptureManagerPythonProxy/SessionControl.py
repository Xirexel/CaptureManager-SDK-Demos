import sys, os
import ctypes
import comtypes
import Session

class SessionControl(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   
        
    def createSession(self, aArrayPtrSourceNodesOfTopology):
        method = "createSession"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        aIID = '{742AC001-D1E0-40A8-8EFE-BA1A550F8805}'	
        arg = []
        arg.append(aArrayPtrSourceNodesOfTopology)
        arg.append(aIID)
        return Session.Session(self.IDispatch.Invoke(id[0], *arg))




