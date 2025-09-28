using Editor.Components;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Editors;

/// <summary>
/// Interaction logic for TransformView.xaml
/// </summary>
public partial class TransformComponentView : UserControl
{
    private Action? _undoAction = null;
    private bool _propertyChanged = false;

    public TransformComponentView()
    {
        InitializeComponent();
        Loaded += OnTransformViewLoaded;
    }

    private void OnTransformViewLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnTransformViewLoaded;
        ((MSTransform)DataContext).PropertyChanged += (_, __) => _propertyChanged = true;
    }

    private void VbPosition_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_propertyChanged && _undoAction is not null) VbPosition_PreviewMouseLeftButtonUp(sender, null);
    }

    private void VbPosition_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _propertyChanged = false;
        _undoAction = GetPositionAction();
    }

    private void VbPosition_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs? e) =>
        RecordActions(GetPositionAction(), "Position changed");

    private void VbRotation_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_propertyChanged && _undoAction is not null) VbRotation_PreviewMouseLeftButtonUp(sender, null);
    }

    private void VbRotation_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _propertyChanged = false;
        _undoAction = GetRotationAction();
    }

    private void VbRotation_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs? e) =>
        RecordActions(GetRotationAction(), "Rotation changed");

    private void VbScale_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_propertyChanged && _undoAction is not null) VbScale_PreviewMouseLeftButtonUp(sender, null);
    }

    private void VbScale_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _propertyChanged = false;
        _undoAction = GetScaleAction();
    }

    private void VbScale_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs? e) =>
        RecordActions(GetScaleAction(), "Scale changed");

    private Action? GetAction(
        Func<Transform,
            (Transform transform, Vector3)> selector,
        Action<(Transform transform, Vector3)> forEachAction
    )
    {
        if (DataContext is not MSTransform vm)
        {
            _undoAction = null;
            _propertyChanged = false;

            return null;
        }

        var selection = vm.SelectedComponents.Select(x => selector(x)).ToList();
        return new Action(() =>
        {
            selection.ForEach(x => forEachAction(x));
            ((MSEntity)EntityView.Instance!.DataContext)?.GetMSComponent<MSTransform>()?.Refresh();
        });
    }

    private Action? GetPositionAction() =>
        GetAction((x) => (x, x.Position), (x) => x.transform.Position = x.Item2);

    private Action? GetRotationAction() =>
        GetAction((x) => (x, x.Rotation), (x) => x.transform.Rotation = x.Item2);

    private Action? GetScaleAction() => GetAction((x) => (x, x.Scale), (x) => x.transform.Scale = x.Item2);

    private void RecordActions(Action? redoAction, string name)
    {
        if (_propertyChanged)
        {

            Debug.Assert(_undoAction != null);

            _propertyChanged = false;
            Project.UndoRedo.Add(new UndoRedoAction(name, _undoAction, redoAction!));
        }
    }
}
