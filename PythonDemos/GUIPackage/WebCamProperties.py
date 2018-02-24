import Tkinter
from Tkconstants import *
from Tix import ComboBox, TixWidget

import Tix
from CaptureManagerPythonProxy import CaptureManager, ISourceControl, SinkFactories, SessionControl

import xml.etree.ElementTree as ElementTree

import xml.etree.ElementPath as ElementPath
class WebCamProperties(object):
    """description of class"""
    def __init__(self, widget, webCamControl):  
        self.widget = widget
        self.webCamControl = webCamControl
        self.mainFrame = Tix.Frame(widget, bd=1, relief=Tix.RAISED)
        widget.wm_minsize(600, 400)  
        camParametrs = self.webCamControl.getCamParametrs()          
        parametrs = ElementTree.fromstring(camParametrs)
        groups = ElementPath.findall(parametrs, "Group") 
        for group in groups:
            self.fillGroup(group)


    def fillGroup(self, group):
        title = group.get('Title')
        labelframe = Tkinter.LabelFrame(self.widget, text=title)
        labelframe.pack(fill="both", expand="yes")
        parametrs = ElementPath.findall(group, "Parametr") 
        
        index = 0
        for parametr in parametrs:           
            WebCamProperty(labelframe, parametr, self.webCamControl, index / 5, index % 5)
            index = index + 1
 
class WebCamProperty(object):
    """description of class"""
    def __init__(self, panel, parametr, webCamControl, row, column): 
        self.webCamControl = webCamControl
        parametrTitle = parametr.get('Title')     
        labelframe = Tkinter.Frame(panel)
        min = parametr.get('Min')
        max = parametr.get('Max')
        currentValue = parametr.get('CurrentValue')
        self.defaultValue = parametr.get('Default')
        self.flag = parametr.get('Flag')
        self.index = parametr.get('Index')
        var = Tkinter.DoubleVar()
        self.scale = Tkinter.Scale(labelframe, state='normal', from_=int(min), to=int(max), orient=HORIZONTAL, label=parametrTitle, variable = var , command = self.changeValue)
        self.scale.set(int(currentValue))
        self.scale.pack(side=LEFT)
        self.checkValue = Tkinter.IntVar(0)
        checkbutton = Tkinter.Checkbutton(labelframe, command=self.changeFlag, variable=self.checkValue, onvalue = 1, offvalue = 0)
        checkbutton.pack(side=LEFT)
        if(self.flag != '2'):
            checkbutton.deselect()
            self.scale.config(state='disable')
        else:
            checkbutton.select()
        labelframe.grid(row=row, column=column)
            
    def changeValue(self, event):
        self.webCamControl.setCamParametr(int(self.index), int(event), 2)

    def changeFlag(self): 
        if(self.flag == '2'):
            self.flag = '1'
            self.scale.config(state='disable')
            self.webCamControl.setCamParametr(int(self.index), int(self.defaultValue), 1)
        else:
            self.flag = '2'
            self.scale.config(state='normal')
