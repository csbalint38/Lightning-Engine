# v0.3.11-beta - 2025.9.30.

## Editor
 - Code cleanup (343)

## Engine
 - Taking screenshots
 - Migration to C++ 20
 - Code cleanup (239)

# v0.3.10-beta - 2025.9.16.
## General
 - Uninstaller now deletes runtime generated files from the installation location leaving no junk behind.

## Editor
 - Fixed saving and loading project that has Entities with Geometry component
 - Added Configuration Dialog with configureable MSBuild Path and Code Editor
 - SceneView improvements like Scene renaming, drag-and-drop entity creation
 - Code cleanup (26)

# v0.3.9-beta - 2025.9.3
## General
 - Fixed the Installer progress indicator, where placeholders were shown
 - Bundled Installer that installs `MSBuild` alongside the Engine

# v0.3.8-beta - 2025.8.19.
## Editor
 - Fixed wrong tooltip labels under certain icons
 - Added MSBuild support that will eventually replace VisualStudio dependency
 - Extended Geometry Component

## Engine
 - Fixed engine shaders path
 - Fixed shader compilation exit code

# v0.3.7-beta - 2025.8.5.
## General
 - CI workflow for release
 - Changed LICENCE format
 - Installer

## UI
 - Fixed the issue where only the first submeshes index and vertex count was displayes in the MeshRenderer
 - Fixet the issue where Icon and Screenshot was not displayed when opening an existing project after creating a new one
 - Fixed SceneView scrolling
 - Fixed MainWindow not getting keyboard focus back after renaming an Entity

## Editor
 - Icon
 - Geometry Component View

# v0.3.6-beta - 2025.7.22.

## Lightning+
 - Opening Linux window using XLib
 - Initialize OpenGL

## UI
 - Improved Texture Editor

## Editor
 - Geometry Component serialization

# v0.3.5-beta - 2025.7.8.

## UI 
 - Content Browser list view
 - Content Browser tile view
 - Custom Save File Dialog
 - Upgraded visuals for Configure Geometry Import Settings, Texture Import Settings and Texture Editor

 ## Editor
 - Geometry Component in Editor

# v0.3.4-beta - 2025.6.24.

## UI
 - Scalar and Vector controls
 - Upgraded visuals for Transform Component, Engine Path Dialog, New Script Dialog, Loading animation

## Editor
 - Custom .NET App host application to fix engine initialization error

# v0.3.3-beta - 2025.6.10.

## Editor
 - Initialize Engine from the Editor

# v0.3.2-beta - 2025.5.27.

## Editor 
 - Engine versioning
 - Fixed Project Teplates project files
 - Applied Materials

# v0.3.1-beta - 2025.5.13.

## UI
 - Styles for Window, Titlebar, Dialog Window, Accent Button, FlatButton, TextBox, TextBlock
 - Styles for IconButton, ScrollBar, ListBox, ListBoxItem
 - Restyled OpenProject page, NewProject page, Scenes view, History tab, Console tab
 - New template icons and screenshots

## Editor
 - Asset Metadata

# v0.3.0-beta - 2025.4.29.

## Lightning+

## Extras
 - Added SamplingVisualization to visualize cubemap sampling function
 - Added visualization of importance sampling for specular

## Editor
 - Material assets
 - Shader compilation in the Editor
 - Create cube in the Editor
 - View cubemaps in the Texture Editor
 - Create folders, rename assets
 - Pack textures in the Editor for the Engine
 - Reimport textures
 - Configure Texture import settings
 - Group textures
 - Asset import progression
 - Configure geometry import settings
 - Texture editor controls help section
 - Texture details in texture editor side panel
 - Texture loading status display
 - View individual color channels of a texture
 - Texture editor controls: zoom, center, original size
 - Viewing different mip-levels of a texture
 - Viewing block-compressed textures
 - Texture editor
 - Import textures
 - Pack meshes for the Engine in the Editor
 - Partial support for other mesh types: skeletal mesh, mesh with joints, mesh with normals, mesh with tangents
 - Highlight and isolate submeshes in FBX files
 - View FBX files
 - Drag-and-drop importer
 - Import FBX files
 - Content browser
 - Asset registry
 - Dynamic switch between projects
 - Saving Geometry assets
 - Smoothing angle support
 - Create UV Sphere 
 - Show textures in Primitive mesh view
 - Create Plane
 - Create primitive mesh dialog window
 - Changed the Editor from Flutter to WPF
 - Load Mesh data into the Editor
 - Multiple Renderer Window support
 - Renderer Window resize handling
 - Renderer Window inside the editor
 - Run the standalone game
 - Renderer Window inside the editor
 - Assign scripts to game entities
 - Script component
 - Build game code
 - Load game code DLL into Editor

## Engine
 - Add/remove shader to/from the Engine using the API
 - Shader compilation with DirectX ShaderCompiler
 - Support for HDRI images as skybox texture
 - Image based ambient lighting
 - Removed deprecated lighting system, that was not using bounding spheres 
 - IBL specular prefiltering
 - IBL optimalization - cubemap diffuse prefiltering
 - Remove deprecated Python EngineAPI

# v0.2.1-alpha - 2024.10.29

## Editor
 - Locate the Engine if it was moved or installed to a different location than default
 - Transform Component
 - Entities are now also created on the Engine side
 - Transform Component also created on the Engine side
 - Added Visual Studio Solution and Visual Studio Project to the templates
 - Create GameScript
 - Open Visual Studio using the Editor

## Engine
 - Geometry component
 - Support of Physically Based Rendering (PBR)
 - HDR as Cubemap support

# v0.2.0-alpha - 2024.08.18

## Editor
 - Added project templates
 - Switch from tkinter (Python) to Flutter (Dart)
 - Create new Project from Project Template
 - Open existing project
 - Add/remove Scenes
 - Undo/redo functionality
 - Add/remove Entities
 - Select, rename, enable/disable Entities
 - Developer console
 - Entity multiselection

## Engine
 - Create/remove Entities
 - Create Transform Component
 - Create game Script in C++
 - Load existing game
 - Run standalone game
 - Create Win32 window
 - Create Mesh primitives (Plane, UV Sphere)
 - DirectX 12 Renderer
 - HLSL shader compilation
 - Post-process shader
 - FBX importer (static meshes only)
 - Camera
 - Material system
 - Rendering 3D models
 - Light system with Phong shadig including Directional Ligh Point light Spotlight
 - Input system support for standard keyboard and standard mouse
 - Optimized light culling (support for up to 64k light - 60+ fps on mid-spec machines)
 - Texture importer
 - BC image compression
 - Texturing
 - Normal mapping (using MikkTSpace)


# v0.1.0-pre-alpha - 2024.04.23

## Editor
 - tkinter setup

## Engine
 - Id system
 - Entity system
 - Transform component
