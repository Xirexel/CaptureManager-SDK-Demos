import Tix
import GUIPackage as GUI

from CaptureManagerPythonProxy import LogPrintOut

logPrintOut = LogPrintOut.LogPrintOut()

logPrintOut.addPrintOutDestination(0, 'test.txt')

logPrintOut.addPrintOutDestination(1, 'test.txt')

if __name__ == '__main__':
    root = Tix.Tk()
    GUI.runMain(root)
    root.mainloop()