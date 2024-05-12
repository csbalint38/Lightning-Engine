import tkinter
from typing import Callable

class ToggleButton(tkinter.Button):
    def __init__(self, master: tkinter.Widget, command: Callable, **kwargs) -> None:
        super().__init__(master, command = self.toggle, **kwargs)
        
        self._command = command
        self._state = False
        
    def toggle(self) -> None:
        self._state = not self._state
        self._command()
        
    def get_state(self) -> bool:
        return self._state
    
    def set_state(self, state: bool) -> None:
        self._state = state