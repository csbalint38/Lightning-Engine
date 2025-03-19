using System.Windows.Controls;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for ComponentsView.xaml
    /// </summary>
    public partial class ComponentsView : UserControl
    {
        public static ComponentsView Instance { get; private set; }

        public ComponentsView()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;
        }
    }
}
