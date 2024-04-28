import ctypes

from Common.Primitives import *

class TransformComponent(ctypes.Structure):
    """Descriptor class to create a transform component in the Engine"""
    
    _fields_ = [
        ("position", f32 * 3),
        ("position", f32 * 3),
        ("position", f32 * 3),
    ]