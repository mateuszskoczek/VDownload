using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Controls
{
    public sealed partial class PlaceholderableStackPanel : UserControl
    {
        #region CONSTRUCTORS

        public PlaceholderableStackPanel()
        {
            InitializeComponent();

            IsPlaceholderActive = true;
            StackPanel.Children.Add(Placeholder);
            StackPanel.VerticalAlignment = VerticalAlignment.Center;
        }

        #endregion



        #region PROPERTIES

        private UIElement _Placeholder = new Grid();
        public UIElement Placeholder 
        {
            get => _Placeholder;
            set
            {
                _Placeholder = value;
                if (IsPlaceholderActive)
                {
                    StackPanel.Children.Clear();
                    StackPanel.Children.Add(_Placeholder);
                    StackPanel.VerticalAlignment = VerticalAlignment.Center;
                }
            }
        }
        public double Spacing 
        {
            get => StackPanel.Spacing;
            set => StackPanel.Spacing = value;
        }

        public bool IsPlaceholderActive { get; private set; }

        #endregion



        #region PUBLIC METHODS

        public void Add(UIElement item)
        {
            if (IsPlaceholderActive)
            {
                StackPanel.Children.Clear();
                IsPlaceholderActive = false;
                StackPanel.VerticalAlignment = VerticalAlignment.Stretch;
            }
            StackPanel.Children.Add(item);
        }

        public void Remove(UIElement item)
        {
            StackPanel.Children.Remove(item);
            if (StackPanel.Children.Count == 0)
            {
                StackPanel.Children.Add(_Placeholder);
                StackPanel.VerticalAlignment = VerticalAlignment.Center;
                IsPlaceholderActive = true;
            }
        }

        public void Clear()
        {
            StackPanel.Children.Clear();
            StackPanel.Children.Add(_Placeholder);
            StackPanel.VerticalAlignment = VerticalAlignment.Center;
            IsPlaceholderActive = true;
        }
        
        public UIElement[] GetAllItems() => IsPlaceholderActive ? StackPanel.Children.ToArray() : new UIElement[0];

        #endregion
    }
}
