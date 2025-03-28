using Editor.Common.Enums;
using Editor.Components;
using Editor.GameProject;
using Editor.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for EntityView.xaml
    /// </summary>
    public partial class EntityView : UserControl
    {
        public static EntityView Instance { get; private set; }

        private Action _undoAction;
        private string _propertyName;

        public EntityView()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;

            DataContextChanged += (_, __) =>
            {
                if (DataContext is not null)
                {
                    (DataContext as MSEntity).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
        }
        private Action GetRenameAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();

            return new Action(() =>
            {
                selection.ForEach(item => item.entity.Name = item.Name);
                (DataContext as MSEntity).Refresh();
            });
        }

        private Action GetIsEnabledAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();

            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
                (DataContext as MSEntity).Refresh();
            });
        }

        private void TbName_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _propertyName = string.Empty;
            _undoAction = GetRenameAction();
        }

        private void TbName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(_propertyName == nameof(MSEntity.Name) && _undoAction is not null)
            {
                var redoAction = GetRenameAction();

                Project.UndoRedo.Add(new UndoRedoAction(
                    $"Rename {(DataContext as MSEntity).SelectedEntities.Count} entities",
                    _undoAction,
                    redoAction
                ));

                _propertyName = null;
            }

            _undoAction = null;
        }

        private void ChBIsEnabled_Click(object sender, RoutedEventArgs e)
        {
            var undoAction = GetIsEnabledAction();
            var vm = DataContext as MSEntity;

            vm.IsEnabled = (sender as CheckBox).IsChecked == true;

            var redoAction = GetIsEnabledAction();

            Project.UndoRedo.Add(new UndoRedoAction(
                vm.IsEnabled == true
                    ?  $"Enable {vm.SelectedEntities.Count} game entities"
                    : $"Disable {vm.SelectedEntities.Count} game entities",
                undoAction,
                redoAction
            ));
        }

        private void MIScriptComponent_Click(object sender, RoutedEventArgs e) =>
            AddComponent(ComponentType.SCRIPT, (sender as MenuItem).Header.ToString());

        private void TgbAddComponent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var menu = FindResource("addComponentMenu") as ContextMenu;
            var btn = sender as ToggleButton;

            btn.IsChecked = true;
            menu.Placement = PlacementMode.Bottom;
            menu.PlacementTarget = btn;
            menu.MinWidth = btn.ActualWidth;
            menu.IsOpen = true;
        }

        private void AddComponent(ComponentType type, object data)
        {
            var creationFunction = ComponentFactory.GetCreationFunction(type);
            var changedEntities = new List<(Entity entity, Component component)>();
            var vm = DataContext as MSEntity;

            foreach (var entity in vm.SelectedEntities)
            {
                var component = creationFunction(entity, data);

                if (entity.AddComponent(component)) changedEntities.Add((entity, component));
            }

            if (changedEntities.Count != 0)
            {
                vm.Refresh();

                Project.UndoRedo.Add(new UndoRedoAction(
                    $"Add {type} component to {changedEntities.Count} entities",
                    () =>
                    {
                        changedEntities.ForEach(x => x.entity.RemoveComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    },
                    () =>
                    {
                        changedEntities.ForEach(x => x.entity.AddComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    }
                ));
            }
        }
    }
}
