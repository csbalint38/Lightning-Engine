from Utilities.Serializable import Serializable

class ProjectTemplate(Serializable):
    data_members = [
        { "field": "_project_type", "name": "ProjectType", "type": str },
        { "field": "_project_file", "name": "ProjectFile", "type": str},
        { "field": "_folders", "name": "Folders", "type": list, "list_type": str}
    ]
    
    def __init__(self, project_type: str = "", project_file: str = "", folders: list = []):
        super().__init__()
        self._project_type = project_type
        self._project_file = project_file
        self._folders = folders
        self._testvar = 1
    
    @property
    def project_type(self) -> str:
        return self._project_type
    
    @project_type.setter
    def project_type(self, value: str) -> None:
        if self._project_type != value:
            self._project_type = value
            
    @property
    def project_file(self) -> str:
        return self._project_file
    
    @project_file.setter
    def project_file(self, value: str) -> None:
        if self._project_file != value:
            self._project_file = value