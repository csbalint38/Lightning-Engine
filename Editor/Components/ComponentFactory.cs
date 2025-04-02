using Editor.Common.Enums;
using System.Diagnostics;

namespace Editor.Components
{
    static class ComponentFactory
    {
        private static readonly Func<Entity, object, Component>[] _functions =
        [
            (entity, data) => new Transform(entity),
            (entity, data) => new Script(entity)
            {
                Name = (string)data,
            }
        ];

        public static Func<Entity, object, Component> GetCreationFunction(ComponentType componentType)
        {
            Debug.Assert((int)componentType < _functions.Length);

            return _functions[(int)componentType];
        }

        public static ComponentType ToEnumType(this Component c) =>
            c switch
            {
                Transform _ => ComponentType.TRANSFORM,
                Script _ => ComponentType.SCRIPT,
                _ => throw new ArgumentException("Unknown component type")
            };
    }
}
