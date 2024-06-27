from xml.dom.minidom import Document, parse, Element
from typing import TypeVar, Type, Any

from Utilities.Serializable import Serializable

T = TypeVar('T')

class Serializer():
    
    @staticmethod
    def to_file(obj: T, path: str) -> None:
        document = Document()
        root = document.createElement(type(obj).__name__)
        document.appendChild(root)
        Serializer.build_xml(root, document, obj)
        
        with open(path, 'w', encoding='UTF-8') as file:
            document.writexml(file, indent="", addindent="\t", newl="\n")
        
    @staticmethod
    def build_xml(parent, document: Document, obj: T) -> None:
        if isinstance(obj, Serializable):
            for field in obj.data_members:
                tag = document.createElement(field["name"])
                parent.appendChild(tag)
                Serializer.build_xml(tag, document, getattr(obj, field["field"]))
        elif isinstance(obj, list):
            for element in obj:
                tag = document.createElement(type(element).__name__)
                Serializer.build_xml(tag, document, element)
                parent.appendChild(tag)
        elif isinstance(obj, dict):
            for key, value in obj.items():
                tag = document.createElement(key)
                parent.appendChild(tag)
                Serializer.build_xml(tag, document, value)
        else:
            data = str(obj)
            tag = document.createTextNode(data)
            parent.appendChild(tag)
    
    @staticmethod
    def from_file(cls: Type[T], path: str) -> T:
        document = parse(path)
        root = document.documentElement
        return Serializer.parse_xml(cls, root)
    
    @staticmethod
    def parse_xml(cls: Type[T], node: Element, **kwargs) -> Any:
        if issubclass(cls, Serializable):
            kwargs = {}
            for field in cls.data_members:
                element = node.getElementsByTagName(field["name"])
                arg = field.get("list_type")
                value = Serializer.parse_xml(field["type"], element[0], list_type = arg)
                kwargs[field["field"][1:]] = value
            return cls(**kwargs)
        elif cls is list:
            elements = node.childNodes
            return [Serializer.parse_xml(kwargs["list_type"], element) for element in elements if isinstance(element, Element)]
        elif cls is dict:
            result = {}
            for child in node.childNodes:
                if isinstance(child, Element):
                    result[child.nodeName] = Serializer.parse_xml(type(result), child)
            return result
        else:
            if not node.firstChild: return None
            elif cls is int: return int(node.firstChild.nodeValue)
            elif cls is float: return float(node.firstChild.nodeValue)
            elif cls is bool: return True if node.firstChild.nodeValue == "True" else False
            else: return node.firstChild.nodeValue