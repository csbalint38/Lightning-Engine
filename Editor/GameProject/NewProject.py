import tkinter
import LightningGUI
from .Abstracts.NewProjectView import NewProjectView
from .Controllers.NewProjectController import NewProjectController

class NewProject(LightningGUI.Container, NewProjectView):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        LightningGUI.Container.__init__(self, master, **kwargs)
        NewProjectView.__init__(self)
        
        self.controller = NewProjectController(self)
        self.controller.add_observer(self)
        self.master = master
        
    def setup(self) -> None:
        font_button = LightningGUI.Font(size=16)
        
        self._c_info = LightningGUI.Container(self)
        self._sc_projects = LightningGUI.ScrollableContainer(self._c_info, width=250, height=200)
        
        for i in range(100):
            tkinter.Label(self._sc_projects.viewport, text=f"AAAAAAAAA{i}").pack(anchor='w')
            
        self._img_scr = LightningGUI.Image(self._c_info, "./diamond.png", size=(320, 200))
        
        self._c_input = LightningGUI.Container(self, bg="red")
        self._c_name = LightningGUI.Container(self._c_input)
        self._c_path = LightningGUI.Container(self._c_input)
        self._label_name = tkinter.Label(self._c_name, text="Name: ", width=5, anchor='w')
        self._entry_name = tkinter.Entry(self._c_name)
        self._label_path = tkinter.Label(self._c_path, text="Path: ", width=5, anchor='w')
        self._entry_path = tkinter.Entry(self._c_path)
        self._btn_path = tkinter.Button(self._c_path, text="Browse", command=print)

        self._btn_create = tkinter.Button(self, text="Create", command=self.create_button_clicked, font=font_button, padx=15)
        
    def draw(self) -> None:
        self._c_info.pack(expand=True, anchor='n', pady=(15, 0))
        self._sc_projects.pack_propagate(0)
        self._sc_projects.pack(side=tkinter.LEFT, padx=(0, 60))
        self._img_scr.pack_propagate(0)
        self._img_scr.pack(side=tkinter.LEFT)
        
        self._c_input.pack(expand=True, fill=tkinter.X, padx=30)
        self._c_name.pack(fill=tkinter.X, expand=True)
        self._c_path.pack(fill=tkinter.X, expand=True)
        self._label_name.pack(side=tkinter.LEFT)
        self._label_name.pack_propagate(0)
        self._entry_name.pack(fill=tkinter.X, expand=True)
        self._label_path.pack(side=tkinter.LEFT)
        self._label_path.pack_propagate(0)
        self._entry_path.pack(side=tkinter.LEFT, fill=tkinter.X, expand=True)
        self._btn_path.pack()
        
        self._btn_create.pack(anchor='n', expand=True)
        
    def property_changed(self, property_name: str) -> None:
        print(property_name)
