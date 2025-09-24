using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Editor.Editors;

/// <summary>
/// Interaction logic for GeometryView.xaml
/// </summary>
public partial class GeometryView : UserControl
{
    private static readonly Lock _lock = new();

    private Point _clickedPosition;
    private bool _capturedLeft = false;
    private bool _capturedRight = false;

    public GeometryView()
    {
        lock (_lock)
        {
            InitializeComponent();
        }

        DataContextChanged += (s, e) => SetGeometry();
    }

    /// <summary>
    /// Set the geometry to be viewd.
    /// </summary>
    /// <param name="index">Index of a specific mesh to view. Displays all meshes if set to -1 (default).</param>
    public void SetGeometry(int index = -1)
    {
        if (DataContext is not MeshRenderer vm) return;

        if (vm.Meshes.Any() && VpViewport.Children.Count == 2) VpViewport.Children.RemoveAt(1);

        var meshIndex = 0;
        var modelGroup = new Model3DGroup();

        foreach (var mesh in vm.Meshes)
        {
            if (index != -1 && meshIndex != index)
            {
                ++meshIndex;
                continue;
            }

            var mesh3D = new MeshGeometry3D()
            {
                Positions = mesh.Positions,
                Normals = mesh.Normals,
                TriangleIndices = mesh.Indices,
                TextureCoordinates = mesh.UVs,
            };

            var diffuse = new DiffuseMaterial(mesh.Diffuse);
            var specular = new SpecularMaterial(mesh.Specular, 50);
            var matGroup = new MaterialGroup();

            matGroup.Children.Add(diffuse);
            matGroup.Children.Add(specular);

            var model = new GeometryModel3D(mesh3D, matGroup);
            modelGroup.Children.Add(model);

            var binding = new Binding(nameof(mesh.Diffuse)) { Source = mesh };
            BindingOperations.SetBinding(diffuse, DiffuseMaterial.BrushProperty, binding);

            if (meshIndex == index) break;
        }

        var visual = new ModelVisual3D() { Content = modelGroup };
        VpViewport.Children.Add(visual);
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _clickedPosition = e.GetPosition(this);
        _capturedLeft = true;

        Mouse.Capture(sender as UIElement);
    }

    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_capturedLeft && !_capturedRight) return;

        var pos = e.GetPosition(this);
        var d = pos - _clickedPosition;

        if (_capturedLeft && !_capturedRight)
        {
            MoveCamera(d.X, d.Y, 0);
        }
        else if (!_capturedLeft && _capturedRight)
        {
            var vm = (MeshRenderer)DataContext;
            var cp = vm.CameraPosition;
            var yOffset = d.Y * 0.001 * Math.Sqrt(cp.X * cp.X + cp.Z * cp.Z);

            vm.CameraTraget = new Point3D(vm.CameraTraget.X, vm.CameraTraget.Y + yOffset, vm.CameraTraget.Z);
        }

        _clickedPosition = pos;
    }

    private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _capturedLeft = false;

        if (!_capturedRight) Mouse.Capture(null);
    }

    private void Grid_MouseWheel(object sender, MouseWheelEventArgs e) => MoveCamera(0, 0, Math.Sign(e.Delta));

    private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _clickedPosition = e.GetPosition(this);
        _capturedRight = true;
        Mouse.Capture(sender as UIElement);
    }

    private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        _capturedRight = false;

        if (!_capturedLeft) Mouse.Capture(null);
    }

    private void MoveCamera(double dx, double dy, int dz)
    {
        var vm = (MeshRenderer)DataContext;
        var v = new Vector3D(vm.CameraPosition.X, vm.CameraPosition.Y, vm.CameraPosition.Z);
        var r = v.Length;
        var theta = Math.Acos(v.Y / r);
        var phi = Math.Atan2(-v.Z, v.X);

        theta -= dy * 0.01;
        phi -= dx * 0.01;
        r *= 1.0 - 0.1 * dz;

        theta = Math.Clamp(theta, 0.0001, Math.PI - 0.0001);
        v.X = r * Math.Sin(theta) * Math.Cos(phi);
        v.Z = -r * Math.Sin(theta) * Math.Sin(phi);
        v.Y = r * Math.Cos(theta);

        vm.CameraPosition = new Point3D(v.X, v.Y, v.Z);
    }
}
