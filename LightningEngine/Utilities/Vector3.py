import math
from typing import TypeVar

T = TypeVar('T', bound="Vector3")

class Vector3:
    """Class to represent a 3 dimensional vector"""

    def __init__(self, x: int = 0, y: int = 0, z:int = 0) -> None:
        self.x = x
        self.y = y
        self.z = z
        
    def __repr__(self) -> str:
        return f"Vector({self.x}, {self.y}, {self.z})"
    
    def __add__(self, other: T) -> T:
        """
        Adds two 3D vectors
        other -- other vector to add to this vector (type: Vector3)
        Returns a new Vector3
        """
 
        return Vector3(self.x + other.x, self.y + other.y, self.z + other.z)
    
    def __sub__(self, other: T) -> T:
        """
        Substract a 3D vector from this vector
        other -- other vector to substract from this vector (type: Vector3)
        Returns a new Vector3
        """
        
        return Vector3(self.x - other.x, self.y - other.y, self.z - other.z)
    
    def __mul__(self, scalar: int | float) -> T:
        """
        Multiplies a 3D vector with a scalar
        scalar -- scalar to multiply this vector with
        Returns a new Vector3
        """
        
        return Vector3(self.x * scalar, self.y * scalar, self.z * scalar)
    
    def __rmul__(self):
        raise ValueError()
    
    def __eq__(self, other: T) -> bool:
        """
        Compares this vector to other 3D vector
        other -- other vector to compare with
        Returns true if all the coordinates (x, y, z) are equal, othervise returns false
        """
        
        return self.x == other.x and self.y == other.y and self.z == other.z
    
    def __iter__(self):
        """
        Method to make the Vector3 class iterable
        """
        yield self.x
        yield self.y
        yield self.z
        
    
    def magnitude(self) -> float:
        """
        Get the magnitude of this 3D vector
        Returns a float
        """
        return math.sqrt(self.x ** 2 + self.y ** 2 + self.z ** 2)
    
    def normalized(self) -> T:
        """
        Normalizes a 3D vector (returns a new vector with same direction but length of 1)
        Returns a new Vector3
        """
        
        mag = self.magnitude()
        return Vector3(self.x / mag, self.y / mag, self.z / mag)
    
    def dot(self, other: T) -> float:
        """
        Calculates a dot product of two 3D vectors
        other -- other vector to calculate dot product with
        Returns a float
        """
        
        return self.x * other.x + self.y * other.y + self.z * other.z
    
    def cross(self, other: T) -> T:
        """
        Calculates a cross product of two 3D vectors
        other -- other vector to calculate cross product with
        Returns a new Vector3
        """
        
        x = self.y * other.z - self.z * other.y
        y = self.z * other.x - self.x * other.z
        z = self.x * other.y - self.y * other.x
        
        return Vector3(x, y, z)
    
    def angle(self, other: T, in_degress: bool = False) -> float:
        """
        Calculates the angle subtended by two vectors.
        The return value is in radians, unless the in_degress flag is set to true. Than the return value is in degrees.
        other -- other vector to calculate angle with
        in_degrees -- flag to get the value in degrees or in radians
        Returns a float
        """
        
        dot_product = self.dot(other)
        self_mag = self.magnitude()
        other_mag = other.magnitude()
        angle = math.acos(dot_product / (self_mag * other_mag))
        
        return math.degrees(angle) if in_degress else angle
