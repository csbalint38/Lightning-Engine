import tkinter

from modules.sampler import Sampler

class Canvas(tkinter.Canvas):
    def __init__(self, root: tkinter.Tk, sampler: Sampler):
        super().__init__(root, width=1280, height=700)
        
        self.root = root
        self.sampler = sampler
        self.size = 300
        self.color = "#7393B3"
        self.font = ("Arial", 20, "bold")
        
        self.start_pos = {
            "top": [320, 300],
            "side": [960, 10]
        }
        
        self.draw_text()
        
    def draw(self):
        for x, y in zip(self.sampler.x_points, self.sampler.y_points):
            x *= self.size
            y *= self.size
            x += self.start_pos["top"][0]
            y += self.start_pos["top"][1]
            
            self.create_oval(x, y, x + 1, y + 1, fill=self.color, outline=None)
            
        for y, z in zip(self.sampler.y_points, self.sampler.z_points):
            z = 1 - z
            y *= self.size
            z *= self.size
            y += self.start_pos["side"][0]
            z += self.start_pos["side"][1]
            
            self.create_oval(y, z, y + 1, z + 1, fill=self.color, outline=None)
            
    def draw_text(self):
        self.create_text(60, 40, text="TOP", fill=self.color, font=self.font)
        self.create_text(960, 400, text="SIDE", fill=self.color, font=self.font)
            
    def update(self):
        self.delete("all")
        self.draw_text()
        self.draw()
        
    def clear(self):
        self.delete("all")
        self.draw_text()
        
