import datetime
from Utilities.Serializable import Serializable

class Project(Serializable):
    data_members = [
        { "field": "_name", "name": "Name", "type": str },
        { "field": "_path", "name": "Path", "type": str },
        { "field": "_created_at", "name": "CreatedAt", "type": int},
        { "field": "_updated_at", "name": "UpdatedAt", "type": int},
    ]
    
    def __init__(self, name: str = "", path: str = "", created_at: int = 0, updated_at: int = 0) -> None:
        self._name = name
        self._path = path
        self._created_at = created_at
        self._updated_at = updated_at
        self._icon_path = ""
        self._screenshot_path = ""
        
    @property
    def name(self) -> str:
        return self._name
    
    @name.setter
    def name(self, value: str) -> None:
        if self._name != value:
            self._name = value
            
    @property
    def path(self) -> str:
        return self._path
    
    @path.setter
    def path(self, value: str) -> None:
        if self._path != value:
            self._path = value

    @property
    def created_at(self) -> datetime.datetime:
        return datetime.datetime.fromtimestamp(self._created_at)
    
    @created_at.setter
    def created_at(self, value: datetime.datetime) -> None:
        if self._created_at != value.timestamp():
            self._created_at = int(value.timestamp())
    
    @property
    def updated_at(self) -> datetime.datetime:
        return datetime.datetime.fromtimestamp(self._updated_at)
    
    @updated_at.setter
    def updated_at(self, value: datetime.datetime) -> None:
        if self._updated_at != value.timestamp():
            self._updated_at = int(value.timestamp())
            
    @property
    def icon_path(self) -> str:
        return self._icon_path
    
    @icon_path.setter
    def icon_path(self, value: str) -> None:
        if self._icon_path != value:
            self._icon_path = value
            
    @property
    def screenshot_path(self) -> str:
        return self._screenshot_path
    
    @screenshot_path.setter
    def screenshot_path(self, value: str) -> None:
        if self._screenshot_path != value:
            self._screenshot_path = value
            
    def __str__(self) -> str:
        return f"name: {self._name} \npath: {self._path} \ncreated: {self._created_at} \nupdated: {self._updated_at} \nscreenshot: {self._screenshot_path} \nicon: {self._icon_path}"
            
class ProjectsData(Serializable):
    data_members = [
        { "field": "_projects", "name": "Projects", "type": list, "list_type": Project }
    ]

    def __init__(self, projects: list = []) -> None:
        self._projects = projects
        
    def __str__(self) -> str:
        return f"{''.join(str(project) for project in self._projects)}"