import os
from typing import Self

from GameProject.Scene import Scene
from Utilities.Serializer import Serializer
from Utilities.Serializable import Serializable

class Project(Serializable):
    data_members = [
        { "field": "_name", "name": "Name", "type": str },
        { "field": "_path", "name": "Path", "type": str },
        { "field": "_scenes", "name": "Scenes", "type": list, "list_type": Scene},
    ]

    def __init__(self, name: str = "", path: str = "", scenes: list = []):
        self._name = name
        self._path = path
        self._scenes = scenes
        self._active_scene = None
            
        self.full_path = self._path + self._name
        
        if len(self._scenes) == 0:
            self._scenes.append(Scene("Default Scene", True))
            
        self._active_scene = next((scene for scene in self._scenes if scene._is_active), None)
        
    @staticmethod
    def load(file: str) -> Self:
        if not os.path.exists(file):
            return
        return Serializer.from_file(Project, file)
    
    def save(project: Self) -> None:
        Serializer.to_file(project, project.full_path)
        
    def __str__(self) -> str:
        scenes = ''.join('{'+str(scene)+'}' for scene in self._scenes)
        return f"name: {self._name}\npath: {self._path}\nscenes: \n[{scenes}]"