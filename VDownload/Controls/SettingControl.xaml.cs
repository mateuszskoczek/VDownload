using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VDownload.Controls
{
    public sealed partial class SettingControl : UserControl
    {
        #region CONSTRUCTORS

        public SettingControl()
        {
            this.InitializeComponent();
        }

        #endregion



        #region PROPERTIES

        // ICON
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(SettingControl), new PropertyMetadata(null));
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        // TITLE
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SettingControl), new PropertyMetadata(string.Empty));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // DESCRIPTION
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(SettingControl), new PropertyMetadata(string.Empty));
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        // SETTING CONTENT
        public FrameworkElement SettingContent { get; set; }

        #endregion
    }
}
