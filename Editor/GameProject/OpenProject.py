import tkinter
import LightningGUI

class OpenProject(LightningGUI.Container):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        super().__init__(master, **kwargs)
        self.master = master
        
        self.draw()
        
    def draw(self) -> None:
        c_info = LightningGUI.Container(self)
        sc_projects = LightningGUI.ScrollableContainer(c_info, width=230, height=280)
        for i in range(100):
            tkinter.Label(sc_projects.viewport, text=f"AAAAAAAAA{i}").pack(anchor='w')
            
        img_scr = LightningGUI.Image(c_info, "./diamond.png", size=(370, 280))
        
        font_button = LightningGUI.Font(size=16)
        btn_open = tkinter.Button(self, text="Open", command=print, font=font_button, padx=15)
        
        c_info.pack(expand=True, anchor='n', pady=(15, 0))
        sc_projects.pack_propagate(0)
        sc_projects.pack(side=tkinter.LEFT, padx=(0, 60))
        img_scr.pack_propagate(0)
        img_scr.pack(side=tkinter.LEFT)
        btn_open.pack(anchor='n', expand=True)