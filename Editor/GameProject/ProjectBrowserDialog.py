import tkinter
import LightningGUI
from .OpenProject import OpenProject
from .NewProject import NewProject

class ProjectBrowserDialog(LightningGUI.Window):
    def __init__(self) -> None:
        super().__init__((800, 450), "Project Browser", False, True)
        self._c_open = OpenProject(self)
        self._c_create = NewProject(self)
        
    def setup(self) -> None:
        font_button = LightningGUI.Font(size=24)
        self._c_header = LightningGUI.Container(self,)
        self._c_button = LightningGUI.Container(self._c_header)
    
        self._btn_open_project = LightningGUI.ToggleButton(self._c_button, lambda: self.header_button_toggle(self._btn_open_project), name="open_project", text="Open Project", font=font_button)
        self._btn_new_project = LightningGUI.ToggleButton(self._c_button, lambda: self.header_button_toggle(self._btn_new_project), name="new_project", text="Create Project", font=font_button)
        self._btn_open_project.set_state(True)
        
        self._c_open.setup()
        self._c_create.setup()
        
    def draw(self) -> None:
        self._btn_open_project.pack(side=tkinter.LEFT, padx=30)
        self._btn_new_project.pack(padx=30)
        self._c_button.pack(expand=True, anchor="center", padx=0, pady=0)
        self._c_header.pack(fill=tkinter.BOTH, anchor='n')
        self._c_open.draw()
        self._c_create.draw()
        self._c_open.pack(fill=tkinter.BOTH, anchor='n', expand=True)

        self.update()
     
    def header_button_toggle(self, sender: tkinter.Widget) -> None:
        if sender == self._btn_new_project:
            if self._btn_new_project.get_state():
                self._btn_open_project.set_state(False)
                self._c_open.pack_forget()
                self._c_create.pack(fill=tkinter.BOTH, anchor='n', expand=True)
            self._btn_new_project.set_state(True)
        else:
            if self._btn_open_project.get_state():
                self._btn_new_project.set_state(False)
                self._c_create.pack_forget()
                self._c_open.pack(fill=tkinter.BOTH, anchor='n', expand=True)
            self._btn_open_project.set_state(True)