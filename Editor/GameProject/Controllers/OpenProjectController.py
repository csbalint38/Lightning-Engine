import os
import datetime

from ..Abstracts.OpenProjectView import OpenProjectView
from LightningGUI import Observable
from Utilities.Serializer import Serializer
from ..ProjectData import ProjectsData
from ..Project import Project

class OpenProjectController(Observable):
    def __init__(self, view: OpenProjectView) -> None:
        super().__init__()
        self.view = view
        
        self._app_data_path = os.getenv("APPDATA") + "\\LightningEditor\\"
        self._projects_path = self._app_data_path + "ProjectData.xml"
        self.projects = []
        self.selected_project_index = 0

        self.view.open_button_clicked.subscribe(self.open_project)
        self.view.selection_changed.subscribe(self._change_project)
        
        self._read_project_data()
        
    def open_project(self):
        self._read_project_data()
        
        project = self.projects[self.selected_project_index]
        project.updated_at = datetime.datetime.now()

        self._write_project_data()
        self.view.project_opened()
        
    def _read_project_data(self) -> None:
        if not os.path.exists(self._app_data_path):
            os.mkdir(self._app_data_path)

        if os.path.exists(self._projects_path):
            projects = Serializer.from_file(ProjectsData, self._projects_path)
            projects = sorted(projects._projects, key = lambda project: project.updated_at, reverse=True)
        else:
            projects = ProjectsData()._projects
        
        self.projects.clear()
        for project in projects:
            if os.path.exists(project.path+project.name):
                project.icon_path = f"{project.path}{project.name}\\.Lightning\\icon.png"
                project.screenshot_path = f"{project.path}{project.name}\\.Lightning\\screenshot.png"
                self.projects.append(project)
                
        self._write_project_data()
        
    def _write_project_data(self) -> None:
        Serializer.to_file(ProjectsData(self.projects), self._projects_path)
        
    def get_screenshot_path(self) -> str:
        return self.projects[self.selected_project_index].screenshot_path
    
    def construct_project(self) -> Project:
        selected = self.projects[self.selected_project_index]
        project = Project(selected.name, f"{selected.path}\\{selected.name}\\")
        return project
    
    def _change_project(self, new: int) -> None:
        self.selected_project_index = new