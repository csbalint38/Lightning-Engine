import tkinter
import GUI

class ProjectBrowserDialog(GUI.Window):
    def __init__(self) -> None:
        super().__init__((800, 450), "Lightning Engine", False, True)
        
    def draw(self) -> None:
        _font_button = GUI.Font(size=30)
        c_header = GUI.Container(self, bg="red")
        c_button = GUI.Container(c_header, bg="green")
        self._btn_open_project = GUI.ToggleButton(c_button, lambda: self.header_button_toggle(self._btn_open_project), name="open_project", text="Open Project", font=font_button)
        self._btn_new_project =  GUI.ToggleButton(c_button, lambda: self.header_button_toggle(self._btn_new_project), name="new_project", text="Create Project", font=font_button)
        sc_scrolled = GUI.ScrollableContainer(self)
        for i in range(100):
            sc_scrolled.insert("insert", f"aaaaaaaaaa{i}\n")
        
        c_header.pack(fill=tkinter.X, anchor="n", expand=True)
        c_button.pack(expand=True, anchor="center")
        self._btn_open_project.pack(side=tkinter.LEFT, padx=30)
        self._btn_new_project.pack(side=tkinter.LEFT, padx=30)
        sc_scrolled.pack()
        
        self.update()
     
    def header_button_toggle(self, sender: tkinter.Widget) -> None:
        if sender == self._btn_open_project:
            if self._btn_new_project.get_state():
                self._btn_new_project.set_state(False)
                #TODO: set open project
            self._btn_open_project.set_state(True)
        else:
            if self._btn_open_project.get_state():