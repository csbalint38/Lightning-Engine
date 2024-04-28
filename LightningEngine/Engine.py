import ctypes
from Config import Config

class Engine:
    """
    A singleton class that initializes Lightning Engine and exposes its basic functions.
    """
    
    _instance = None
    _dll_name = Config().get_dll_path()

    def __new__(cls, *args, **kwargs) -> "Config":
        if not cls._instance:
            cls._instance = super().__new__(cls, *args, **kwargs)
        return cls._instance
    
    def __init__(self) -> None:
        self._lib = ctypes.CDLL(Engine._dll_name)
        
    def create_game_entity(des) -> int:
        """
        Creates a game entity in the Engine
        Returns the id of the game entity in the Engine
    
        desc -- GameEntity type
        """
        
        