from dataclasses import dataclass

@dataclass
class ProjectTemplate:
    ProjectType: str
    ProjectFile: str
    Folders: list
    #IconPath: str
    #ScreenshotPath: str
    #ProjectFilePath: str