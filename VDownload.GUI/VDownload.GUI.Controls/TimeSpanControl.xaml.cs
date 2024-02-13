using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VDownload.GUI.Controls
{
    public sealed partial class TimeSpanControl : UserControl
    {
        #region PROPERTIES

        public TimeSpan Value
        {
            get => (TimeSpan)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeSpanControl), new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(ValuePropertyChanged)));

        public TimeSpan Maximum
        {
            get => (TimeSpan)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(TimeSpan), typeof(TimeSpanControl), new PropertyMetadata(TimeSpan.MaxValue, new PropertyChangedCallback(RangePropertyChanged)));

        public TimeSpan Minimum
        {
            get => (TimeSpan)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(TimeSpan), typeof(TimeSpanControl), new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(RangePropertyChanged)));

        #endregion



        #region CONSTRUCTORS

        public TimeSpanControl()
        {
            this.InitializeComponent();
        }

        #endregion



        #region PRIVATE METHODS

        private void UpdateOnChanges()
        {
            if (this.IsLoaded)
            {
                TimeSpan hoursTimeSpan = TimeSpan.FromHours(Hours.Value);
                TimeSpan minutesTimeSpan = TimeSpan.FromMinutes(Minutes.Value);
                TimeSpan secondsTimeSpan = TimeSpan.FromSeconds(Seconds.Value);
                TimeSpan value = secondsTimeSpan + minutesTimeSpan + hoursTimeSpan;
                if (value >= Maximum)
                {
                    Hours.Value = Math.Floor(Maximum.TotalHours);
                    Minutes.Value = Maximum.Minutes;
                    Seconds.Value = Maximum.Seconds;
                }
                else if (value <= Minimum)
                {
                    Hours.Value = Math.Floor(Minimum.TotalHours);
                    Minutes.Value = Minimum.Minutes;
                    Seconds.Value = Minimum.Seconds;
                }
                Value = value;
            }
        }

        private void UpdateOnValueChange()
        {
            if (this.IsLoaded)
            {
                TimeSpan value = Value;
                if (value > Maximum)
                {
                    value = Maximum;
                }
                else if (value < Minimum)
                {
                    value = Minimum;
                }
                Hours.Value = Math.Floor(value.TotalHours);
                Minutes.Value = value.Minutes;
                Seconds.Value = value.Seconds;
            }
        }

        #endregion



        #region EVENT HANDLERS

        private void ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => UpdateOnChanges();

        private static void ValuePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) => ((TimeSpanControl)obj).UpdateOnValueChange();

        private static void RangePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) => ((TimeSpanControl)obj).UpdateOnChanges();

        private void Control_Loaded(object sender, RoutedEventArgs e) => UpdateOnValueChange();

        #endregion
    }
}
