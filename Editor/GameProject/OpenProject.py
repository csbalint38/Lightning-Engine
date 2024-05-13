import tkinter
import LightningGUI
from LightningGUI import Event

class OpenProject(LightningGUI.Container):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        super().__init__(master, **kwargs)
        self.master = master

        self.open_button_clicked = Event("open_button_clicked")
        
    def setup(self) -> None:
        font_button = LightningGUI.Font(size=16)
        
        self._c_info = LightningGUI.Container(self)
        self._sc_projects = LightningGUI.ScrollableContainer(self._c_info, width=230, height=280)
        
        # TEMP
        for i in range(100):
            tkinter.Label(self._sc_projects.viewport, text=f"AAAAAAAAA{i}").pack(anchor='w')
        
        self._img_scr = LightningGUI.Image(self._c_info, "./diamond.png", size=(370, 280))
        self._btn_open = tkinter.Button(self, text="Open", command=print, font=font_button, padx=15)
        
    def draw(self) -> None:
        self._c_info.pack(expand=True, anchor='n', pady=(15, 0))
        self._sc_projects.pack_propagate(0)
        self._sc_projects.pack(side=tkinter.LEFT, padx=(0, 60))
        self._img_scr.pack_propagate(0)
        self._img_scr.pack(side=tkinter.LEFT)
        self._btn_open.pack(anchor='n', expand=True)