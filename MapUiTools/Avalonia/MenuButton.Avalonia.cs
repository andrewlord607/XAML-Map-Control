using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapControl.UiTools
{
    public class MenuButton : Button, IStyleable
    {
        Type IStyleable.StyleKey => typeof(MenuButton);

        protected MenuButton(string icon)
        {
            Content = icon;
            Click += (_, _) => ContextMenu.Open();
        }

        protected ContextMenu CreateMenu()
        {
            var menu = new ContextMenu();
            ContextMenu = menu;
            return menu;
        }

        protected IEnumerable<MenuItem> GetMenuItems()
        {
            return ContextMenu.Items.OfType<MenuItem>();
        }

        protected static MenuItem CreateMenuItem(string text, object item, EventHandler<RoutedEventArgs> click)
        {
            var menuItem = new MenuItem { Header = text, Tag = item };
            menuItem.Click += click;
            return menuItem;
        }

        protected static Separator CreateSeparator()
        {
            return new Separator();
        }
    }
}