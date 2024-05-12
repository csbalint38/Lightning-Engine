import tkinter 

class Container(tkinter.Frame):
    def __init__(self, parent: tkinter.Widget, **kwargs) -> None:
        super().__init__(parent, **kwargs)