using Editor.Common.Enums;
using Editor.DLLs;
using Editor.DLLs.Descriptors;
using Editor.Editors;
using Editor.GameProject;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.Content
{
    /// <summary>
    /// Interaction logic for PrimitiveMeshDialog.xaml
    /// </summary>
    public partial class PrimitiveMeshDialog : Window
    {
        private static readonly List<ImageBrush> _textures = [];

        static PrimitiveMeshDialog()
        {
            LoadTextures();
        }

        public PrimitiveMeshDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdatePrimitive();
        }

        private static void LoadTextures()
        {
            var uris = new List<Uri>
            {
                new("pack://application:,,,/Resources/PlaneTexture.png"),
                new("pack://application:,,,/Resources/PlaneTexture.png"),
                new("pack://application:,,,/Resources/SphereUvChecker.png"),
            };

            _textures.Clear();

            foreach (var uri in uris)
            {
                var resource = Application.GetResourceStream(uri);
                using var reader = new BinaryReader(resource.Stream);
                var data = reader.ReadBytes((int)resource.Stream.Length);
                var imageSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(data);

                imageSource.Freeze();

                var brush = new ImageBrush(imageSource)
                {
                    Transform = new ScaleTransform(1, -1, 0.5, 0.5),
                    ViewportUnits = BrushMappingMode.Absolute
                };

                brush.Freeze();
                _textures.Add(brush);
            }
        }

        private void CbPrimitiveType_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePrimitive();
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdatePrimitive();

        private void UpdatePrimitive()
        {
            if (!IsInitialized) return;

            var primitiveType = (PrimitiveMeshType)CbPrimitiveType.SelectedItem;
            var info = new PrimitiveInitInfo() { Type = primitiveType };
            var smoothingAngle = 0;

            switch (primitiveType)
            {
                case PrimitiveMeshType.PLANE:
                    {
                        info.SegmentX = (int)SldPlaneX.Value;
                        info.SegmentZ = (int)SldPlaneZ.Value;
                        info.Size.X = Value(TbWidthPlane, .001f);
                        info.Size.Z = Value(TbLengthPlane, .001f);
                    }
                    break;
                case PrimitiveMeshType.CUBE:
                    return;
                case PrimitiveMeshType.UV_SPHERE:
                    {
                        info.SegmentX = (int)SldUvSphereX.Value;
                        info.SegmentY = (int)SldUvSphereY.Value;
                        info.Size.X = Value(TbXUvSphere, 0.001f);
                        info.Size.Y = Value(TbYUvSphere, 0.001f);
                        info.Size.Z = Value(TbZUvSphere, 0.001f);
                        smoothingAngle = (int)SldSmoothingAngle.Value;
                    }
                    break;
                case PrimitiveMeshType.ICO_SPHERE:
                    return;
                case PrimitiveMeshType.CYLINDER:
                    return;
                case PrimitiveMeshType.CAPSULE:
                    return;
                default:
                    return;
            }

            var geometry = new Geometry();
            geometry.ImportSettings.SmoothingAngle = smoothingAngle;

            ContentToolsAPI.CreatePrimitiveMesh(geometry, info);

            (DataContext as GeometryEditor).SetAsset(geometry);
            CBTexture_Click(CBTexture, null);
        }

        private float Value(TextBox tb, float min)
        {
            float.TryParse(tb.Text, out var result);

            return Math.Max(result, min);
        }

        private void CBTexture_Click(object sender, RoutedEventArgs e)
        {
            Brush brush = Brushes.White;

            if ((sender as CheckBox).IsChecked == true) brush = _textures[(int)CbPrimitiveType.SelectedItem];

            var vm = DataContext as GeometryEditor;

            foreach (var mesh in vm.MeshRenderer.Meshes) mesh.Diffuse = brush;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                InitialDirectory = Project.Current.ContentPath,
                Filter = "Asset file (*.lngasset)|*.lngasset",
            };

            if (dialog.ShowDialog() == true)
            {
                Debug.Assert(!string.IsNullOrEmpty(dialog.FileName));

                var asset = (DataContext as IAssetEditor).Asset;

                Debug.Assert(asset is not null);

                asset.Save(dialog.FileName);
            }
        }
    }
}
