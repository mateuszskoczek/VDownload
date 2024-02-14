using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VDownload.Toolkit.UI.Behaviors
{
    public class EventToCommandBehavior : Behavior<FrameworkElement>
    {
        #region FIELDS

        private Delegate _handler;
        private EventInfo _oldEvent;

        #endregion



        #region PROPERTIES

        public string Event { get { return (string)GetValue(EventProperty); } set { SetValue(EventProperty, value); } }
        public static readonly DependencyProperty EventProperty = DependencyProperty.Register("Event", typeof(string), typeof(EventToCommandBehavior), new PropertyMetadata(null, OnEventChanged));

        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommandBehavior), new PropertyMetadata(null));

        public bool PassArguments { get { return (bool)GetValue(PassArgumentsProperty); } set { SetValue(PassArgumentsProperty, value); } }
        public static readonly DependencyProperty PassArgumentsProperty = DependencyProperty.Register("PassArguments", typeof(bool), typeof(EventToCommandBehavior), new PropertyMetadata(false));

        #endregion



        #region PRIVATE METHODS

        private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EventToCommandBehavior beh = (EventToCommandBehavior)d;

            if (beh.AssociatedObject != null)
            {
                beh.AttachHandler((string)e.NewValue);
            }
        }

        protected override void OnAttached()
        {
            AttachHandler(this.Event);
        }

        private void AttachHandler(string eventName)
        {
            if (_oldEvent != null)
            {
                _oldEvent.RemoveEventHandler(this.AssociatedObject, _handler);
            }

            if (!string.IsNullOrEmpty(eventName))
            {
                EventInfo ei = this.AssociatedObject.GetType().GetEvent(eventName);
                if (ei != null)
                {
                    MethodInfo mi = this.GetType().GetMethod("ExecuteCommand", BindingFlags.Instance | BindingFlags.NonPublic);
                    _handler = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
                    ei.AddEventHandler(this.AssociatedObject, _handler);
                    _oldEvent = ei;
                }
                else
                {
                    throw new ArgumentException(string.Format("The event '{0}' was not found on type '{1}'", eventName, this.AssociatedObject.GetType().Name));
                }
            }
        }

        private void ExecuteCommand(object sender, object e)
        {
            object parameter = this.PassArguments ? e : null;
            if (this.Command != null && this.Command.CanExecute(parameter))
            {
                this.Command.Execute(parameter);
            }
        }

        #endregion
    }
}
