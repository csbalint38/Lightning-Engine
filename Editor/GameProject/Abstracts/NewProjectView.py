from LightningGUI import Event, Observer

class NewProjectView(Observer):
    def __init__(self):
        self.create_button_clicked = Event("create_button_clicked")
        self.name_changed = Event("name_changed")
        self.path_changed = Event("path_changed")