using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AttachedCommandBehavior
{
    /// <summary>
    /// Defines a Command Binding
    /// This inherits from freezable so that it gets inheritance context for DataBinding to work
    /// </summary>
    [Obsolete("Нигде не используется")]
    public class BehaviorBinding : Freezable
    {
        CommandBehaviorBinding behavior;
        /// <summary>
        /// Stores the Command Behavior Binding
        /// </summary>
        internal CommandBehaviorBinding Behavior
        {
            get
            {
                if (behavior == null)
                    behavior = new CommandBehaviorBinding();
                return behavior;
            }
        }

        DependencyObject owner;
        /// <summary>
        /// Gets or sets the Owner of the binding
        /// </summary>
        public DependencyObject Owner
        {
            get { return owner; }
            set
            {
                owner = value;
                ResetEventBinding();
            }
        }

        #region Command

        /// <summary>
        /// Command Dependency Property
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(BehaviorBinding),
                new FrameworkPropertyMetadata((ICommand)null,
                    new PropertyChangedCallback(OnCommandChanged)));

        /// <summary>
        /// Gets or sets the Command property.  
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Command property.
        /// </summary>
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BehaviorBinding)d).OnCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Command property.
        /// </summary>
        protected virtual void OnCommandChanged(DependencyPropertyChangedEventArgs e)
        {
            Behavior.Command = Command;
        }

        #endregion

        #region Action

        /// <summary>
        /// Action Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register("Action", typeof(Action<object>), typeof(BehaviorBinding),
                new FrameworkPropertyMetadata((Action<object>)null,
                    new PropertyChangedCallback(OnActionChanged)));

        /// <summary>
        /// Gets or sets the Action property. 
        /// </summary>
        public Action<object> Action
        {
            get { return (Action<object>)GetValue(ActionProperty); }
            set { SetValue(ActionProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Action property.
        /// </summary>
        private static void OnActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BehaviorBinding)d).OnActionChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Action property.
        /// </summary>
        protected virtual void OnActionChanged(DependencyPropertyChangedEventArgs e)
        {
            Behavior.Action = Action;
        }

        #endregion

        #region CommandParameter

        /// <summary>
        /// CommandParameter Dependency Property
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(BehaviorBinding),
                new FrameworkPropertyMetadata((object)null,
                    new PropertyChangedCallback(OnCommandParameterChanged)));

        /// <summary>
        /// Gets or sets the CommandParameter property.  
        /// </summary>
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CommandParameter property.
        /// </summary>
        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BehaviorBinding)d).OnCommandParameterChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CommandParameter property.
        /// </summary>
        protected virtual void OnCommandParameterChanged(DependencyPropertyChangedEventArgs e)
        {
            Behavior.CommandParameter = CommandParameter;
        }

        #endregion

        #region Event

        /// <summary>
        /// Event Dependency Property
        /// </summary>
        public static readonly DependencyProperty EventProperty =
            DependencyProperty.Register("Event", typeof(string), typeof(BehaviorBinding),
                new FrameworkPropertyMetadata((string)null,
                    new PropertyChangedCallback(OnEventChanged)));

        /// <summary>
        /// Gets or sets the Event property.  
        /// </summary>
        public string Event
        {
            get { return (string)GetValue(EventProperty); }
            set { SetValue(EventProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Event property.
        /// </summary>
        private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BehaviorBinding)d).OnEventChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Event property.
        /// </summary>
        protected virtual void OnEventChanged(DependencyPropertyChangedEventArgs e)
        {
            ResetEventBinding();
        }

        #endregion

        static void OwnerReset(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BehaviorBinding)d).ResetEventBinding();
        }

        private void ResetEventBinding()
        {
            if (Owner != null) //only do this when the Owner is set
            {
                //check if the Event is set. If yes we need to rebind the Command to the new event and unregister the old one
                if (Behavior.Event != null && Behavior.Owner != null)
                    Behavior.Dispose();

                //bind the new event to the command
                Behavior.BindEvent(Owner, Event);
            }
        }

        /// <summary>
        /// This is not actually used. This is just a trick so that this object gets WPF Inheritance Context
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
    }
}
