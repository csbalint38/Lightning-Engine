import os
import re
import shutil
import subprocess

from GameProject.Project import Project
from ..Abstracts.NewProjectView import NewProjectView
from LightningGUI import Observable
from ..ProjectTemplate import ProjectTemplate
from Utilities.Serializer import Serializer

class NewProjectController(Observable):
    def __init__(self, view: NewProjectView) -> None:
        super().__init__()
        self.view = view
        self._name = "NewProject"
        self._path = f"{os.environ["USERPROFILE"]}\\Documents\\LightningProjects\\"
        self._is_path_valid = True
        self._error_message = ""

        self.templates_path = "ProjectTemplates/"

        self.view.create_button_clicked.subscribe(self.create_project)
        self.view.name_changed.subscribe(self._set_name)
        self.view.path_changed.subscribe(self._set_path)
        self.view.name_changed.subscribe(self._validate_project_path)
        self.view.path_changed.subscribe(self._validate_project_path)
        self.view.selection_changed.subscribe(self._change_template)
        
        self.templates = self._get_template_files()
        self.selected_template = 0

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
            
    @property
    def is_path_valid(self) -> bool:
        return self._is_path_valid
    
    @is_path_valid.setter
    def is_path_valid(self, value: bool) -> None:
        if self._is_path_valid != value:
            self._is_path_valid = value
            self.notify("is_path_valid")
            
    @property
    def error_message(self) -> str:
        return self._error_message
    
    @error_message.setter
    def error_message(self, value: str) -> None:
        if self._error_message != value:
            self._error_message = value
            self.notify("error_message")

    def _set_name(self, value: str) -> None:
        self.name = value
        
    def _set_path(self, value: str) -> None:
        self.path = value
        
    def _get_template_files(self) -> list:
        templates = []
        for root, _, files in os.walk(self.templates_path):
            for file in files:
                if file == "template.xml":
                    path = os.path.join(root, file)
                    template = Serializer.from_file(ProjectTemplate, path)
                    templates.append(template)
        return templates
    
    def _change_template(self, new: int) -> None:
        self.selected_template = new
    
    def _validate_project_path(self, _: str = "") -> None:
        if not self._path.endswith(os.path.sep):
            self._path += os.path.sep
        full_path = f"{self.path}{self._name}\\"
        
        if(self._name.strip() == ""):
            self.is_path_valid = False
            self.error_message = "Project name can't be empty."
            return
        
        valid_windows_pattern = re.compile(r'^(?!^(?:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\..+)?$)[^\\/:*?"<>|\r\n]{0,254}[^\\/:*?"<>|\r\n. ]$')
        if not valid_windows_pattern.match(self.name):
            self.is_path_valid = False
            self.error_message = "Invalid character(s) in file name."
            return
        
        if(full_path.strip() == ""):
            self.is_path_valid = False
            self.error_message = "Invalid folder."
            return

        valid_windows_pattern = re.compile(r'^(?:[a-zA-Z]:\\|\\)(?:[^\\/:*?"<>|\r\n]+\\)*(?:[^\\/:*?"<>|\r\n]*[^\\/:*?"<>|\r\n. ])?$')
        if not valid_windows_pattern.match(full_path):
            self.is_path_valid = False
            self.error_message = "Invalid character(s) in path name."
            return
        
        if not os.path.isdir(self._path):
            self.is_path_valid = False
            self.error_message = "Folder doesn't exist"
            return 
        
        if not os.path.isdir(full_path):
            os.mkdir(full_path)
        
        if len(os.listdir(full_path)) > 0:
            self.is_path_valid = False
            self.error_message = "Folder alredy exists and its not empty."
            return
        
        self.error_message = ""
        self.is_path_valid = True
        
    def create_project(self) -> None:
        self._validate_project_path()
        if not self.is_path_valid:
            return
        
        if not self._path.endswith(os.path.sep):
            self._path += os.path.sep
        full_path = f"{self.path}{self._name}\\"
        
        if not os.path.isdir(full_path):
            os.mkdir(full_path)
            
        for folder in self.templates[self.selected_template]._folders:
            os.mkdir(full_path+folder)
        subprocess.run(["attrib", "+h", f"{full_path}.Lightning"], check=True)
        shutil.copy(self.get_icon_path(self.templates[self.selected_template]), full_path)
        shutil.copy(self.get_screenshot_path(self.templates[self.selected_template]), full_path)
        shutil.copy(self._get_project_file_path(self.templates[self.selected_template]), full_path)
        
        with open(f"{full_path}project.lightning") as file:
            text = file.read()
            
        text = text.replace("{0}", self.name)
        text = text.replace("{1}", self.path)
        
        new_file = open(f"{full_path}project.lightning", 'w')
        new_file.write(text)
        new_file.close()
       
        os.rename(f"{full_path}project.lightning", f"{full_path}{self.name}.lightning")

        self.view.project_created()
        
    def _get_project_file_path(self, template: ProjectTemplate) -> str:
        return self.templates_path + template.project_type + '/' + template.project_file
            
    def get_icon_path(self, template: ProjectTemplate) -> str:
        return self.templates_path + template.project_type + "/icon.png"
    
    def get_screenshot_path(self, template: ProjectTemplate) -> str:
         return self.templates_path + template.project_type + "/screenshot.png"