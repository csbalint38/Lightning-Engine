import tkinter

from modules.canvas import Canvas
from modules.controls import Controls
from modules.sampler import Sampler

class App:
    def __init__(self, root: tkinter.Tk) -> None:
        self.root = root
        self.root.title("Sampling Visualization")
        self.root.geometry("1280x720")
        
        self.sampler = Sampler()
        
        canvas = Canvas(self.root, self.sampler)
        controls = Controls(self.root, self.sampler, canvas)
        
        controls.pack()
        canvas.pack()
        
        
