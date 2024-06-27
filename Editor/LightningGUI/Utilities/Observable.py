from abc import ABC, abstractmethod

class Observable(ABC):
    def __init__(self) -> None:
        self._observers = []
        
    def add_observer(self, observer: object) -> None:
        self._observers.append(observer)
        
    def remove_observer(self, observer: object) -> None:
        self._observers.remove(observer)
        
    def notify(self, property_name: str) -> None:
        for observer in self._observers:
            observer.property_changed(property_name)
            
class Observer(ABC):
    @abstractmethod
    def property_changed(property_name: str) -> None:
        raise NotImplementedError()