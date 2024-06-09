import tkinter 
from typing import Callable

class Container(tkinter.Frame):
    def __init__(self, parent: tkinter.Widget, **kwargs) -> None:
        super().__init__(parent, **kwargs)
        
    def config_all(self, **kwargs) -> None:
        self.config(**kwargs)
        for child in self.winfo_children():
            if isinstance(child, Container):
                child.config_all(**kwargs)
            else:
                child.configure(**kwargs)
                
    def bind_child(self, event: str, callback: Callable) -> None:
        self.bind(event, callback)
        for child in self.winfo_children():
            if isinstance(child, Container):
                child.bind_child(event, callback)
            else:
                child.bind(event, callback)