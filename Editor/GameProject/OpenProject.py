import tkinter
import LightningGUI

from .Abstracts.OpenProjectView import OpenProjectView
from .Controllers.OpenProjectController import OpenProjectController

class OpenProject(LightningGUI.Container, OpenProjectView):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        LightningGUI.Container.__init__(self, master, **kwargs)
        OpenProjectView.__init__(self)
        
        self.controller = OpenProjectController(self)
        self.controller.add_observer(self)
        self.master = master
        
        self.open_button_clicked.subscribe(self.close_dialog)
        
    def setup(self) -> None:
        font_button = LightningGUI.Font(size=16)
        
        self._c_info = LightningGUI.Container(self)
        self._lb_projects = LightningGUI.ListBox(self._c_info, width=250, height=200)
        self._lb_projects.selection_changed.subscribe(lambda: self.selection_changed(self._lb_projects.selected_indicies[0] if len(self._lb_projects.selected_indicies) > 0 else 0))
        self.selection_changed.subscribe(self.change_screenshot)
        self._c_image = LightningGUI.Container(self._c_info)
        self._btn_open = tkinter.Button(self, text="Open", command=self.open_button_clicked, font=font_button, padx=15)

        if len(self.controller.projects) > 0:
            for project in self.controller.projects:
                self._lb_projects.add_widget(self._create_listbox_item(project.name, project.icon_path))
                self._lb_projects.toggle_selection(self._lb_projects.items[0][1])
        
    def draw(self) -> None:
        self._c_info.pack(expand=True, anchor='n', pady=(15, 0))
        self._lb_projects.pack_propagate(0)
        self._lb_projects.pack(side=tkinter.LEFT, padx=(0, 60))
        self._c_image.pack(expand=True)
        self._btn_open.pack(anchor='n', expand=True)
        
    def _create_listbox_item(self, text: str, icon_path: str) -> tkinter.Widget:
        listbox_item = LightningGUI.Container(self._lb_projects.viewport)
        label = tkinter.Label(listbox_item, text=text)
        img = LightningGUI.Image(listbox_item, icon_path)
        listbox_item.bind_all("<Double-1>", lambda _: self.open_button_clicked())
        img.pack()
        label.pack()
        
        return listbox_item
        
    def property_changed(self, property_name: str) -> None:
        pass
    
    def change_screenshot(self, _) -> None:
        if len(self._c_image.winfo_children()) > 0:
            self._c_image.winfo_children()[0].destroy()
        self._img_scs=LightningGUI.Image(self._c_image, self.controller.get_screenshot_path(), size=(320, 200))
        self._img_scs.pack_propagate(0)
        self._img_scs.pack(side=tkinter.LEFT)
        
    def close_dialog(self):
        self.master.result = self.controller.construct_project()
        self.master.destroy()