from typing import Callable

class Event():
    def __init__(self, name: str) -> None:
        self.name = name
        self.subscribers = []
        
    def subscribe(self, subscriber: Callable) -> None:
        self.subscribers.append(subscriber)
        
    def unsubscribe(self, subscriber: Callable) -> None:
        self.subscribers.remove(subscriber)
        
    def __call__(self, *args, **kwargs):
        for subscriber in self.subscribers:
            subscriber(*args, **kwargs)