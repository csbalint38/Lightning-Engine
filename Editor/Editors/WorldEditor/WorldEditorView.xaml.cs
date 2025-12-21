using Editor.Components;
using Editor.Config;
using Editor.Content;
using Editor.Content.ContentBrowser;
using Editor.GameCode;
using Editor.GameProject;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Editors;

/// <summary>
/// Interaction logic for WorldEditorView.xaml
/// </summary>
public partial class WorldEditorView : UserControl
{
    public WorldEditorView()
    {
        InitializeComponent();

        Project.SceneUpdated += OnSceneUpdated;
        DataContextChanged += OnWorldEditorDataContextChanged;
    }

    private void OnWorldEditorDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => Focus();

    private void OnSceneUpdated(object? sender, EventArgs e)
    {
        Debug.Assert((sender as Scene)?.Project == Project.Current);

        if (sender is Scene scene)
        {
            var ids = scene.IsActive ? scene.GetGeometryComponentIds() : [];

            sv1.RenderSurfaceControl.SetComponentIds(ids);
            sv2.RenderSurfaceControl.SetComponentIds(ids);
            sv3.RenderSurfaceControl.SetComponentIds(ids);
            sv4.RenderSurfaceControl.SetComponentIds(ids);
        }
    }

    private void WorldEditorView_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= WorldEditorView_OnLoaded;
        Focus();
    }

    private void BtnNewScript_Click(object sender, RoutedEventArgs e) => new NewScriptDialog().ShowDialog();
    private void BtnPrimitiveMesh_Click(object sender, RoutedEventArgs e) => new PrimitiveMeshDialog().ShowDialog();

    private void MINewProject_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        ProjectBrowserDialog.GoToNewProjectTab = true;

        UnloadAndCloseAllWindows();
    }

    private void MIOpenProject_Executed(object sender, ExecutedRoutedEventArgs e) => UnloadAndCloseAllWindows();
    private void MIExit_Executed(object sender, ExecutedRoutedEventArgs e) => Application.Current.MainWindow.Close();
    private void ContentBrowserView_Loaded(object sender, RoutedEventArgs e) =>
        ContentBrowserView_IsVisibleChanged(sender, default);

    private void ContentBrowserView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (
            ((FrameworkElement)sender).DataContext is ContentBrowser cb &&
            string.IsNullOrEmpty(cb.SelectedFolder?.Trim())
        )
        {
            cb.SelectedFolder = cb.ContentFolder;
        }
    }

    private void UnloadAndCloseAllWindows()
    {
        Project.Current?.Unload();

        var mainWindow = Application.Current.MainWindow;

        foreach (Window win in Application.Current.Windows)
            if (win != mainWindow) win.Close();

        mainWindow.DataContext = null;

        Project.Current?.Unload();

        mainWindow.Close();
    }

    private void BtnImport_Click(object sender, RoutedEventArgs e) =>
        contentBrowserView.OpenImportSettingsConfigurator();

    private void BtnBugReport_Click(object sender, RoutedEventArgs e)
    {
        var psi = new ProcessStartInfo
        {
            FileName =
                "https://docs.google.com/forms/d/e/1FAIpQLSeRvHm2lCkyRihm-OmrU0Iclv_iSVMvrasasV-anfdNa-hMGA/viewform?usp=header",
            UseShellExecute = true,
        };

        Process.Start(psi);
    }

    private void BtnOpenEditor_Click(object sender, RoutedEventArgs e) =>
        ICodeEditor.Current.ShowWindow(Project.Current!.Solution);

    private void Options_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ConfigSettingsDialog();

        dialog.ShowDialog();
    }

    private void OnProjectLayoutView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && e.OriginalSource is not TextBox && MSEntity.CurrentSelection?.SelectedEntities.Count > 0)
        {
            var avgPos = Vector3.Zero;

            foreach (var entity in MSEntity.CurrentSelection.SelectedEntities)
            {
                avgPos += entity.GetComponent<Transform>()!.Position;
            }

            avgPos /= MSEntity.CurrentSelection.SelectedEntities.Count;

            sv1.RenderSurfaceControl.FocusPosition(avgPos);
            sv2.RenderSurfaceControl.FocusPosition(avgPos);
            sv3.RenderSurfaceControl.FocusPosition(avgPos);
            sv4.RenderSurfaceControl.FocusPosition(avgPos);
        }
    }
}
