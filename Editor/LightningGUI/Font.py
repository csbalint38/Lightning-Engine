from enum import Enum
from typing import Tuple

class FontWeight(Enum):
    BOLD = "bold",
    ITALIC = "italic"
    NORMAL = "normal"
    
class FontFamilies(Enum):
    SEGOE_UI = "Segoe_UI"
    ARIAL = "Arial"
    TIMES_NEW_ROMAN = "Times New Roman"
    CALIBRI = "Calibri"
    COURIER_NEW = "Courier New"
    VERDANA = "Verdana"
    TAHOMA = "Tahoma"
    IMPACT = "Impact"
    GEORGIA = "Georgia"
    PALATINO = "Palatino"
    GARAMOND = "Garamond"
    COMIC_SANS_MS = "Comic Sans MS"

class Font():
    def __new__(self, family: FontFamilies = FontFamilies.SEGOE_UI, size: int = 9, weight: FontWeight = FontWeight.NORMAL) -> Tuple[str, int, FontWeight]:
        return (family, size, weight.value)