using Imagin.Core;
using Imagin.Core.Collections.Serialization;
using Imagin.Core.Controls;
using Imagin.Core.Conversion;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using Imagin.Core.Media;
using Imagin.Core.Models;
using Imagin.Core.Numerics;
using Imagin.Core.Storage;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Imagin.Apps.Desktop;

public enum TileTypes { Calendar, Clock, Color, CountDown, Folder, Image, Note, Search }

public class MainViewModel : MainViewModel<MainWindow>
{
    public static readonly ResourceKey<ContextMenu> TileMenuKey = new();

    public static readonly ResourceKey<FrameworkElement> TileStyleKey = new();

    public static readonly Core.ResourceKey TileTemplateKey = new();

    public static readonly Core.ResourceKey TileHeaderTemplateSelectorKey = new();

    public static readonly Core.ResourceKey TileContentTemplateSelectorKey = new();
        
    //...

    public static Screen DefaultScreen => new(new NoteTile()
    {
        Title = "This is a tile",
        Text = "Feel free to move me around...",
        Size = new(256, 256), Position = new(SystemParameters.FullPrimaryScreenWidth / 2d, SystemParameters.FullPrimaryScreenHeight / 2d)
    });

    //...

    bool drawing = false;
    public bool Drawing
    {
        get => drawing;
        set => this.Change(ref drawing, value);
    }

    Screen screen = null;
    public Screen Screen
    {
        get => screen;
        set
        {
            this.Change(ref screen, value);
            this.Changed(() => TaskbarItemDescription);
        }
    }

    public XmlWriter<Screen> Screens => Get.Current<Options>().Screens;

    public string TaskbarItemDescription => Screen != null && Screens != null ? $"{Screens.IndexOf(Screen) + 1} / {Screens.Count}" : string.Empty;

    //...

    public MainViewModel() : base()
    {
        Screens.CollectionChanged += OnScreensChanged;

        if (Screens.Count == 0)
            Screens.Add(DefaultScreen);

        Screen = Screens.ElementAtOrDefault(Get.Current<Options>().Screen)
            ?? Enumerable.FirstOrDefault(Screens);

        OnThemeChanged();
        Get.Current<Options>().ThemeChanged += OnThemeChanged;
    }

    //...

    void OnScreensChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                break;

            case NotifyCollectionChangedAction.Remove:
                if (Screen == e.OldItems[0])
                    Screen = Enumerable.FirstOrDefault(Screens);
                break;
        }
    }

    void OnThemeChanged(object sender, EventArgs<string> e) => OnThemeChanged();

    //...

    public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(Drawing):
                if (Drawing)
                    View.Activate();

                break;

            case nameof(Screen):
                Get.Current<Options>().Screen = Screen.Index - 1;
                break;
        }
    }

    //...

    protected virtual void OnThemeChanged()
    {
        //This doesn't change automatically when the theme does...
        View?.TaskbarIcon?.ContextMenu?.UpdateDefaultStyle();
    }

    //...

    IValueConverter tileNameConverter
        => new SimpleConverter<Type, string>(i => i.GetAttribute<DisplayNameAttribute>().DisplayName);

    public void Draw(DoubleRegion selection)
    {
        var tilePosition
            = new Point2(M.NearestFactor(selection.X, Get.Current<Options>().TileSnap), M.NearestFactor(selection.Y, Get.Current<Options>().TileSnap));
        var tileSize
            = new DoubleSize(M.NearestFactor(selection.Height, Get.Current<Options>().TileSnap), M.NearestFactor(selection.Width, Get.Current<Options>().TileSnap));

        var tileTypes 
            = XAssembly.GetAssembly(nameof(Desktop)).GetDerivedTypes(typeof(Tile), "Imagin.Apps.Desktop", true, true);

        var t = MemberWindow.ShowDialog("NewTile".Translate(), new { Type = TileTypes.Calendar }, out int result, null, Buttons.ContinueCancel);

        if (result == 0)
        {
            Type tileType = null;
            switch (t.Type)
            {
                case TileTypes.Calendar:
                    tileType = typeof(CalendarTile);
                    break;
                case TileTypes.Clock:
                    tileType = typeof(ClockTile);
                    break;
                case TileTypes.Color:
                    tileType = typeof(ColorTile);
                    break;
                case TileTypes.CountDown:
                    tileType = typeof(CountDownTile);
                    break;
                case TileTypes.Folder:
                    tileType = typeof(FolderTile);
                    break;
                case TileTypes.Image:
                    tileType = typeof(ImageTile);
                    break;
                case TileTypes.Note:
                    tileType = typeof(NoteTile);
                    break;
                case TileTypes.Search:
                    tileType = typeof(SearchTile);
                    break;
            }

            var tile = Activator.CreateInstance(tileType) as Tile;
            tile.Position
                = tilePosition;
            tile.Size
                = tileSize;

            Screen.Add(tile);

            if (tile is ImageTile imageTile)
            {
                StorageWindow.Show(out string path, "Browse file or folder...", StorageWindowModes.Open, ImageFormats.Readable.Select(i => i.Extension));
                imageTile.Path = path;
            }
        }

        selection.X = selection.Y = selection.Height = selection.Width = 0;
        Drawing = false;
    }

    //...

    ICommand addScreenCommand;
    public ICommand AddScreenCommand => addScreenCommand ??= new RelayCommand(() =>
    {
        var result = new Screen();
        Screens.Add(result);
        Screen = result;
    });

    ICommand cancelCommand;
    public ICommand CancelCommand => cancelCommand ??= new RelayCommand(() => Drawing = false, () => Drawing);

    ICommand deleteScreenCommand;
    public ICommand DeleteScreenCommand => deleteScreenCommand ??= new RelayCommand<Screen>(i =>
    {
        var result = Dialog.Show("Delete screen", $"Are you sure you want to delete 'Screen {i.Index}'?", DialogImage.Warning, Buttons.YesNo);
        if (result == 0)
            Screens.Remove(i);
    },
    i => i is Screen);

    ICommand drawCommand;
    public ICommand DrawCommand => drawCommand ??= new RelayCommand(() => Drawing = true, () => !Drawing && Screen != null);

    ICommand leftScreenCommand;
    public ICommand LeftScreenCommand => leftScreenCommand ??= new RelayCommand(() =>
    {
        if (Screen != null && Screens.Any<Screen>() && Screens.IndexOf(Screen) > 0)
        {
            var index = Screens.IndexOf(Screen);
            index--;

            if (index < 0)
                return;

            Screen = Screens[index];
        }
    }, () =>
    {
        return true;
        if (Screen != null)
        {
            var index = Screens?.IndexOf(Screen) ?? -1;
            if (index > 0 && index <= Screens.Count - 1)
                return true;
        }
        return false;
    });

    ICommand removeTileCommand;
    public ICommand RemoveTileCommand => removeTileCommand ??= new RelayCommand<Tile>(i => Screen.Remove(i), i => i is Tile);

    ICommand rightScreenCommand;
    public ICommand RightScreenCommand => rightScreenCommand ??= new RelayCommand(() =>
    {
        if (Screen != null && Screens.Any<Screen>() && Screens.IndexOf(Screen) < Screens.Count - 1)
        {
            var index = Screens.IndexOf(Screen);
            index++;

            if (index > Screens.Count - 1)
                return;

            Screen = Screens[index];
        }
    }, () =>
    {
        return true;
        if (Screen != null)
        {
            var index = Screens?.IndexOf(Screen) ?? -1;
            if (index >= 0 && index < Screens.Count - 1)
                return true;
        }
        return false;
    });
        
    ICommand selectScreenCommand;
    public ICommand SelectScreenCommand 
        => selectScreenCommand ??= new RelayCommand<Screen>(i => Screen = i, i => i is Screen);
        
    ICommand showLogWindowCommand;
    public ICommand ShowLogWindowCommand 
        => showLogWindowCommand ??= new RelayCommand(() => new LogWindow(Get.Current<App>().Log, new LogPanel(Get.Current<App>().Log)).Show());

    ICommand showTileOptionsCommand;
    public ICommand ShowTileOptionsCommand 
        => showTileOptionsCommand ??= new RelayCommand<Tile>(i => MemberWindow.ShowDialog("Tile", i), i => i != null);
}