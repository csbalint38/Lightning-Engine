import tkinter

from common.common import SAMPLE_TYPES
from modules.canvas import Canvas
from modules.sampler import Sampler

class Controls(tkinter.Frame):
    def __init__(self, root: tkinter.Tk, sampler: Sampler, canvas: Canvas):
        super().__init__()
        
        self.root = root
        self.sampler = sampler
        self.canvas = canvas
        
        frame1 = tkinter.Frame(self.root)
        frame1.pack(anchor="w")
        frame2 = tkinter.Frame(self.root)
        frame2.pack(anchor="w")
        frame3 = tkinter.Frame(self.root)
        frame3.pack(anchor="w")
        
        self.button = tkinter.Button(frame1, text="Add", command=self._add_samples)
        self.button.pack(pady=10, padx=10, side="left")
        
        self.button = tkinter.Button(frame1, text="Clear", command=self._clear)
        self.button.pack(pady=10, padx=10, side="left")
        
        self.sample_type = tkinter.StringVar()
        self.sample_type.set(SAMPLE_TYPES[0])

        self.roughtness = tkinter.IntVar()
        
        tkinter.Radiobutton(frame1, text="Discrete", variable=self.sample_type, value=SAMPLE_TYPES[0]).pack(side="left")
        tkinter.Radiobutton(frame1, text="Uniform", variable=self.sample_type, value=SAMPLE_TYPES[1]).pack(side="left")
        tkinter.Radiobutton(frame1, text="Importance Sampling", variable=self.sample_type, value=SAMPLE_TYPES[2]).pack(side="left")
        tkinter.Radiobutton(frame1, text="Importance Sampling Specular", variable=self.sample_type, value=SAMPLE_TYPES[3]).pack(side="left")

        tkinter.Scale(frame1, orient="horizontal", variable=self.roughtness).pack(side="left")
        
        self.sample_count = tkinter.IntVar()
        
        tkinter.Label(frame3, text="Sample Count: ").pack(side="left")
        tkinter.Label(frame3, textvariable=self.sample_count).pack(side="left")
        
    def _add_samples(self):
        self.sampler.sample(self.sample_type.get(), self.roughtness.get() / 100)
        self.canvas.update()
        self.sample_count.set(self.sampler.count)
        
    def _clear(self):
        self.sampler.clear()
        self.canvas.clear()
        self.sample_count.set(0)