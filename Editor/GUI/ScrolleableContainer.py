import tkinter
from tkinter.scrolledtext import ScrolledText

class ScrollableContainer(ScrolledText):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        super().__init__(master, **kwargs)