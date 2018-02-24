import sys, os
import ctypes
import comtypes

class WebCamControl(object):
    """description of class"""
    def __init__(self, interface):
        self.IDispatch = interface.QueryInterface(comtypes.automation.IDispatch)  
        self.currentlcid = {"lcid": 0}   

    def getCamParametrs(self):
        method = "getCamParametrs"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        return self.IDispatch.Invoke(id[0], *arg)

    def getCamParametr(self, aParametrIndex):
        method = "getCamParametr"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aParametrIndex)
        return self.IDispatch.Invoke(id[0], *arg)

    def setCamParametr(self, aParametrIndex, aNewValue, aFlag):
        method = "setCamParametr"
        id = self.IDispatch.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aParametrIndex)
        arg.append(aNewValue)
        arg.append(aFlag)
        self.IDispatch.Invoke(id[0], *arg)

