from ..Abstracts.NewProjectView import NewProjectView
from LightningGUI import Observable

class NewProjectController(Observable):
    def __init__(self, view: NewProjectView) -> None:
        super().__init__()
        self.view = view
        self._name = "Meeee"

        self.view.create_button_clicked.subscribe(self.ax)
        
    @property
    def name(self) -> str:
        return self.name
    
    @name.setter
    def name(self, value: str) -> None:
        self._name = value
        self.notify("name")

    def ax(self):
        print('ax')
        self.name = "New name"