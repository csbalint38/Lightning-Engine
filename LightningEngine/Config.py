import configparser
import Exceptions.ConfigException

class Config:
    """
    Singleton class to read values from config.ini and validate value types
    """
    
    _instance = None
    _config_path = "engine.ini"

    def __new__(cls, *args, **kwargs) -> "Config":
        if not cls._instance:
            cls._instance = super().__new__(cls, *args, **kwargs)
        return cls._instance
    
    def __init__(self) -> None:
        self.config = configparser.ConfigParser()
        
        self.config.read(Config._config_path)
        
    def get_dll_path(self) -> str:
        """
        Returns the path of the EngineDLL from the config.ini
        Rises ConfigException if Library of DLL_PATH is missing
        """

        if "Library" not in self.config:
            raise Exceptions.ConfigException.ConfigException("DLL_PATH", "Library missing from config.ini")
        
        if "DLL_PATH" not in self.config["Library"]:
            raise Exceptions.ConfigException.ConfigException("DLL_PATH", "DLL_PATH missing from config.ini")
        
        return self.config["Library"]["DLL_PATH"]