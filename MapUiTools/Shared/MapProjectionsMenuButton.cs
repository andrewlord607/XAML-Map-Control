// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
#elif Avalonia
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.Collections;
#else
using System.Windows;
using System.Windows.Markup;
#endif

namespace MapControl.UiTools
{
#if WINUI || UWP
    [ContentProperty(Name = nameof(Projection))]
#elif Avalonia
#else
    [ContentProperty(nameof(Projection))]
#endif
    public class MapProjectionItem
    {
        public string Text { get; set; }
#if Avalonia
        [Content]
#endif
        public string Projection { get; set; }
    }

#if WINUI || UWP
    [ContentProperty(Name = nameof(MapProjections))]
#elif Avalonia
#else
    [ContentProperty(nameof(MapProjections))]
#endif
    public class MapProjectionsMenuButton : MenuButton
    {
        private string selectedProjection;

        public MapProjectionsMenuButton()
            : base("\uE809")
        {
            ((INotifyCollectionChanged)MapProjections).CollectionChanged += (s, e) => InitializeMenu();
        }

#if Avalonia
        static MapProjectionsMenuButton()
        {
            MapProperty.Changed.AddClassHandler<MapProjectionsMenuButton>((o, e) => o.InitializeMenu());
        }
#endif

#if !Avalonia
        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
            nameof(Map), typeof(MapBase), typeof(MapProjectionsMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapProjectionsMenuButton)o).InitializeMenu()));
#else
        public static readonly AvaloniaProperty<MapBase> MapProperty = AvaloniaProperty.Register<MapProjectionsMenuButton, MapBase>(
            nameof(Map));
#endif

        public MapBase Map
        {
            get { return (MapBase)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

#if Avalonia
        //[Content] TODO: https://github.com/kekekeks/XamlX/issues/45
#endif
        public Collection<MapProjectionItem> MapProjections { get; } = new ObservableCollection<MapProjectionItem>();

        private void InitializeMenu()
        {
            if (Map != null)
            {
                var menu = CreateMenu();
#if Avalonia
                AvaloniaList<object> menus = new();
#endif
                foreach (var item in MapProjections)
                {
#if !Avalonia
                    menu.Items.Add(CreateMenuItem(item.Text, item.Projection, MapProjectionClicked));
#else
                    menus.Add(CreateMenuItem(item.Text, item.Projection, MapProjectionClicked));
#endif
                }

#if Avalonia
                menu.Items = menus;
#endif

                var initialProjection = MapProjections.Select(p => p.Projection).FirstOrDefault();

                if (initialProjection != null)
                {
                    SetMapProjection(initialProjection);
                }
            }
        }

        private void MapProjectionClicked(object sender, RoutedEventArgs e)
        {
#if !Avalonia
            var item = (FrameworkElement)sender;
#else
            var item = (Control)sender;
#endif
            var projection = (string)item.Tag;

            SetMapProjection(projection);
        }

        private void SetMapProjection(string projection)
        {
            if (selectedProjection != projection)
            {
                selectedProjection = projection;
                Map.MapProjection = MapProjection.Factory.GetProjection(selectedProjection);
            }

            UpdateCheckedStates();
        }

        private void UpdateCheckedStates()
        {
            foreach (var item in GetMenuItems())
            {
#if !Avalonia
//TODO: After https://github.com/AvaloniaUI/Avalonia/issues/3153
                item.IsChecked = selectedProjection == (string)item.Tag;
#endif
            }
        }
    }
}
