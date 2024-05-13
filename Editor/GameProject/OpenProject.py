import tkinter
import LightningGUI

class OpenProject(LightningGUI.Container):
    def __init__(self, master: tkinter.Widget, **kwargs) -> None:
        super().__init__(master, **kwargs)
        
    def draw(self) -> None:
        sc_projects = LightningGUI.ScrollableContainer(self)
        for i in range(100):
            tkinter.Label(sc_projects.viewport, text=f"AAAAAAAAA{i}").pack()
        sc_projects.pack()
        self.pack()