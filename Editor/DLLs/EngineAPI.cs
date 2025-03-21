using Editor.Components;
using Editor.DLLs.Descriptors;
using System.Runtime.InteropServices;

namespace Editor.DLLs
{
    static class EngineAPI
    {
        private const string _dllName = "EngineDLL.dll";

        [DllImport(_dllName, EntryPoint = "create_game_entity")]
        private static extern int CreateGameEntity(GameEntityDescriptor desc);

        [DllImport(_dllName, EntryPoint = "remove_game_entity")]
        private static extern void RemoveGameEntity(int id);

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
    }
}
