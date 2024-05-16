import tkinter
from .ScrolleableContainer import ScrollableContainer
from .Image import Image
from .Container import Container
from .Utilities.Event import Event

class ListBox(ScrollableContainer):
    def __init__(self, master: tkinter.Widget, multiselect: bool = False, **kwargs) -> None:
        super().__init__(master, **kwargs)
        self.multiselect = multiselect
        self.items = []
        self.selected_indicies = []
        self.selection_changed = Event("listbox::selection_changed")
        
    def add_item(self, text: str, image: str = None) -> None:
        y = len(self.items) * 50
        item_frame = Container(self.viewport, background="white")
        item_frame.pack(fill=tkinter.X)
        
        label = tkinter.Label(item_frame, text=text, anchor="w")
        
        if image:
            image = Image(item_frame, image, 40)
            image.pack(side=tkinter.LEFT, padx=5, pady=5)
       
        label.pack(side=tkinter.LEFT, padx=(5, 0), pady=5)
        
        item_frame.bind("<Button-1>", lambda _: self.toggle_selection(item_frame))
        
        self.items.append((text, item_frame))
        
    def toggle_selection(self, item_frame: tkinter.Frame) -> None:
        self.selection_changed()
        index = [i for i, (_, frame) in enumerate(self.items) if frame == item_frame][0]
        if index in self.selected_indicies:
            self.deselect_item(index)
        else:
            self.select_item(index)
            
    def select_item(self, index: int) -> None:
        if not self.multiselect and len(self.selected_indicies) > 0:
            self.deselect_item(self.selected_indicies[0])
        _, item_frame = self.items[index]
        item_frame.config_all(bg="blue")
        self.selected_indicies.append(index)
        
    def deselect_item(self, index: int) -> None:
        _, item_frame = self.items[index]
        item_frame.config_all(bg="white")
        self.selected_indicies.remove(index)