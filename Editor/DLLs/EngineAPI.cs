using Editor.Common.Enums;
using Editor.Components;
using Editor.DLLs.Descriptors;
using Editor.GameProject;
using Editor.Utilities;
using System.Runtime.InteropServices;

namespace Editor.DLLs
{
    static class EngineAPI
    {
        private const string _engineDll = "EngineDLL.dll";

        [DllImport(_engineDll, EntryPoint = "create_game_entity")]
        private static extern int CreateGameEntity(GameEntityDescriptor desc);

        [DllImport(_engineDll, EntryPoint = "remove_game_entity")]
        private static extern void RemoveGameEntity(int id);

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
        public static extern IntPtr GetWindowHandle(int surfaceId);

        [DllImport(_engineDll, EntryPoint = "resize_renderer_surface")]
        public static extern void ResizeRenderSurface(int SurfaceId);

        public static int CreateGameEntity(Entity entity)
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
    }
}
