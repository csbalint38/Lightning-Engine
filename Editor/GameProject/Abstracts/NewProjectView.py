from LightningGUI import Event, Observer

class NewProjectView(Observer):
    def __init__(self):
        self.create_button_clicked = Event("create_button_clicked")