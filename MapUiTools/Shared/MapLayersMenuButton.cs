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
using Avalonia.Metadata;
using Avalonia.Controls;
using Avalonia.Collections;
#else
using System.Windows;
using System.Windows.Markup;
#endif

namespace MapControl.UiTools
{
#if WINUI || UWP
    [ContentProperty(Name = nameof(Layer))]
#elif Avalonia
#else
    [ContentProperty(nameof(Layer))]
#endif
    public class MapLayerItem
    {
        public string Text { get; set; }
#if !Avalonia
        public UIElement Layer { get; set; }
#else
        [Content]
        public Control Layer { get; set; }
#endif
    }

#if WINUI || UWP
    [ContentProperty(Name = nameof(MapLayers))]
#elif Avalonia
#else
    [ContentProperty(nameof(MapLayers))]
#endif
    public class MapLayersMenuButton : MenuButton
    {
#if !Avalonia
        private UIElement selectedLayer;
#else
        private Control selectedLayer;
#endif

#if Avalonia
        static MapLayersMenuButton()
        {
            MapProperty.Changed.AddClassHandler<MapLayersMenuButton>((o, e) => o.InitializeMenu());
        }
#endif

        public MapLayersMenuButton()
            : base("\uE81E")
        {
            ((INotifyCollectionChanged)MapLayers).CollectionChanged += (s, e) => InitializeMenu();
            ((INotifyCollectionChanged)MapOverlays).CollectionChanged += (s, e) => InitializeMenu();
        }

#if !Avalonia
        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
            nameof(Map), typeof(MapBase), typeof(MapLayersMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapLayersMenuButton)o).InitializeMenu()));
#else
        public static readonly AvaloniaProperty<MapBase> MapProperty =
            AvaloniaProperty.Register<MapLayersMenuButton, MapBase>(nameof(Map));
#endif

        public MapBase Map
        {
            get { return (MapBase)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

#if Avalonia
        // [Content] TODO: https://github.com/kekekeks/XamlX/issues/45
#endif
        public Collection<MapLayerItem> MapLayers { get; } = new ObservableCollection<MapLayerItem>();

        public Collection<MapLayerItem> MapOverlays { get; } = new ObservableCollection<MapLayerItem>();

        private void InitializeMenu()
        {
            if (Map != null)
            {
                var menu = CreateMenu();
#if Avalonia
                AvaloniaList<object> menus = new();
#endif
                foreach (var item in MapLayers)
                {
#if !Avalonia
                    menu.Items.Add(CreateMenuItem(item.Text, item.Layer, MapLayerClicked));
#else
                    menus.Add(CreateMenuItem(item.Text, item.Layer, MapLayerClicked));
#endif

                }

                var initialLayer = MapLayers.Select(l => l.Layer).FirstOrDefault();

                if (MapOverlays.Count > 0)
                {
                    if (initialLayer != null)
                    {
#if !Avalonia
                        menu.Items.Add(CreateSeparator());
#else
                        menus.Add(CreateSeparator());
#endif
                    }

                    foreach (var item in MapOverlays)
                    {
#if !Avalonia
                        menu.Items.Add(CreateMenuItem(item.Text, item.Layer, MapOverlayClicked));
#else
                        menus.Add(CreateMenuItem(item.Text, item.Layer, MapOverlayClicked));
#endif
                    }
                }

#if Avalonia
                menu.Items = menus;
#endif

                if (initialLayer != null)
                {
                    SetMapLayer(initialLayer);
                }
            }
        }

        private void MapLayerClicked(object sender, RoutedEventArgs e)
        {
#if !Avalonia
            var item = (FrameworkElement)sender;
            var layer = (UIElement)item.Tag;
#else
            var item = (Control)sender;
            var layer = (Control)item.Tag;
#endif

            SetMapLayer(layer);
        }

        private void MapOverlayClicked(object sender, RoutedEventArgs e)
        {
#if !Avalonia
            var item = (FrameworkElement)sender;
            var layer = (UIElement)item.Tag;
#else
            var item = (Control)sender;
            var layer = (Control)item.Tag;
#endif

            ToggleMapOverlay(layer);
        }

#if !Avalonia
        private void SetMapLayer(UIElement layer)
#else
        private void SetMapLayer(Control layer)
#endif
        {
            if (selectedLayer != layer)
            {
                selectedLayer = layer;
                Map.MapLayer = selectedLayer;
            }

            UpdateCheckedStates();
        }

#if !Avalonia
        private void ToggleMapOverlay(UIElement layer)
#else
        private void ToggleMapOverlay(Control layer)
#endif
        {
            if (Map.Children.Contains(layer))
            {
                Map.Children.Remove(layer);
            }
            else
            {
                int index = 1;

                foreach (var overlay in MapOverlays.Select(l => l.Layer))
                {
                    if (overlay == layer)
                    {
                        Map.Children.Insert(index, layer);
                        break;
                    }

                    if (Map.Children.Contains(overlay))
                    {
                        index++;
                    }
                }
            }

            UpdateCheckedStates();
        }

        private void UpdateCheckedStates()
        {
            foreach (var item in GetMenuItems())
            {
#if !Avalonia
//TODO: After https://github.com/AvaloniaUI/Avalonia/issues/3153
                item.IsChecked = Map.Children.Contains((UIElement)item.Tag);
#endif
            }
        }
    }
}
