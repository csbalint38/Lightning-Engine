class ConfigException(Exception):
    """ Class to throw exception if there is an invalid value in config.ini """
    
    def __init__(self, variable: str, message: str ) -> None:
        _message_to_show = "Variable %s: %s" % (variable, message)

        super().__init__(_message_to_show)




