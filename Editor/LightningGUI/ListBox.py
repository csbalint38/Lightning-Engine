import tkinter

from .ScrolleableContainer import ScrollableContainer
from .Image import Image
from .Container import Container
from .Utilities.Event import Event

class ListBox(ScrollableContainer):
    def __init__(self, master: tkinter.Widget, multiselect: bool = False, can_deselect_all: bool = False, **kwargs) -> None:
        super().__init__(master, **kwargs)
        self.multiselect = multiselect
        self.can_deselect_all = can_deselect_all
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
        
        item_frame.bind_child("<Button-1>", lambda _: self.toggle_selection(item_frame))
        
        self.items.append((text, item_frame))
        
    def add_widget(self, widget: Container) -> None:
        y = len(self.items) * widget.winfo_height()
        
        widget.pack(fill=tkinter.X)
        
        widget.bind_child("<Button-1>", lambda _: self.toggle_selection(widget))
        
        self.items.append((None, widget))
        
    def toggle_selection(self, item_frame: tkinter.Frame) -> None:
        prev_indicies = self.selected_indicies.copy()
        index = [i for i, (_, frame) in enumerate(self.items) if frame == item_frame][0]
        if index in self.selected_indicies:
            self.deselect_item(index)
        else:
            self.select_item(index)
        if self.selected_indicies != prev_indicies:
            self.selection_changed() 
            
    def select_item(self, index: int) -> None:
        if not self.multiselect and len(self.selected_indicies) > 0 and self.can_deselect_all:
            self.deselect_item(self.selected_indicies[0])
        _, item_frame = self.items[index]
        item_frame.config_all(bg="blue")
        self.selected_indicies.append(index)
        if not self.multiselect and len(self.selected_indicies) > 0 and not self.can_deselect_all:
            self.deselect_item(self.selected_indicies[0])
        
    def deselect_item(self, index: int) -> None:
        if not self.can_deselect_all and len(self.selected_indicies) == 1:
            return
        _, item_frame = self.items[index]
        item_frame.config_all(bg="white")
        self.selected_indicies.remove(index)