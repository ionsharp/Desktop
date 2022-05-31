using Imagin.Core;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using Imagin.Core.Numerics;
using Imagin.Core.Storage;
using System;
using System.Windows;

namespace Imagin.Apps.Desktop
{
    public partial class MainWindow : Core.Controls.MainWindow
    {
        public MainWindow() : base() => InitializeComponent();

        //...

        void OnFileOpened(object sender, EventArgs<string> e) 
            => File.Long.Open(e.Value);

        void OnFolderOpened(object sender, EventArgs<string> e) 
            => sender.If<Browser>(i => i.IsReadOnly, i => Computer.OpenInWindowsExplorer(e.Value));

        void OnSelected(DependencyObject sender, RoutedEventArgs<DoubleRegion> e)
            => Get.Current<MainViewModel>().Draw(e.Value);

        //...

        void AddressBox_Refreshed(object sender, RoutedEventArgs e)
        {
            if (sender is AddressBox box)
            {
                if (box.DataContext is FolderTile tile)
                    _ = tile.Browser?.Items.RefreshAsync();
            }
        }

        //...

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (Get.Current<MainViewModel>().Screen != null)
            {
                foreach (var i in Get.Current<MainViewModel>().Screen)
                {
                    if (i.IsSelected)
                        i.IsSelected = false;
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
        }
    }
}