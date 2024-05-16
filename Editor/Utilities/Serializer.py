import dataclasses
from xml.dom.minidom import Document, parse, Element
from typing import TypeVar, Type, Any

T = TypeVar('T')

class Serializer():
    
    @staticmethod
    def to_file(obj: T, path: str) -> None:
        document = Document()
        root = document.createElement(type(obj).__name__)
        document.appendChild(root)
        Serializer.build_xml(root, document, obj)
        
        with open(path, 'w', encoding='UTF-8') as file:
            print(path)
            document.writexml(file, indent="", addindent="\t", newl="\n")
        
    @staticmethod
    def build_xml(parent, document: Document, obj: T) -> None:
        if dataclasses.is_dataclass(obj):
            for key in dataclasses.asdict(obj):
                tag = document.createElement(key)
                parent.appendChild(tag)
                Serializer.build_xml(tag, document, getattr(obj, key))
        elif isinstance(obj, list):
            for element in obj:
                tag = document.createElement(type(element).__name__)
                Serializer.build_xml(tag, document, element)
                parent.appendChild(tag)
        elif isinstance(obj, dict):
            for key in obj:
                tag = document.createElement(key)
                parent.appendChild(tag)
                Serializer.build_xml(tag, document, obj[key])
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
    def parse_xml(cls: Type[T], node: Element) -> Any:
        if dataclasses.is_dataclass(cls):
            fields = {field.name: field.type for field in dataclasses.fields(cls)}
            kwargs = {}
            for key, field_type in fields.items():
                element = node.getElementsByTagName(key)[0]
                value = Serializer.parse_xml(field_type, element)
                kwargs[key] = value
            return cls(**kwargs)
        elif cls is list:
            elements = node.childNodes
            return [Serializer.parse_xml(type(elements[0]), element) for element in elements if isinstance(element, Element)]
        elif cls is dict:
            result = {}
            for child in node.childNodes:
                if isinstance(child, Element):
                    result[child.nodeName] = Serializer.parse_xml(type(result), child)
            return result
        else:
            return node.firstChild.nodeValue if node.firstChild else None