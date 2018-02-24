import sys, os
import ctypes
import comtypes
import comtypes.client as cc
from CaptureManagerPythonProxy import SourceControl, SinkControl, SessionControl, EncoderControl, StreamControl, MediaTypeParser, StrideForBitmap, VersionControl

class CaptureManager(object):
    """Class for managment of capture"""
    def __init__(self):
        self.captureManager = cc.CreateObject('{D5F07FB8-CE60-4017-B215-95C8A0DDF42A}', None, None, comtypes.automation.IDispatch)
        self.currentlcid = {"lcid": 0}

    def createSourceControl(self):  
        method = "createControl"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{1276CC17-BCA8-4200-87BB-7180EF562447}')
        return SourceControl.SourceControl(self.captureManager.Invoke(id[0], *arg))

    def createSinkControl(self):  
        method = "createControl"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{C6BA3732-197E-438B-8E73-277759A7B58F}')
        return SinkControl.SinkControl(self.captureManager.Invoke(id[0], *arg))

    def createSessionControl(self):  
        method = "createControl"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{D0C58520-A941-4C0F-81B0-3ED8A4DE11ED}')
        return SessionControl.SessionControl(self.captureManager.Invoke(id[0], *arg))

    def createEncoderControl(self):  
        method = "createControl"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{96223507-D8FF-4EC1-B125-71AA7F9726A4}')
        return EncoderControl.EncoderControl(self.captureManager.Invoke(id[0], *arg))

    def createStreamControl(self):  
        method = "createControl"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{E8F25B4A-8C71-4C9E-BD8C-82260DC4C21B}')
        return StreamControl.StreamControl(self.captureManager.Invoke(id[0], *arg))

    def createMediaTypeParser(self):  
        method = "createMisc"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{74F0DC2B-E470-4359-A1E7-467B521BDFE1}')
        return MediaTypeParser.MediaTypeParser(self.captureManager.Invoke(id[0], *arg))

    def createStrideForBitmap(self):  
        method = "createMisc"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{74D903C9-69E6-4FC7-BF7A-9F47605C52BE}')
        return StrideForBitmap.StrideForBitmap(self.captureManager.Invoke(id[0], *arg))

    def createVersionControl(self):  
        method = "createMisc"
        id = self.captureManager.GetIDsOfNames(method, **self.currentlcid)
        arg = []
        arg.append('{39DC3AEF-3B59-4C0D-A1B2-54BF2653C056}')
        return VersionControl.VersionControl(self.captureManager.Invoke(id[0], *arg))



