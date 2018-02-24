import Tkinter
from Tkconstants import *
from Tix import ComboBox, TixWidget
import Tix
from GUIPackage import Viewer, Recorder
from CaptureManagerPythonProxy import CaptureManager

captureManager = CaptureManager.CaptureManager()

def runMain(widget):
    mainFrame = Tix.Frame(widget, bd=1, relief=Tix.RAISED)
    widget.wm_minsize(600, 400)
    widget.title('Capture Manager Python Demo')
    mainFrame.pack(side=Tix.TOP, fill=Tix.BOTH, expand=1)
    box = Tix.ButtonBox(widget, orientation=Tix.HORIZONTAL)
    box.add('viewer', text='Viewer', underline=0, width=6,
            command=lambda mainFrame=mainFrame: invokeViewer(mainFrame))
    box.add('recorder', text='Recorder', underline=0, width=6,
            command=lambda mainFrame=mainFrame: invokeRecorder(mainFrame))
    box.pack(side=Tix.BOTTOM, fill=Tix.X)
    mainFrame.pack(side=Tix.TOP, fill=Tix.BOTH, expand=1)

def invokeViewer(widget):    
    try:
        for c in widget.winfo_children():
            c.destroy()
        mainFrame = Tix.Frame(widget, bd=1, relief=Tix.RAISED)
        mainFrame.pack(side=Tix.TOP, fill=Tix.BOTH, expand=1)
        v = Viewer.Viewer(mainFrame, captureManager)
    except:  
        print 'Something went wrong.'

def invokeRecorder(widget):
    try:
        for c in widget.winfo_children():
            c.destroy()
        mainFrame = Tix.Frame(widget, bd=1, relief=Tix.RAISED)
        mainFrame.pack(side=Tix.TOP, fill=Tix.BOTH, expand=1)
        v = Recorder.Recorder(mainFrame, captureManager)
    except:  
        print 'Something went wrong.'
  