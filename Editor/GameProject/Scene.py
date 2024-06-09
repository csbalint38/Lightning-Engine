from Utilities.Serializable import Serializable

class Scene(Serializable):
    data_members = [
        { "field": "_name", "name": "Name", "type": str },
        { "field": "_is_active", "name": "IsActive", "type": bool }
    ]

    def __init__(self, name: str = "", is_active: bool = False) -> None:
        self._name = name
        self._is_active = is_active
        
    def __str__(self) -> str:
        return f"name: {self._name}, \nactive: {self._is_active}"