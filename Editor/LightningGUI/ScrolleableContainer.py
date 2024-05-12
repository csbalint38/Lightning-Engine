import platform
import tkinter
from .Container import Container
class ScrollableContainer(Container):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        super().__init__(master, **kwargs)
        
        self._canvas = tkinter.Canvas(self, borderwidth=0, background="white")
        self._viewport = tkinter.Frame(self._canvas, background="white")
        self._vsb = tkinter.Scrollbar(self, orient="vertical", command=self._canvas.yview)
        self._canvas.configure(yscrollcommand=self.vsb.set)
        
        self._vsb.pack(side=tkinter.RIGHT, fill=tkinter.Y)
        self._canvas.pack(side=tkinter.LEFT, fill=tkinter.BOTH, expand=True)
        self._canvas_window = self._canvas.create_window((4, 4), window=self._viewport, anchor="nw", tags="self._viewport")
        
        self._viewport.bind("<Configure>", self.on_frame_configure)
        self._canvas.bind("<Configure>", self.on_canvas_configure)
        
        self._viewport.bind("<Enter>", self.on_enter)
        self._viewport.bind("<Leave>", self.on_leave)
        
        self.on_frame_configure(None)
        
    def on_frame_configure(self, _) -> None:
        self._canvas.configure(scrollregion=self._canvas.bbox(tkinter.ALL))
        
    def on_canvas_configure(self, event: tkinter.Event) -> None:
        canvas_width = event.width
        self._canvas.itemconfig(self._canvas_window, width=canvas_width)
        
    def on_mousewheel(self, event: tkinter.Event) -> None:
        if platform.system() == "Windows":
            self._canvas.yview_scroll(int(-1 * (event.delta / 120)), tkinter.UNITS)
        elif platform.system() == "Darwin":
            self._canvas.yview_scroll(int(-1 * event.delta), tkinter.UNITS)
        else:
            if event.num == 4:
                self._canvas.yview_scroll(-1, tkinter.UNITS)
            elif event.num == 5:
                self._canvas.yview_scroll(1, tkinter.UNITS)
                
    def on_enter(self, _) -> None:
        if platform.system() == "Linux":
            self._canvas.bind_all("<Button-4>", self.on_mousewheel)
            self._canvas.bind_all("<Button-5>", self.on_mousewheel)
        else:
            self._canvas.bind_all("<Mousewheel>", self.on_mousewheel)
            
    def on_leave(self, _) -> None:
        if platform.system() == "Linux":
            self._canvas.unbind_all("<Button-4>", self.on_mousewheel)
            self._canvas.unbind_all("<Button-5>", self.on_mousewheel)
        else:
            self._canvas.unbind_all("<Mousewheel>", self.on_mousewheel)