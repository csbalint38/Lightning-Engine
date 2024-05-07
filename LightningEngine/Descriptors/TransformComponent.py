import ctypes

from Common.Primitives import *
from Utilities.Vector3 import Vector3

class TransformComponent(ctypes.Structure):
    """Descriptor class to create a transform component in the Engine"""
    
    _fields_ = [
        ("position", f32 * 3),
        ("rotation", f32 * 3),
        ("scale", f32 * 3),
    ]
    
    def __init__(self, pos:  Vector3 = Vector3(0, 0, 0), rot: Vector3 = Vector3(0, 0, 0), scl: Vector3 = Vector3(1, 1, 1)) -> None:
        self.position = (f32 * 3)(*pos)
        self.rotation = (f32 * 3)(*rot)
        self.scale = (f32 * 3)(*scl)
        
    def __str__(self) -> str:
        return f"Position: {(self.position[0], self.position[1], self.position[2])} \nRotation: {(self.rotation[0], self.rotation[1], self.rotation[2])} \nScale: {(self.scale[0], self.scale[1], self.scale[2])}"