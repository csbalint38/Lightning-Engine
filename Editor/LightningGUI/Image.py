import tkinter 
from PIL import Image as PILImage, ImageTk
from typing import Tuple

class Image(tkinter.Label):
    def __init__(self, master: tkinter.Widget, path: str, size: Tuple[int, int] | int | None = None, **kwargs) -> None:
        self._path = path
        self._size = size
        self._image = PILImage.open(self._path)

        super().__init__(master, **kwargs)
        
        if self._size is not None:
            self._size = self._size if type(self._size) == tuple else (self._size, self._size)
            self._image = self._image.resize(self._size, PILImage.Resampling.LANCZOS)
            self._image = ImageTk.PhotoImage(self._image)
        else: self._image = tkinter.PhotoImage(file=self._path)
        
        self.configure(image=self._image)