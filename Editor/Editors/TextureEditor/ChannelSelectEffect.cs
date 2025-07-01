using Editor.Utilities;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Editor.Editors
{
    public class ChannelSelectEffect : ShaderEffect
    {
        private static PixelShader _pixelShader = new()
        {
            UriSource = ContentHelper.GetPackUri("Resources/TextureEditor/ChannelSelect.cso", typeof(ChannelSelectEffect))
        };

        public static readonly DependencyProperty MipImageProperty = RegisterPixelShaderSamplerProperty(
            nameof(MipImage),
            typeof(ChannelSelectEffect),
            0
        );

        public static readonly DependencyProperty ChannelsProperty = DependencyProperty.Register(
            nameof(Channels),
            typeof(Color),
            typeof(ChannelSelectEffect),
            new PropertyMetadata(Colors.Black, PixelShaderConstantCallback(0))
        );

        public static readonly DependencyProperty StrideProperty = DependencyProperty.Register(
            nameof(Stride),
            typeof(float),
            typeof(ChannelSelectEffect),
            new PropertyMetadata(1.0f, PixelShaderConstantCallback(1))
        );

        public Brush MipImage
        {
            get => (Brush)GetValue(MipImageProperty);
            set => SetValue(MipImageProperty, value);
        }

        public Color Channels
        {
            get => (Color)GetValue(ChannelsProperty);
            set => SetValue(ChannelsProperty, value);
        }

        public float Stride
        {
            get => (float)GetValue(StrideProperty);
            set => SetValue(StrideProperty, value);
        }

        public ChannelSelectEffect()
        {
            PixelShader = _pixelShader;

            UpdateShaderValue(MipImageProperty);
            UpdateShaderValue(ChannelsProperty);
            UpdateShaderValue(StrideProperty);
        }
    }
}
