using Editor.Common.Enums;
using Editor.Content;
using Editor.DLLs;
using Editor.DLLs.Descriptors;
using Editor.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Components
{
    /// <summary>
    /// Interaction logic for PrimitiveMeshDialog.xaml
    /// </summary>
    public partial class PrimitiveMeshDialog : Window
    {
        public PrimitiveMeshDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdatePrimitive();
        }

        private void CbPrimitiveType_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePrimitive();
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdatePrimitive();

        private void UpdatePrimitive()
        {
            if (!IsInitialized) return;

            var primitiveType = (PrimitiveMeshType)new EnumValueConverter().ConvertBack(
                CbPrimitiveType.SelectedItem,
                typeof(PrimitiveMeshType)
            );

            var info = new PrimitiveInitInfo() { Type = primitiveType };

            switch (primitiveType)
            {
                case PrimitiveMeshType.PLANE:
                    {
                        info.SegmentX = (int)SldPlaneX.Value;
                        info.SegmentZ = (int)SldPlaneZ.Value;
                        info.Size.X = Value(TbWidthPlane, .001f);
                        info.Size.Y = Value(TbLengthPlane, .001f);
                        break; 
                    }
                case PrimitiveMeshType.CUBE:
                    break;
                case PrimitiveMeshType.UV_SPHERE:
                    break;
                case PrimitiveMeshType.ICO_SPHERE:
                    break;
                case PrimitiveMeshType.CYLINDER:
                    break;
                case PrimitiveMeshType.CAPSULE:
                    break;
                default:
                    break;
            }

            ContentToolsAPI.CreatePrimitiveMesh(new Geometry(), info);
        }

        private float Value(TextBox tb, float min)
        {
            float.TryParse(tb.Text, out var result);

            return Math.Max(result, min);
        }
    }
}
