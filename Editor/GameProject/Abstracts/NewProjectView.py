from LightningGUI import Event, Observer

class NewProjectView(Observer):
    def __init__(self):
        self.create_button_clicked = Event("create_button_clicked")
        self.name_changed = Event("name_changed")
        self.path_changed = Event("path_changed")
        self.selection_changed = Event("selection_changed")
        self.project_created = Event("project_created")