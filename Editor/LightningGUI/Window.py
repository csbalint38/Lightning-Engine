import tkinter
from typing import Tuple

class Window(tkinter.Tk):
    def __init__(self, size: Tuple[int, int], title: str, resizable: bool  = True, centered: bool = False) -> None:
        super().__init__()
        
        self._size = size
        self._title = title
        self._resizable = resizable
        
        self.title(self._title)
        self.resizable(self._resizable, self._resizable)
        
        self.center_window() if centered else self.geometry(f"{self._size[0]}x{self._size[1]}")
        
    def center_window(self) -> None:
        width = self.winfo_screenwidth()
        height = self.winfo_screenheight()
        
        x = (width - self._size[0]) // 2
        y = (height - self._size[1]) // 2
        
        self.geometry(f"{self._size[0]}x{self._size[1]}+{x}+{y}")
        
    def update(self) -> None:
        self.mainloop()