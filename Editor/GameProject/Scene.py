from Utilities.Serializable import Serializable

class Scene(Serializable):
    data_members = [
        { "field": "_name", "name": "Name", "type": str }
    ]

    def __init__(self, name: str = ""):
        self._name = name