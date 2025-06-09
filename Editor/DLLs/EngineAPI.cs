using Editor.Common.Enums;
using Editor.Components;
using Editor.Content;
using Editor.DLLs.Descriptors;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Editor.DLLs
{
    static class EngineAPI
    {
        private const string _engineDll = "EngineDLL.dll";

        [DllImport(_engineDll, EntryPoint = "create_game_entity")]
        private static extern IdType CreateGameEntity(GameEntityDescriptor desc);

        [DllImport(_engineDll, EntryPoint = "remove_game_entity")]
        private static extern void RemoveGameEntity(IdType id);

        [DllImport(_engineDll, EntryPoint = "compile_shader")]
        private static extern int CompileShader([In, Out] ShaderData data);

        [DllImport(_engineDll, EntryPoint = "add_shader_group")]
        private static extern IdType AddShaderGroup([In] ShaderGroupData data);

        [DllImport(_engineDll, EntryPoint = "initialize_engine")]
        public static extern EngineInitError InitializeEngine(); // Add parameter if Engine will support multiple APIs

        [DllImport(_engineDll, EntryPoint = "shutdown_engine")]
        public static extern void ShutdownEngine();

        [DllImport(_engineDll, EntryPoint = "load_game_code_dll", CharSet = CharSet.Ansi)]
        public static extern int LoadGameCodeDll(string path);

        [DllImport(_engineDll, EntryPoint = "unload_game_code_dll")]
        public static extern int UnloadGameCodeDll();

        [DllImport(_engineDll, EntryPoint = "get_script_names")]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        public static extern string[] GetScriptNames();

        [DllImport(_engineDll, EntryPoint = "get_script_creator")]
        public static extern IntPtr GetScriptCreator(string name);

        [DllImport(_engineDll, EntryPoint = "create_renderer_surface")]
        public static extern int CreateRendererSurface(IntPtr host, int width, int height);

        [DllImport(_engineDll, EntryPoint = "remove_renderer_surface")]
        public static extern void RemoveRendererSurface(int surfaceId);

        [DllImport(_engineDll, EntryPoint = "get_window_handle")]
        public static extern IntPtr GetWindowHandle(IdType surfaceId);

        [DllImport(_engineDll, EntryPoint = "resize_renderer_surface")]
        public static extern void ResizeRenderSurface(int SurfaceId);

        [DllImport(_engineDll, EntryPoint = "remove_shader_group")]
        public static extern void RemoveShaderGroup(IdType id);

        [DllImport(_engineDll, EntryPoint = "create_resource")]
        private static extern IdType CreateResource(IntPtr data, int type);

        [DllImport(_engineDll, EntryPoint = "destroy_resource")]
        public static extern void DestroyResource(IdType id, int type);

        public static IdType CreateGameEntity(Entity entity)
        {
            GameEntityDescriptor desc = new();

            {
                var c = entity.GetComponent<Transform>();
                desc.Transform.Position = c.Position;
                desc.Transform.Rotation = c.Rotation;
                desc.Transform.Scale = c.Scale;
            }
            {
                var c = entity.GetComponent<Script>();

                if (c is not null && Project.Current is not null)
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

            var id = CreateGameEntity(desc);
            return id;
        }

        public static void RemoveGameEntity(Entity entity) => RemoveGameEntity(entity.EntityId);

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

            return AddShaderGroup(shaderGroup);
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

        public static IdType CreateResource(byte[] resourceData, AssetType type)
        {
            IntPtr data = IntPtr.Zero;

            try
            {
                data = Marshal.AllocCoTaskMem(resourceData.Length);

                Marshal.Copy(resourceData, 0, data, resourceData.Length);

                return CreateResource(data, (int)type);
            }
            finally
            {
                Marshal.FreeCoTaskMem(data);
            }
        }
    }
}
