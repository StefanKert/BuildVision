using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace AttachedCommandBehavior
{
    public class CommandBehaviorCollection
    {
        #region Behaviors

        /// <summary>
        /// Behaviors Read-Only Dependency Property
        /// As you can see the Attached readonly property has a name registered different (BehaviorsInternal) than the property name, this is a tricks os that we can construct the collection as we want
        /// Read more about this here http://wekempf.spaces.live.com/blog/cns!D18C3EC06EA971CF!468.entry
        /// </summary>
        private static readonly DependencyPropertyKey BehaviorsPropertyKey
            = DependencyProperty.RegisterAttachedReadOnly("BehaviorsInternal", typeof(BehaviorBindingCollection), typeof(CommandBehaviorCollection),
                new FrameworkPropertyMetadata((BehaviorBindingCollection)null));

        public static readonly DependencyProperty BehaviorsProperty
            = BehaviorsPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the Behaviors property.  
        /// Here we initialze the collection and set the Owner property
        /// </summary>
        public static BehaviorBindingCollection GetBehaviors(DependencyObject d)
        {
            if (d == null)
                throw new InvalidOperationException("The dependency object trying to attach to is set to null");

            BehaviorBindingCollection collection = d.GetValue(CommandBehaviorCollection.BehaviorsProperty) as BehaviorBindingCollection;
            if (collection == null)
            {
                collection = new BehaviorBindingCollection();
                collection.Owner = d;
                SetBehaviors(d, collection);
            }
            return collection;
        }

        /// <summary>
        /// Provides a secure method for setting the Behaviors property.  
        /// This dependency property indicates ....
        /// </summary>
        private static void SetBehaviors(DependencyObject d, BehaviorBindingCollection value)
        {
            d.SetValue(BehaviorsPropertyKey, value);
            INotifyCollectionChanged collection = (INotifyCollectionChanged)value;
            collection.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChanged);
        }

        static void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BehaviorBindingCollection sourceCollection = (BehaviorBindingCollection)sender;
            switch (e.Action)
            {
                //when an item(s) is added we need to set the Owner property implicitly
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (BehaviorBinding item in e.NewItems)
                            item.Owner = sourceCollection.Owner;
                    break;
                //when an item(s) is removed we should Dispose the BehaviorBinding
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (BehaviorBinding item in e.OldItems)
                            item.Behavior.Dispose();
                    break;

                //here we have to set the owner property to the new item and unregister the old item
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null)
                        foreach (BehaviorBinding item in e.NewItems)
                            item.Owner = sourceCollection.Owner;

                    if (e.OldItems != null)
                        foreach (BehaviorBinding item in e.OldItems)
                            item.Behavior.Dispose();
                    break;

                //when an item(s) is removed we should Dispose the BehaviorBinding
                case NotifyCollectionChangedAction.Reset:
                    if (e.OldItems != null)
                        foreach (BehaviorBinding item in e.OldItems)
                            item.Behavior.Dispose();
                    break;

                case NotifyCollectionChangedAction.Move:
                default:
                    break;
            }
        }

        #endregion

    }

    /// <summary>
    /// Collection to store the list of behaviors. This is done so that you can intiniate it from XAML
    /// This inherits from freezable so that it gets inheritance context for DataBinding to work
    /// </summary>
    public class BehaviorBindingCollection : FreezableCollection<BehaviorBinding>
    {
        /// <summary>
        /// Gets or sets the Owner of the binding
        /// </summary>
        public DependencyObject Owner { get; set; }
    }
}