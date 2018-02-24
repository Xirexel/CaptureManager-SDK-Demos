import sys, os
import ctypes
import comtypes
import comtypes.client as cc

class LogPrintOut(object):
    """Class for managment of log print out."""
    def __init__(self):
        self.logPrintOut = cc.CreateObject('{4563EE3E-DA1E-4911-9F40-88A284E2DD69}', None, None, comtypes.automation.IDispatch)
        self.currentlcid = {"lcid": 0}

    def setVerbose(self, aLevelType, aFilePath, aState):   
        method = "setVerbose"
        id = self.logPrintOut.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aLevelType)
        arg.append(aFilePath)
        arg.append(aState)
        self.logPrintOut.Invoke(id[0], *arg)

    def addPrintOutDestination(self, aLevelType, aFilePath):   
        method = "addPrintOutDestination"
        id = self.logPrintOut.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aLevelType)
        arg.append(aFilePath)
        self.logPrintOut.Invoke(id[0], *arg)

    def removePrintOutDestination(self, aLevelType, aFilePath):   
        method = "removePrintOutDestination"
        id = self.logPrintOut.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append(aLevelType)
        arg.append(aFilePath)
        self.logPrintOut.Invoke(id[0], *arg)
