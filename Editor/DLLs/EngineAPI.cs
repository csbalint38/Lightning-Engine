using Editor.Components;
using Editor.DLLs.Descriptors;
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
        private static extern int LoadGameCodeDLL(string path);

        [DllImport(_engineDll, EntryPoint = "unload_game_code_dll")]
        private static extern int UnloadGameCodeDLL();

        public static int CreateGameEntity(Entity entity)
        {
            GameEntityDescriptor desc = new();

            {
                var c = entity.GetComponent<Transform>();
                desc.Transform.Position = c.Position;
                desc.Transform.Rotation = c.Rotation;
                desc.Transform.Scale = c.Scale;
            }

            return CreateGameEntity(desc);
        }

        public static void RemoveGameEntity(Entity entity) => RemoveGameEntity(entity.EntityId);
        public static int LoadGameCodeDll(string path) => LoadGameCodeDLL(path);
        public static int UloadGameCodeDll() => UloadGameCodeDll();
    }
}
