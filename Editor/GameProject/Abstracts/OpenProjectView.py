from LightningGUI import Event, Observer

class OpenProjectView(Observer):
    def __init__(self):
        self.open_button_clicked = Event("open_button_clicked")
        self.project_opened = Event("project_opened")
        self.selection_changed = Event("selection_changed")