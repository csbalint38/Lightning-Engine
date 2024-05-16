import os

from ..Abstracts.NewProjectView import NewProjectView
from LightningGUI import Observable
from ..Dataclasses.ProjectTemplate import ProjectTemplate
from Utilities.Serializer import Serializer

class NewProjectController(Observable):
    def __init__(self, view: NewProjectView) -> None:
        super().__init__()
        self.view = view
        self._name = "NewProject"
        self._path = f"{os.environ["USERPROFILE"]}\\Documents\\LightningProjects\\"
        self.__templates_path = "ProjectTemplates"

        self.view.create_button_clicked.subscribe(print)
        self.view.name_changed.subscribe(self._set_name)
        self.view.path_changed.subscribe(self._set_path)
        
        self.templates = self._get_template_files()

    @property
    def name(self) -> str:
        return self._name
    
    @name.setter
    def name(self, value: str) -> None:
        if self._name != value:
            self._name = value
            self.notify("name")
            
    @property
    def path(self) -> str:
        return self._path
    
    @path.setter
    def path(self, value: str) -> None:
        if self._path != value:
            self._path = value
            self.notify("path")

    def _set_name(self, value: str) -> None:
        self.name = value
        
    def _set_path(self, value: str) -> None:
        self.path = value
        
    def _get_template_files(self) -> list:
        templates = []
        for root, _, files in os.walk(self.__templates_path):
            for file in files:
                if file == "template.xml":
                    path = os.path.join(root, file)
                    template = Serializer.from_file(ProjectTemplate, path)
                    templates.append(template)
        return templates