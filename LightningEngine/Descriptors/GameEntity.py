import ctypes

import TransformComponent

class GameEntity(ctypes.Structure):
    """Descriptor class to create a game entity in the Engine"""
    
    _fields_ = [("transform", TransformComponent)]
    