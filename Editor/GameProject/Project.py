from GameProject.Scene import Scene
from Utilities.Serializable import Serializable

class Project(Serializable):
    data_members = [
        { "field": "_name", "name": "Name", "type": str },
        { "field": "_path", "name": "Path", "type": str },
        { "field": "_scenes", "name": "Scenes", "type": Scene },
    ]

    def __init__(self, name: str = "", path: str = "", scenes: list = []):
        self._name = name
        self._path = path
        self._scenes = scenes
            
        self.full_path = self._path + self._name
        self._scenes.append(Scene("Default Scene"))