// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#elif UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#elif Avalonia
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Controls.Generators;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#endif

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public partial class MapItem : ListBoxItem
    {
        #if !Avalonia
        public static readonly DependencyProperty LocationMemberPathProperty = DependencyProperty.Register(
            nameof(LocationMemberPath), typeof(string), typeof(MapItem),
            new PropertyMetadata(null, (o, e) => BindingOperations.SetBinding(
                o, LocationProperty, new Binding { Path = new PropertyPath((string)e.NewValue) })));
#else
        public static readonly AvaloniaProperty<string> LocationMemberPathProperty = AvaloniaProperty.Register<MapItem, string>(
            nameof(LocationMemberPath));
        #endif

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get { return (bool)GetValue(AutoCollapseProperty); }
            set { SetValue(AutoCollapseProperty, value); }
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        /// <summary>
        /// Path to a source property for binding the Location property.
        /// </summary>
        public string LocationMemberPath
        {
            get { return (string)GetValue(LocationMemberPathProperty); }
            set { SetValue(LocationMemberPathProperty, value); }
        }

#if Avalonia
        static MapItem()
        {
            LocationMemberPathProperty.Changed.AddClassHandler<MapItem>(
                (o, e) =>
                {
                    o.Bind(LocationProperty, new Binding {Path = (string)e.NewValue});
                });
        }
#endif
    }

#if Avalonia
    public class MapItemContainerGenerator : ItemContainerGenerator<MapItem>
    {
        public MapItemContainerGenerator(MapItemsControl owner) : base(owner, ContentControl.ContentProperty, ContentControl.ContentTemplateProperty)
        {
            Owner = owner;
        }

        public new MapItemsControl Owner { get; }

        protected override IControl CreateContainer(object item)
        {
            var mapItem = (MapItem)base.CreateContainer(item);
            mapItem.ParentControl = Owner;
            return mapItem;
        }
    }
#endif

    /// <summary>
    /// Manages a collection of selectable items on a Map.
    /// </summary>
    public partial class MapItemsControl : ListBox
    {
#if!Avalonia
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MapItem;
        }
#else
        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new MapItemContainerGenerator(this);
        }
#endif

#if !Avalonia
        public void SelectItems(Predicate<object> predicate)
#else
        public void SelectItems(Predicate<int> predicate)
#endif
        {
            if (SelectionMode == SelectionMode.Single)
            {
                throw new InvalidOperationException("SelectionMode must not be Single");
            }

            #if Avalonia
            var i = 0;
            #endif
            foreach (var item in Items)
            {
#if !Avalonia
                var selected = predicate(item);
#else
                var selected = predicate(i);
#endif

                if (selected != SelectedItems.Contains(item))
                {
                    if (selected)
                    {
                        SelectedItems.Add(item);
                    }
                    else
                    {
                        SelectedItems.Remove(item);
                    }
                }
#if Avalonia
                i++;
#endif
            }
        }

        public void SelectItemsByLocation(Predicate<Location> predicate)
        {
            SelectItems(item =>
            {
#if !Avalonia
                var loc = MapPanel.GetLocation(ContainerFromItem(item));
#else
                var loc = MapPanel.GetLocation((Control)ItemContainerGenerator.ContainerFromIndex(item));
#endif
                return loc != null && predicate(loc);
            });
        }

        public void SelectItemsByPosition(Predicate<Point> predicate)
        {
            SelectItems(item =>
            {
#if !Avalonia
                var pos = MapPanel.GetViewPosition(ContainerFromItem(item));
#else
                var pos = MapPanel.GetViewPosition((Control)ItemContainerGenerator.ContainerFromIndex(item));
#endif
                return pos.HasValue && predicate(pos.Value);
            });
        }

        public void SelectItemsInRect(Rect rect)
        {
            SelectItemsByPosition(p => rect.Contains(p));
        }

#if !Avalonia
        protected internal void OnItemClicked(FrameworkElement mapItem, bool controlKey, bool shiftKey)
#else
        protected internal void OnItemClicked(Control mapItem, bool controlKey, bool shiftKey)
#endif
        {
#if !Avalonia
            var item = ItemFromContainer(mapItem);
#else
            var i = 0;
            object item = null;
            var index = ItemContainerGenerator.IndexFromContainer(mapItem);
            foreach (var insideItem in Items)
            {
                if (i == index)
                {
                    item = insideItem;
                }

                i++;
            }
#endif
            
            if (SelectionMode == SelectionMode.Single)
            {
                // Single -> set only SelectedItem

                if (SelectedItem != item)
                {
                    SelectedItem = item;
                }
                else if (controlKey)
                {
                    SelectedItem = null;
                }
            }
            else if (SelectionMode == SelectionMode.Multiple || controlKey)
            {
                // Multiple or Extended with Ctrl -> toggle item in SelectedItems

                if (SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                }
                else
                {
                    SelectedItems.Add(item);
                }
            }
            else if (shiftKey && SelectedItem != null)
            {
                // Extended with Shift -> select items in view rectangle

#if !Avalonia
                var p1 = MapPanel.GetViewPosition(ContainerFromItem(SelectedItem));
#else
                var p1 = MapPanel.GetViewPosition((Control)ItemContainerGenerator.ContainerFromIndex(SelectedIndex));
#endif
                var p2 = MapPanel.GetViewPosition(mapItem);

                if (p1.HasValue && p2.HasValue)
                {
                    SelectItemsInRect(new Rect(p1.Value, p2.Value));
                }
            }
            else if (SelectedItem != item)
            {
                // Extended without Control or Shift -> set selected item

                SelectedItem = item;
            }
        }
    }
}
