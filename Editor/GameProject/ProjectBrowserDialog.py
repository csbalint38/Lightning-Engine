import tkinter
import LightningGUI
from .OpenProject import OpenProject
from .NewProject import NewProject

class ProjectBrowserDialog(LightningGUI.Window):
    def __init__(self) -> None:
        super().__init__((800, 450), "Project Browser", False, True)
        
    def draw_header(self) -> None:
        font_button = LightningGUI.Font(size=24)
        c_header = LightningGUI.Container(self,)
        c_button = LightningGUI.Container(c_header)
        self._btn_open_project = LightningGUI.ToggleButton(c_button, lambda: self.header_button_toggle(self._btn_open_project), name="open_project", text="Open Project", font=font_button)
        self._btn_new_project = LightningGUI.ToggleButton(c_button, lambda: self.header_button_toggle(self._btn_new_project), name="new_project", text="Create Project", font=font_button)
        self._btn_open_project.set_state(True)

        c_header.pack(fill=tkinter.BOTH, anchor='n')
        c_button.pack(expand=True, anchor="center", padx=0, pady=0)
        self._btn_open_project.pack(side=tkinter.LEFT, padx=30)
        self._btn_new_project.pack(padx=30)
        
    def draw_open(self) -> None:
        self._c_open = OpenProject(self)
        self._c_open.pack(fill=tkinter.BOTH, anchor='n', expand=True)
        
    def draw_create(self) -> None:
        self._c_create = NewProject(self)

    def draw(self) -> None:
        self.draw_header()
        self.draw_open()
        self.draw_create()

        self.update()
     
    def header_button_toggle(self, sender: tkinter.Widget) -> None:
        print(self._btn_open_project.get_state())
        print(self._btn_new_project.get_state())
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