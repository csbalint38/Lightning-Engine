# v0.3.0-beta - 2025.x.x

## Lightning+

## Extras
 - Added SamplingVisualization to visualize cubemap sampling function
 - Added visualization of importance sampling for specular

## Editor
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
 - Id syste
 - Entity system
 - Transform component
