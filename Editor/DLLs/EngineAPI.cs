using Editor.Common.Enums;
using Editor.Components;
using Editor.Content;
using Editor.DLLs.Descriptors;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Editor.DLLs
{
    static partial class EngineAPI
    {
        private const string _engineDll = "EngineDLL.dll";

        [DllImport(_engineDll, EntryPoint = "create_game_entity")]
        private static extern IdType CreateGameEntity(GameEntityDescriptor desc);

        [LibraryImport(_engineDll, EntryPoint = "remove_game_entity")]
        public static partial void RemoveGameEntity(IdType id);

        [DllImport(_engineDll, EntryPoint = "compile_shader")]
        private static extern int CompileShader([In, Out] ShaderData data);

        [DllImport(_engineDll, EntryPoint = "add_shader_group")]
        private static extern IdType AddShaderGroup([In] ShaderGroupData data);

        [LibraryImport(_engineDll, EntryPoint = "initialize_engine")]
        public static partial EngineInitError InitializeEngine(); // Add parameter if Engine will support multiple APIs

        [LibraryImport(_engineDll, EntryPoint = "shutdown_engine")]
        public static partial void ShutdownEngine();

        [LibraryImport(
            _engineDll,
            EntryPoint = "load_game_code_dll",
            StringMarshalling = StringMarshalling.Custom,
            StringMarshallingCustomType = typeof(AnsiStringMarshaller)
        )]
        public static partial int LoadGameCodeDll(string path);

        [LibraryImport(_engineDll, EntryPoint = "unload_game_code_dll")]
        public static partial int UnloadGameCodeDll();

        [DllImport(_engineDll, EntryPoint = "get_script_names")]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public static extern string[] GetScriptNames();

        [LibraryImport(
            _engineDll,
            EntryPoint = "get_script_creator",
            StringMarshalling = StringMarshalling.Custom,
            StringMarshallingCustomType = typeof(AnsiStringMarshaller)
        )]
        public static partial IntPtr GetScriptCreator(string name);

        [LibraryImport(_engineDll, EntryPoint = "create_renderer_surface")]
        public static partial int CreateRendererSurface(IntPtr host, int width, int height);

        [LibraryImport(_engineDll, EntryPoint = "remove_renderer_surface")]
        public static partial void RemoveRendererSurface(int surfaceId);

        [LibraryImport(_engineDll, EntryPoint = "get_window_handle")]
        public static partial IntPtr GetWindowHandle(int surfaceId);

        [LibraryImport(_engineDll, EntryPoint = "resize_renderer_surface")]
        public static partial void ResizeRenderSurface(int SurfaceId);

        [LibraryImport(_engineDll, EntryPoint = "remove_shader_group")]
        public static partial void RemoveShaderGroup(IdType id);

        [LibraryImport(_engineDll, EntryPoint = "create_resource")]
        public static partial IdType CreateResource([In] byte[] data, int type);

        [LibraryImport(_engineDll, EntryPoint = "destroy_resource")]
        public static partial void DestroyResource(IdType id, int type);

        [LibraryImport(_engineDll, EntryPoint = "set_geometry_ids")]
        public static partial void SetGeometryIds(int surfaceId, [In] IdType[] geometryComponentIds, int count);

        [LibraryImport(_engineDll, EntryPoint = "render_frame")]
        public static partial void RenderFrame(int surfaceId, IdType cameraId, ulong lightSet);

        [DllImport(_engineDll, EntryPoint = "update_component")]
        private static extern int UpdateComponent(IdType entityId, GameEntityDescriptor desc, ComponentType type);

        [LibraryImport(_engineDll, EntryPoint = "get_component_id")]
        public static partial IdType GetComponentId(IdType entityId, ComponentType type);

        public static IdType CreateGameEntity(Entity entity)
        {
            using GameEntityDescriptor desc = new();

            {
                var c = entity.GetComponent<Transform>()!;
                desc.Transform.Position = c.Position;
                desc.Transform.Rotation = c.Rotation;
                desc.Transform.Scale = c.Scale;
            }
            {
                if (entity.GetComponent<Script>() is Script c && Project.Current is not null)
                {
                    if (Project.Current.AvailableScripts.Contains(c.Name))
                    {
                        desc.Script.ScriptCreator = GetScriptCreator(c.Name);
                    }
                    else
                    {
                        Logger.LogAsync(
                            LogLevel.ERROR,
                            $"Unable to find script with name {c.Name}. GameEntity will be created without script component!"
                        );
                    }
                }
            }
            {
                if (entity.GetComponent<Components.Geometry>() is Components.Geometry c)
                {
                    Debug.Assert(c.Materials.Count > 0 && Id.IsValid(c.ContentId));

                    desc.Geometry = new(c);
                }
            }

            return CreateGameEntity(desc);
        }

        public static IdType AddShaderGroup(ShaderGroup shaderGroup)
        {
            using var data = new ShaderGroupData();

            data.Type = (int)shaderGroup.Type;
            data.Count = shaderGroup.Count;

            var packageData = shaderGroup.PackForEngine();

            if (packageData is null || packageData.Length == 0) throw new Exception("Invalid shader data");

            data.DataSize = packageData.Length;
            data.Data = Marshal.AllocCoTaskMem(data.DataSize);

            Marshal.Copy(packageData, 0, data.Data, data.DataSize);

            return AddShaderGroup(data);
        }

        public static void CompileShader(ShaderGroup shaderGroup)
        {
            Debug.Assert(!string.IsNullOrEmpty(shaderGroup?.Code));
            Debug.Assert(!string.IsNullOrEmpty(shaderGroup.FunctionName));
            Debug.Assert(shaderGroup.ExtraArgs?.Any() == true);
            Debug.Assert(!shaderGroup.ByteCode.Any() == true);

            shaderGroup.ByteCode.Clear();
            shaderGroup.Errors.Clear();
            shaderGroup.Assembly.Clear();

            try
            {
                foreach (var args in shaderGroup.ExtraArgs)
                {
                    using var data = new ShaderData();
                    var code = Encoding.Default.GetBytes([.. shaderGroup.Code]);

                    data.Type = (int)shaderGroup.Type;
                    data.CodeSize = code.Length;
                    data.FunctionName = shaderGroup.FunctionName;
                    data.ExtraArgs = args.Any() ? string.Join(';', args) : string.Empty;
                    data.Code = Marshal.AllocCoTaskMem(code.Length);

                    Marshal.Copy(code, 0, data.Code, data.CodeSize);

                    if (CompileShader(data) == 0) throw new Exception("Shader compilation failed");

                    var bytes = new byte[data.ByteCodeSize + data.ErrorSize + data.AssemblySize + data.HashSize];

                    Marshal.Copy(data.ByteCodeErrorAssemblyHash, bytes, 0, bytes.Length);

                    int offset = 0;

                    if (data.ByteCodeSize > 0)
                    {
                        var byteCode = new byte[data.ByteCodeSize];

                        Array.Copy(bytes, offset, byteCode, 0, data.ByteCodeSize);
                        shaderGroup.ByteCode.Add(byteCode);

                        offset += data.ByteCodeSize;
                    }
                    else shaderGroup.ByteCode.Add([]);

                    if (data.ErrorSize > 0)
                    {
                        var errors = new byte[data.ErrorSize];
                        Array.Copy(bytes, offset, errors, 0, data.ErrorSize);
                        var errorString = Encoding.Default.GetString(errors);
                        shaderGroup.Errors.Add(errorString);

                        Logger.LogAsync(data.ByteCodeSize > 0 ? LogLevel.WARNING : LogLevel.ERROR, errorString);

                        offset += data.ErrorSize;
                    }
                    else shaderGroup.Errors.Add(string.Empty);

                    if (data.AssemblySize > 0)
                    {
                        var assembly = new byte[data.AssemblySize];
                        Array.Copy(bytes, offset, assembly, 0, data.AssemblySize);
                        shaderGroup.Assembly.Add(Encoding.Default.GetString(assembly));
                        offset += data.AssemblySize;
                    }
                    else shaderGroup.Assembly.Add(string.Empty);

                    if (data.HashSize > 0)
                    {
                        var hash = new byte[data.HashSize];
                        Array.Copy(bytes, offset, hash, 0, data.HashSize);
                        shaderGroup.Hash.Add(hash);
                        offset += data.HashSize;
                    }
                    else shaderGroup.Hash.Add([]);
                }
            }
            catch (Exception ex)
            {
                Logger.LogAsync(LogLevel.ERROR, $"Failed to compile shader {shaderGroup.FunctionName}");
                Debug.WriteLine(ex.Message);

                throw;
            }
        }

        public static bool UpdateComponent(Entity entity, ComponentType type)
        {
            Debug.Assert(Id.IsValid(entity?.EntityId ?? Id.InvalidId));
            Debug.Assert(type != ComponentType.TRANSFORM);

            using GameEntityDescriptor desc = new();

            switch (type)
            {
                case ComponentType.TRANSFORM: return false;
                case ComponentType.SCRIPT:
                    {
                        if (entity?.GetComponent<Script>() is Script c)
                        {
                            Debug.Assert(Project.Current is not null);

                            if (Project.Current.AvailableScripts.Contains(c.Name))
                            {
                                desc.Script.ScriptCreator = GetScriptCreator(c.Name);
                            }
                            else
                            {
                                Logger.LogAsync(
                                    LogLevel.ERROR,
                                    $"Unable to find script with name {c.Name}. Script component will not be updated!"
                                );
                            }
                        }
                    }

                    break;
                case ComponentType.GEOMETRY:
                    {
                        if (entity?.GetComponent<Components.Geometry>() is Components.Geometry c)
                        {
                            Debug.Assert(c.Materials.Count > 0 && Id.IsValid(c.ContentId));

                            desc.Geometry = new(c);
                        }
                    }
                    break;
                default:
                    break;
            }

            return UpdateComponent(entity?.EntityId ?? Id.InvalidId, desc, type) != 0;
        }
    }
}
