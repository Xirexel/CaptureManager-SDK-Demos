import sys, os
import ctypes
import comtypes

class StreamControl(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}  
        
    def getCollectionOfStreamControlNodeFactories(self):
        method = "getCollectionOfStreamControlNodeFactories"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        return self.IDispatch.Invoke(id[0], *arg)
        
    def createSpreaderNodeFactory(self):
        method = "createStreamControlNodeFactory"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{85DFAAA1-4CC0-4A88-AE28-8F492E552CCA}')
        return SpreaderNodeFactory(self.IDispatch.Invoke(id[0], *arg))


class SpreaderNodeFactory(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}  
        
    def createSpreaderNode(self, aArrayPtrDownStreamTopologyNodes):
        method = "createSpreaderNode"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aArrayPtrDownStreamTopologyNodes)
        return self.IDispatch.Invoke(id[0], *arg)

