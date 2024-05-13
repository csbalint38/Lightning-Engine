import tkinter
import LightningGUI

class NewProject(LightningGUI.Container):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        super().__init__(master, **kwargs)
        self.master = master
        
        self.draw()
        
    def draw(self) -> None:
        c_info = LightningGUI.Container(self)
        sc_projects = LightningGUI.ScrollableContainer(c_info, width=250, height=200)
        for i in range(100):
            tkinter.Label(sc_projects.viewport, text=f"AAAAAAAAA{i}").pack(anchor='w')
            
        img_scr = LightningGUI.Image(c_info, "./diamond.png", size=(320, 200))
        
        c_input = LightningGUI.Container(self, bg="red")
        c_name = LightningGUI.Container(c_input)
        c_path = LightningGUI.Container(c_input)
        label_name = tkinter.Label(c_name, text="Name: ", width=5, anchor='w')
        entry_name = tkinter.Entry(c_name)
        label_path = tkinter.Label(c_path, text="Path: ", width=5, anchor='w')
        entry_path = tkinter.Entry(c_path)
        btn_path = tkinter.Button(c_path, text="Browse", command=print)

        font_button = LightningGUI.Font(size=16)
        btn_open = tkinter.Button(self, text="Create", command=print, font=font_button, padx=15)
        
        c_info.pack(expand=True, anchor='n', pady=(15, 0))
        sc_projects.pack_propagate(0)
        sc_projects.pack(side=tkinter.LEFT, padx=(0, 60))
        img_scr.pack_propagate(0)
        img_scr.pack(side=tkinter.LEFT)
        
        c_input.pack(expand=True, fill=tkinter.X, padx=30)
        c_name.pack(fill=tkinter.X, expand=True)
        c_path.pack(fill=tkinter.X, expand=True)
        label_name.pack(side=tkinter.LEFT)
        label_name.pack_propagate(0)
        entry_name.pack(fill=tkinter.X, expand=True)
        label_path.pack(side=tkinter.LEFT)
        label_path.pack_propagate(0)
        entry_path.pack(side=tkinter.LEFT, fill=tkinter.X, expand=True)
        btn_path.pack()
        
        btn_open.pack(anchor='n', expand=True)
