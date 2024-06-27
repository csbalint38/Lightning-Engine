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
        
        self._sv_name = tkinter.StringVar()
        self._sv_path = tkinter.StringVar()
        
        self.project_created.subscribe(lambda: self.master._c_open.open_button_clicked())
        
    def setup(self) -> None:
        font_button = LightningGUI.Font(size=16)
        
        self._sv_name.set(self.controller.name)
        self._sv_name.trace_add("write", lambda *_: self.name_changed(self._sv_name.get()))
        self._sv_path.set(self.controller.path)
        self._sv_path.trace_add("write", lambda *_: self.path_changed(self._sv_path.get()))

        self._c_info = LightningGUI.Container(self)
        self._lb_projects = LightningGUI.ListBox(self._c_info, width=250, height=200)
        self._lb_projects.selection_changed.subscribe(lambda: self.selection_changed(self._lb_projects.selected_indicies[0] if len(self._lb_projects.selected_indicies) > 0 else 0))
        self.selection_changed.subscribe(self.change_screenshot)
        
        for template in self.controller.templates:
            self._lb_projects.add_item(template.project_type, self.controller.get_icon_path(template))
        
        self._c_image = LightningGUI.Container(self._c_info)
        self._lb_projects.toggle_selection(self._lb_projects.items[0][1])
        
        self._c_input = LightningGUI.Container(self, bg="red")
        self._c_name = LightningGUI.Container(self._c_input)
        self._c_path = LightningGUI.Container(self._c_input)
        self._label_name = tkinter.Label(self._c_name, text="Name: ", width=5, anchor='w')
        self._entry_name = tkinter.Entry(self._c_name, textvariable=self._sv_name)
        self._label_path = tkinter.Label(self._c_path, text="Path: ", width=5, anchor='w')
        self._entry_path = tkinter.Entry(self._c_path, textvariable=self._sv_path)
        self._btn_path = tkinter.Button(self._c_path, text="Browse", command=print)
        self._label_error_message = tkinter.Label(self, text="", fg="red")

        self._btn_create = tkinter.Button(self, text="Create", command=self.create_button_clicked, font=font_button, padx=15)
        
        self.controller._validate_project_path()
        
    def draw(self) -> None:
        self._c_info.pack(expand=True, anchor='n', pady=(15, 0))
        self._lb_projects.pack_propagate(0)
        self._lb_projects.pack(side=tkinter.LEFT, padx=(0, 60))
        self._c_image.pack(expand=True)
        
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
        self._label_error_message.pack()
        
        self._btn_create.pack(anchor='n', expand=True)
        
    def property_changed(self, property_name: str) -> None:
        if property_name == "name" and self._sv_name.get() != self.controller.name:
            self._sv_name.set(self.controller.name)
        elif property_name == "path" and self._sv_path.get() != self.controller.path:
            self._sv_path.set(self.controller.path)
        elif property_name == "is_path_valid":
            self._btn_create.config(state = tkinter.DISABLED if not self.controller.is_path_valid else tkinter.NORMAL)
        elif property_name == "error_message":
            self._label_error_message.config(text=self.controller.error_message)
            
    def change_screenshot(self, index: int) -> None:
        if len(self._c_image.winfo_children()) > 0:
            self._c_image.winfo_children()[0].destroy()
        self._img_scs=LightningGUI.Image(self._c_image, self.controller.get_screenshot_path(self.controller.templates[index]), size=(320, 200))
        self._img_scs.pack_propagate(0)
        self._img_scs.pack(side=tkinter.LEFT)