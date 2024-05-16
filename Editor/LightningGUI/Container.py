import tkinter 

class Container(tkinter.Frame):
    def __init__(self, parent: tkinter.Widget, **kwargs) -> None:
        super().__init__(parent, **kwargs)
        
    def config_all(self, **kwargs) -> None:
        self.config(**kwargs)
        for child in self.winfo_children():
            try:
                child.config_all(**kwargs)
            except AttributeError:
                child.configure(**kwargs)