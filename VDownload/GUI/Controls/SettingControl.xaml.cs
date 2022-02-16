using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VDownload.GUI.Controls
{
    public sealed partial class SettingControl : UserControl
    {
        // INIT
        public SettingControl()
        {
            this.InitializeComponent();
        }

        // SETTING CONTENT
        public FrameworkElement SettingContent { get; set; }

        // ICON
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(SettingControl), new PropertyMetadata(null));
        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
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

        // DESCRIPTION COLOR
        public static readonly DependencyProperty DescriptionColorProperty = DependencyProperty.Register("DescriptionColor", typeof(Brush), typeof(SettingControl), new PropertyMetadata(new SolidColorBrush((Color)Application.Current.Resources["SystemBaseMediumColor"])));
        public Brush DescriptionColor
        {
            get => (Brush)GetValue(DescriptionColorProperty);
            set => SetValue(DescriptionColorProperty, value);
        }
    }
}
