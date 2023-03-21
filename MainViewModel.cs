using Imagin.Core;
using Imagin.Core.Analytics;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Collections.Serialization;
using Imagin.Core.Controls;
using Imagin.Core.Conversion;
using Imagin.Core.Effects;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using Imagin.Core.Media;
using Imagin.Core.Models;
using Imagin.Core.Numerics;
using Imagin.Core.Reflection;
using Imagin.Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace Imagin.Apps.Desktop;

[Menu(typeof(Menu), typeof(TileMenu))]
public class MainViewModel : MainViewModel<MainWindow>
{
    enum Category { Add, Properties, Resize, Rotate, Set }

    [Menu]
    enum Menu
    {
        [MenuItem(Icon = SmallImages.Arrange)]
        Arrange,
        [MenuItem(Icon = SmallImages.Pencil)]
        Edit,
        [MenuItem(Icon = SmallImages.Fx)]
        Effects,
        [MenuItem(Icon = SmallImages.Open)]
        Tile,
        [MenuItem(Icon = SmallImages.Transform)]
        Transform,
        [MenuItem(Icon = SmallImages.Computer)]
        Screen,
        [MenuItem(Icon = SmallImages.Selection)]
        Select,
    }

    [Menu(Parent = Menu.Tile)]
    enum TileMenu
    {
        [MenuItem(Category = Category.Properties, Icon = SmallImages.Color)]
        Background,
        [MenuItem(Category = Category.Properties, Icon = SmallImages.Color)]
        Foreground,
    }

    #region Keys

    public static readonly Core.ResourceKey ScreenTemplateKey = new();

    public static readonly Core.ResourceKey TileMenuKey = new();

    public static readonly Core.ResourceKey TileStyleKey = new();

    public static readonly Core.ResourceKey TileTemplateKey = new();

    public static readonly Core.ResourceKey TileContentTemplateSelectorKey = new();

    #endregion

    #region Properties

    public bool AnySelectedTiles => SelectedTiles?.Any() == true;

    ///

    public static Point2 CenterScreen => new(ScreenWidth / 2.0, ScreenHeight / 2.0);

    public static double ScreenHeight => SystemParameters.FullPrimaryScreenHeight;

    public static double ScreenWidth => SystemParameters.FullPrimaryScreenWidth;

    ///

    public static Screen DefaultScreen
    {
        get
        {
            var fullSize = new DoubleSize(128, 512);
            var halfSize = fullSize / 2.0;

            return new(new NoteTile()
            {
                FontAlignment = TextAlignment.Center,
                FontSize = 24,
                Title = "This is a tile",
                Text = "Feel free to move me around...",
                Size = fullSize,
                Position = new(CenterScreen.X - halfSize.Width, CenterScreen.Y - halfSize.Height)
            });
        }
    }

    ///

    public ObservableCollection<ImageEffect> EffectSource { get => Get(new ObservableCollection<ImageEffect>()); set => Set(value); }

    public ListCollectionView EffectView { get => Get<ListCollectionView>(); set => Set(value); }

    public History History { get; set; } = new();

    ///

    public bool Drawing { get => Get(false); set => Set(value); }

    public Func<bool> IsDesktopActive => Windows.Desktop.IsActive;

    public bool? IsLocked
    {
        get
        {
            var result = true;
            SelectedTiles.ForEach(i =>
            {
                if (!result)
                    return;

                if (!i.IsLocked)
                    result = false;
            });

            if (result)
                return true;

            result = true;
            SelectedTiles.ForEach(i =>
            {
                if (!result)
                    return;

                if (i.IsLocked)
                    result = false;
            });

            return result ? false : null;
        }
    }

    public Screen Screen { get => Get<Screen>(); set => Set(value); }

    public XmlWriter<Screen> Screens => Current.Get<Options>().Screens;

    public IEnumerable<Tile> SelectedTiles => Screen?.Where(i => i.IsSelected);

    public double SelectedTilesOpacity
    {
        get => SelectedTiles?.FirstOrDefault()?.Opacity ?? 0;
        set => SelectedTiles.If(i => i?.Any() == true, i => i.ForEach(j => j.Opacity = value));
    }
    
    public ListCollectionView Shapes
    {
        get
        {
            var result = new ListCollectionView(Current.Get<Options>().Shapes);
            result.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NamableCategory<Core.Media.Shape>.Category)));
            result.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(NamableCategory<Core.Media.Shape>.Name), System.ComponentModel.ListSortDirection.Ascending));
            return result;
        }
    }

    public string TaskbarItemDescription => Screen != null && Screens != null ? $"{Screens.IndexOf(Screen) + 1} / {Screens.Count}" : string.Empty;


    #endregion

    #region MainViewModel

    public MainViewModel() : base()
    {

        ///Effects

        EffectSource = new ObservableCollection<ImageEffect>();
        EffectCollection.Types.ForEach(i =>
        {
            ImageEffect result = null;
            Try.Invoke(() => result = i.Create<ImageEffect>(), e => Core.Analytics.Log.Write<MainViewModel>(e));
            result.If(i => EffectSource.Add(i));
        });

        EffectView = new(EffectSource);
        EffectView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ImageEffect.Category)));
        EffectView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ImageEffect.Category), System.ComponentModel.ListSortDirection.Ascending));
        EffectView.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ImageEffect.Name), System.ComponentModel.ListSortDirection.Ascending));
        EffectView.Refresh();

        ///

        Screens.CollectionChanged += OnScreensChanged;

        if (Screens.Count == 0)
            Screens.Add(DefaultScreen);

        Screen = Screens.ElementAtOrDefault(Current.Get<Options>().Screen)
            ?? Enumerable.FirstOrDefault(Screens);

    }

    #endregion

    #region Methods

    void OnScreensChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Update(() => TaskbarItemDescription);
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

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(Drawing):
                if (Drawing)
                    View.Activate();

                break;

            case nameof(Screen):
                Current.Get<Options>().Screen = Screen.Index - 1;
                Update(() => TaskbarItemDescription);
                break;
        }
    }

    #endregion

    #region Menu

    #region Arrange

    Dictionary<int, string> ArrangeNames = new()
    {
        { 0, "Left to right, Top to bottom" },
        { 1, "Right to left, Top to bottom" },
        { 2, "Left to right, Bottom to top" },
        { 3, "Right to left, Bottom to top" },
        { 4, "Top to bottom, Left to right" },
        { 5, "Top to bottom, Right to left" },
        { 6, "Bottom to top, Left to right" },
        { 7, "Bottom to top, Right to left" },
    };

    void Arrange(int type)
    {
        var result = new ArrangeForm();
        Dialog.ShowObject($"Arrange ({ArrangeNames[type]})", result, Resource.GetImageUri(SmallImages.Arrange), choice =>
        {
            if (choice == 0)
            {
                double x = 0, y = 0;
                switch (type)
                {
                    case 0:
                    case 4:
                        x = result.Region.X; y = result.Region.Y;
                        break;
                    case 1:
                    case 5:
                        x = result.Region.X + result.Region.Width; y = result.Region.Y;
                        break;
                    case 2:
                    case 6:
                        x = result.Region.X; y = result.Region.Y + result.Region.Height;
                        break;
                    case 3:
                    case 7:
                        x = result.Region.X + result.Region.Width; y = result.Region.Y + result.Region.Height;
                        break;
                }

                double yMax = double.MinValue, yMin = double.MaxValue;
                for (var i = 0; i < SelectedTiles.Count(); i++)
                {
                    var tile = SelectedTiles.ElementAt(i);
                    var tileNext = SelectedTiles.ElementAt(i + 1);

                    tile.Position.X = x; tile.Position.Y = y;

                    var p = tile.Position.Y + tile.Size.Height;
                    yMax = p > yMax ? p : yMax;

                    var q = tile.Position.Y - tile.Size.Height;
                    yMin = q < yMin ? q : yMin;

                    switch (type)
                    {
                        //Left to right, Top to bottom
                        case 0:
                            x += tile.Size.Width + result.Spacing;
                            if (x + tileNext.Size.Width > result.Region.X + result.Region.Width)
                            {
                                x = result.Region.X;
                                y = yMax;
                            }
                            break;

                        //Right to left, Top to bottom
                        case 1:
                            break;

                        //Left to right, Bottom to top
                        case 2:
                            x += tile.Size.Width + result.Spacing;
                            if (x + tileNext.Size.Width > result.Region.X + result.Region.Width)
                            {
                                x = result.Region.X;
                                y = yMin;
                            }
                            break;

                        //Right to left, Bottom to top
                        case 3:
                            x = result.Region.X + result.Region.Width; y = result.Region.Y;
                            break;

                        //Top to bottom, Left to right
                        case 4:
                            break;

                        //Top to bottom, Right to left
                        case 5:
                            x = result.Region.X; y = result.Region.Y + result.Region.Height;
                            break;

                        //Bottom to top, Left to right
                        case 6:
                            break;

                        //Bottom to top, Right to left
                        case 7:
                            x = result.Region.X + result.Region.Width; y = result.Region.Y + result.Region.Height;
                            break;
                    }
                }
            }
        }, 
        Buttons.ContinueCancel);
    }

    ICommand arrangeLeftRightTopBottomCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 0, Header = "Left to right, Top to bottom", Icon = SmallImages.ArrowRightDown)]
    public ICommand ArrangeLeftRightTopBottomCommand => arrangeLeftRightTopBottomCommand ??= new RelayCommand(() => 
        Arrange(0), () => AnySelectedTiles);

    ICommand arrangeRightLeftTopBottomCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 0, Header = "Right to left, Top to bottom", Icon = SmallImages.ArrowLeftDown)]
    public ICommand ArrangeRightLeftTopBottomCommand => arrangeRightLeftTopBottomCommand ??= new RelayCommand(() =>
        Arrange(1), () => AnySelectedTiles);

    ICommand arrangeLeftRightBottomTopCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 1, Header = "Left to right, Bottom to top", Icon = SmallImages.ArrowRightUp)]
    public ICommand ArrangeLeftRightBottomTopCommand => arrangeLeftRightBottomTopCommand ??= new RelayCommand(() =>
        Arrange(2), () => AnySelectedTiles);

    ICommand arrangeRightLeftBottomTopCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 1, Header = "Right to left, Bottom to top", Icon = SmallImages.ArrowLeftUp)]
    public ICommand ArrangeRightLeftBottomTopCommand => arrangeRightLeftBottomTopCommand ??= new RelayCommand(() =>
        Arrange(3), () => AnySelectedTiles);

    ICommand arrangeTopBottomLeftRightCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 2, Header = "Top to bottom, Left to right", Icon = SmallImages.ArrowDownRight)]
    public ICommand ArrangeTopBottomLeftRightCommand => arrangeTopBottomLeftRightCommand ??= new RelayCommand(() =>
        Arrange(4), () => AnySelectedTiles);

    ICommand arrangeTopBottomRightLeftCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 2, Header = "Top to bottom, Right to left", Icon = SmallImages.ArrowDownLeft)]
    public ICommand ArrangeTopBottomRightLeftCommand => arrangeTopBottomRightLeftCommand ??= new RelayCommand(() =>
        Arrange(5), () => AnySelectedTiles);

    ICommand arrangeBottomTopLeftRightCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 3, Header = "Bottom to top, Left to right", Icon = SmallImages.ArrowUpRight)]
    public ICommand ArrangeBottomTopLeftRightCommand => arrangeBottomTopLeftRightCommand ??= new RelayCommand(() =>
        Arrange(6), () => AnySelectedTiles);

    ICommand arrangeBottomTopRightLeftCommand;
    [MenuItem(Parent = Menu.Arrange, SubCategory = 3, Header = "Bottom to top, Right to left", Icon = SmallImages.ArrowUpLeft)]
    public ICommand ArrangeBottomTopRightLeftCommand => arrangeBottomTopRightLeftCommand ??= new RelayCommand(() =>
        Arrange(7), () => AnySelectedTiles);


    #endregion

    #region Edit

    ICommand cutCommand;
    [MenuItem(Parent = Menu.Edit, Header = "Cut", Icon = SmallImages.Copy, Index = 0, SubCategory = 0)]
    public ICommand CutCommand => cutCommand ??= new RelayCommand(() => { }, () => AnySelectedTiles);

    ICommand copyCommand;
    [MenuItem(Parent = Menu.Edit, Header = "Copy", Icon = SmallImages.Cut, Index = 1, SubCategory = 0)]
    public ICommand CopyCommand => copyCommand ??= new RelayCommand(() => Copy.Set(SelectedTiles), () => AnySelectedTiles);

    ICommand cloneCommand;
    [MenuItem(Parent = Menu.Edit, Header = "Clone", Icon = SmallImages.Clone, Index = 2, SubCategory = 0)]
    public ICommand CloneCommand => cloneCommand ??= new RelayCommand(() =>
    {
        for (var i = Screen.Count - 1; i >= 0; i--)
        {
            if (Screen[i].IsSelected)
            {
                var clone = Try.Return(() => Screen[i].DeepClone(), e => Log.Write<Tile>(e));
                if (clone != null)
                {
                    clone.Position.X += M.NearestFactor(32.0, Current.Get<Options>().TileResizeSnap);
                    clone.Position.Y += M.NearestFactor(32.0, Current.Get<Options>().TileResizeSnap);
                    Screen.Add(clone);
                }
            }
        }
    },
    () => SelectedTiles?.Any() == true);

    ICommand pasteCommand;
    [MenuItem(Parent = Menu.Edit, Header = "Paste", Icon = SmallImages.Paste, Index = 3, SubCategory = 0)]
    public ICommand PasteCommand => pasteCommand ??= new RelayCommand(() => { }, () => Copy.Contains<IEnumerable<Tile>>());

    ICommand undoCommand;
    [MenuItem(Parent = Menu.Edit, Header = "Undo", Icon = SmallImages.Undo, SubCategory = 1)]
    public ICommand UndoCommand => undoCommand ??= new RelayCommand(() => Current.Get<History>().Undo());

    ICommand undoActionCommand;
    public ICommand UndoActionCommand => undoActionCommand ??= new RelayCommand(() => { });

    ICommand redoCommand;
    [MenuItem(Parent = Menu.Edit, Header = "Redo", Icon = SmallImages.Redo, SubCategory = 1)]
    public ICommand RedoCommand => redoCommand ??= new RelayCommand(() => Current.Get<History>().Redo());

    [MenuItemCollection(Parent = Menu.Edit, SubCategory = 2,
        Header = "History",
        IsInline = false,

        ItemCommandName = nameof(UndoActionCommand),
        ItemCommandParameterPath = ".",

        ItemHeaderConverter = typeof(CamelCaseConverter),
        ItemHeaderPath = nameof(Change.Title),

        ItemType = typeof(Change))]
    public object _History => Current.Get<History>();

    #endregion

    #region Effects

    ICommand clearEffectsCommand;
    [MenuItem(Parent = Menu.Effects,
        Header = "Clear", Icon = SmallImages.X)]
    public ICommand ClearEffectsCommand => clearEffectsCommand ??= new RelayCommand(() => Dialog.ShowWarning("Clear effects", new("Are you sure you want to clear?"), i => i.If(0, () => SelectedTiles.ForEach(i => i.Effects.Clear())), Buttons.YesNo));

    ICommand editEffectsCommand;
    [MenuItem(Parent = Menu.Effects,
        Header = "Edit", Icon = SmallImages.Pencil)]
    public ICommand EditEffectsCommand => editEffectsCommand ??= new RelayCommand(() =>
    {
        var result = new EditEffectForm(SelectedTiles.First());
        Dialog.ShowObject("Effects", result, Resource.GetImageUri(SmallImages.Pencil));
    },
    () => AnySelectedTiles);

    AddEffectForm effectSelection;

    [MenuItemCollection(Parent = Menu.Effects, Category = Category.Add,
        Header = "Effects",
        Icon = SmallImages.Fx,
        IsInline = true,
        ItemCommandName = nameof(EffectCommand),
        ItemCommandParameterPath = ".",
        ItemHeaderPath = nameof(ImageEffect.Name),
        ItemIcon = SmallImages.Fx,
        ItemType = typeof(ImageEffect))]
    public object _Effects => EffectView;

    #endregion

    #region Screen

    ICommand editScreenCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 0, Header = "Edit", Icon = SmallImages.Pencil)]
    public ICommand EditScreenCommand => editScreenCommand ??= new RelayCommand(() => Dialog.ShowObject("Edit screen", Screen, Resource.GetImageUri(SmallImages.Pencil)), () => Screen != null);

    ///

    ICommand addScreenCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 1, Header = "Add", Icon = SmallImages.Plus)]
    public ICommand AddScreenCommand => addScreenCommand ??= new RelayCommand(() =>
    {
        var result = new AddScreenForm();
        Dialog.ShowObject("Add screen", result, Resource.GetImageUri(SmallImages.Pencil), i =>
        {
            if (i == 0)
            {
                var screen = new Screen() { Name = result.Name };
                Screens.Insert(result.InsertAt, screen);
                Screen = screen;
            }
        },
        Buttons.SaveCancel);
    });

    ICommand removeCurrentScreenCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 1, Header = "Remove current", Icon = SmallImages.Close)]
    public ICommand RemoveCurrentScreenCommand => removeCurrentScreenCommand ??= new RelayCommand(() =>
    {
        var neverShow = new BooleanAccessor(() => !Current.Get<Options>().WarnBeforeRemovingScreen, i => Current.Get<Options>().WarnBeforeRemovingScreen = !i);
        Dialog.ShowWarning("Remove screen", new($"Are you sure you want to remove the current screen?"), neverShow, result =>
        {
            if (result == 0)
            {
                var index = Screens.IndexOf(Screen);
                if (index - 1 >= 0)
                {
                    Screen = Screens[index - 1];
                    Screens.RemoveAt(index);
                }
                else
                {
                    Screens.RemoveAt(index);
                    Screen = Screens.First();
                }
            }
        },
        Buttons.YesNo);
    },
    () => Screen != null && Screens?.Count > 1);

    ICommand removeAllScreensCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 1, Header = "Remove all", Icon = SmallImages.CloseAll)]
    public ICommand RemoveAllScreensCommand => removeAllScreensCommand ??= new RelayCommand(() =>
    {
        var neverShow = new BooleanAccessor(() => !Current.Get<Options>().WarnBeforeRemovingAllScreens, i => Current.Get<Options>().WarnBeforeRemovingAllScreens = !i);
        Dialog.ShowWarning("Remove all screens", new($"Are you sure you want to remove all screens?"), neverShow, result =>
        {
            if (result == 0)
                Screens.Clear();
        },
        Buttons.YesNo);
    },
    () => Screens?.Count > 0);

    ///

    ICommand lockAllScreensCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 2, Header = "Lock all", Icon = SmallImages.Lock)]
    public ICommand LockAllScreensCommand => lockAllScreensCommand ??= new RelayCommand(() => Screens.ForEach(i => i.IsLocked = true), () => Screens?.Count > 0);

    ICommand lockCurrentScreenCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 2, Header = "Lock current", Icon = SmallImages.Lock)]
    public ICommand LockCurrentScreenCommand => lockCurrentScreenCommand ??= new RelayCommand(() => Screen.IsLocked = true, () => Screen != null);

    ICommand unlockAllScreensCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 2, Header = "Unlock all", Icon = SmallImages.Unlock)]
    public ICommand UnlockAllScreensCommand => unlockAllScreensCommand ??= new RelayCommand(() => Screens.ForEach(i => i.IsLocked = false), () => Screens?.Count > 0);

    ICommand unlockCurrentScreenCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 2, Header = "Unlock current", Icon = SmallImages.Unlock)]
    public ICommand UnlockCurrentScreenCommand => unlockCurrentScreenCommand ??= new RelayCommand(() => Screen.IsLocked = false, () => Screen != null);

    ///

    ICommand leftScreenCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 3, Header = "Go left", Icon = SmallImages.ArrowLeft)]
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

    ICommand rightScreenCommand;
    [MenuItem(Parent = Menu.Screen, SubCategory = 3, Header = "Go right", Icon = SmallImages.ArrowRight)]
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

    [MenuItemCollection(Parent = Menu.Screen, SubCategory = 3, Header = "Go to", Icon = SmallImages.Period,
        IsInline = false,
        ItemCommandName = nameof(SelectScreenCommand),
        ItemCommandParameterPath = ".",
        ItemHeaderPath = nameof(Desktop.Screen.Name),
        ItemIcon = SmallImages.Computer,
        ItemType = typeof(Screen))]
    public object _Screens => Screens;

    #endregion

    #region Select

    ICommand selectAllCommand;
    [MenuItem(Parent = Menu.Select, Header = "All", Icon = SmallImages.SelectAll)]
    public ICommand SelectAllCommand => selectAllCommand 
        ??= new RelayCommand(() => Current.Get<History>().Add(Screen, nameof(Tile.IsSelected), i => true), () => Screen?.Count > 0);

    ICommand selectInvertCommand;
    [MenuItem(Parent = Menu.Select, Header = "Inverse", Icon = SmallImages.SelectInvert)]
    public ICommand SelectInvertCommand => selectInvertCommand 
        ??= new RelayCommand(() => Current.Get<History>().Add(Screen, nameof(Tile.IsSelected), i => !i.IsSelected), () => Screen?.Count > 0);

    ICommand selectNoneCommand;
    [MenuItem(Parent = Menu.Select, Header = "None", Icon = SmallImages.SelectNone)]
    public ICommand SelectNoneCommand => selectNoneCommand 
        ??= new RelayCommand(() => Current.Get<History>().Add(Screen, nameof(Tile.IsSelected), i => false), () => Screen?.Count > 0);

    ICommand selectRandomCommand;
    [MenuItem(Parent = Menu.Select, Header = "Random", Icon = SmallImages.Random)]
    public ICommand SelectRandomCommand => selectRandomCommand 
        ??= new RelayCommand(() => Current.Get<History>().Add(Screen, nameof(Tile.IsSelected), i => Core.Numerics.Random.NextBoolean()), () => Screen?.Count > 0);

    #endregion

    #region Tile

    ICommand editTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = -1, Header = "Edit", Icon = SmallImages.Pencil)]
    public ICommand EditTilesCommand => editTilesCommand ??= new RelayCommand(() =>
    {
        //SelectedTiles.ToArray();
        Dialog.ShowObject("Edit", SelectedTiles.First(), Resource.GetImageUri(SmallImages.Pencil));
    },
    () => AnySelectedTiles);

    ///

    ICommand addTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 1, Header = "Add", Icon = SmallImages.Plus)]
    public ICommand AddTilesCommand => addTilesCommand ??= new RelayCommand(() => Drawing = true, () => !Drawing && Screen != null);

    ICommand removeAllTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 1,
        Header = "Remove all", Icon = SmallImages.Minus)]
    public ICommand RemoveAllTilesCommand => removeAllTilesCommand ??= new RelayCommand(() =>
    {
        var neverShow = new BooleanAccessor(() => !Current.Get<Options>().WarnBeforeRemovingAllTiles, i => Current.Get<Options>().WarnBeforeRemovingAllTiles = !i);
        Dialog.ShowWarning("Remove all tiles", new("Are you sure you want to remove all tiles?"), neverShow, result =>
        {
            if (result == 0)
            {
                for (var i = Screen.Count - 1; i >= 0; i--)
                    Screen.RemoveAt(i);
            }
        },
        Buttons.YesNo);
    },
    () => AnySelectedTiles);

    ICommand removeTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 1,
        Header = "Remove selected", Icon = SmallImages.Minus)]
    public ICommand RemoveTilesCommand => removeTilesCommand ??= new RelayCommand(() =>
    {
        var neverShow = new BooleanAccessor(() => !Current.Get<Options>().WarnBeforeRemovingASelectionOfTiles, i => Current.Get<Options>().WarnBeforeRemovingASelectionOfTiles = !i);
        Dialog.ShowWarning("Remove tiles", new("Are you sure you want to remove the selected tiles?"), neverShow, result =>
        {
            if (result == 0)
            {
                for (var i = Screen.Count - 1; i >= 0; i--)
                {
                    if (Screen[i].IsSelected)
                        Screen.RemoveAt(i);
                }
            }
        },
        Buttons.YesNo);
    },
    () => AnySelectedTiles);

    ///

    ICommand lockAllTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 2, Header = "Lock all", Icon = SmallImages.Lock)]
    public ICommand LockAllTilesCommand => lockAllTilesCommand ??= new RelayCommand(() => Screen.ForEach(i => i.IsLocked = true),
        () => Screen?.Count > 0 && Screen.Any(i => !i.IsLocked));

    ICommand lockTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 2, Header = "Lock selected", Icon = SmallImages.Lock)]
    public ICommand LockTilesCommand => lockTilesCommand ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.IsLocked = true), () => AnySelectedTiles && SelectedTiles.Any(i => !i.IsLocked));

    ICommand unlockAllTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 2, Header = "Unlock all", Icon = SmallImages.Unlock)]
    public ICommand UnlockAllTilesCommand => unlockAllTilesCommand ??= new RelayCommand(() => Screen.ForEach(i => i.IsLocked = false),
        () => Screen?.Count > 0 && Screen.Any(i => i.IsLocked));

    ICommand unlockTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 2, Header = "Unlock selected", Icon = SmallImages.Unlock)]
    public ICommand UnlockTilesCommand => unlockTilesCommand ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.IsLocked = false), () => AnySelectedTiles && SelectedTiles.Any(i => i.IsLocked));

    ///

    ICommand hideTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 3, Header = "Hide selected", Icon = SmallImages.Hide)]
    public ICommand HideTilesCommand => hideTilesCommand ??= new RelayCommand(() =>
    {
        for (var i = Screen.Count - 1; i >= 0; i--)
        {
            var tile = Screen[i];
            if (tile.IsSelected)
            {
                Screen.RemoveAt(i);
                Screen.Minimized.Add(tile);
            }
        }
    },
    () => AnySelectedTiles);

    ICommand hideAllTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 3, Header = "Hide all", Icon = SmallImages.HideAll)]
    public ICommand HideAllTilesCommand => hideAllTilesCommand ??= new RelayCommand(() =>
    {
        for (var i = Screen.Count - 1; i >= 0; i--)
        {
            var tile = Screen[i];
            Screen.RemoveAt(i);
            Screen.Minimized.Add(tile);
        }
    },
    () => Screen?.Count > 0);

    ICommand showAllTilesCommand;
    [MenuItem(Parent = Menu.Tile, SubCategory = 3, Header = "Show all", Icon = SmallImages.ShowAll)]
    public ICommand ShowAllTilesCommand => showAllTilesCommand ??= new RelayCommand(() =>
    {
        for (var i = Screen.Minimized.Count - 1; i >= 0; i--)
        {
            var tile = Screen.Minimized[i];
            Screen.Minimized.RemoveAt(i);
            Screen.Add(tile);
        }
    },
    () => Screen?.Minimized.Count > 0);

    ///

    [MenuItemCollection(Parent = TileMenu.Background, Category = Category.Set,
        Header = "Set",

        Icon = SmallImages.Color,

        IsInline = true,

        ItemCommandName = nameof(BackgroundColorCommand),
        ItemCommandParameterPath = ".",

        ItemHeaderConverter = typeof(ByteVector4ToColorNameConverter),
        ItemHeaderPath = ".",

        ItemIconPath = ".",
        ItemIconTemplateSource = typeof(XColor),
        ItemIconTemplateKey = nameof(XColor.IconTemplateKey),

        ItemToolTipPath = ".",
        ItemToolTipTemplateSource = typeof(XColor),
        ItemToolTipTemplateKey = nameof(XColor.ToolTipTemplateKey),

        ItemType = typeof(ByteVector4))]
    public object BackgroundColors => Current.Get<Options>().Colors;

    ICommand resetBackgroundCommand;
    [MenuItem(Parent = TileMenu.Background,
        Header = "Reset", Icon = SmallImages.Reset)]
    public ICommand ResetBackgroundCommand => resetBackgroundCommand ??= new RelayCommand(() => Dialog.ShowWarning("Reset background", new("Are you sure you want to reset?"), i => i.If(0, () => SelectedTiles.ForEach(j => j.Background = null)), Buttons.YesNo));

    [MenuItemCollection(Parent = Menu.Tile, Category = Category.Properties,
        Header = "Blend",
        Icon = SmallImages.Blend,
        IsInline = false,
        ItemCommandName = nameof(BlendCommand),
        ItemCommandParameterPath = ".",
        ItemHeaderConverter = typeof(CamelCaseConverter),
        ItemHeaderPath = ".",
        ItemToolTipPath = ".",
        ItemType = typeof(BlendModes))]
    public object BlendModes => typeof(BlendModes).GetEnumCollection(Core.Appearance.Visible);

    [MenuItemCollection(Parent = TileMenu.Foreground, Category = Category.Set,
        Header = "Set",

        Icon = SmallImages.Color,

        IsInline = true,

        ItemCommandName = nameof(ForegroundColorCommand),
        ItemCommandParameterPath = ".",

        ItemHeaderConverter = typeof(ByteVector4ToColorNameConverter),
        ItemHeaderPath = ".",

        ItemIconPath = ".",
        ItemIconTemplateSource = typeof(XColor),
        ItemIconTemplateKey = nameof(XColor.IconTemplateKey),

        ItemToolTipPath = ".",
        ItemToolTipTemplateSource = typeof(XColor),
        ItemToolTipTemplateKey = nameof(XColor.ToolTipTemplateKey),

        ItemType = typeof(ByteVector4))]
    public object ForegroundColors => Current.Get<Options>().Colors;

    ICommand resetForegroundCommand;
    [MenuItem(Parent = TileMenu.Foreground,
        Header = "Reset", Icon = SmallImages.Reset)]
    public ICommand ResetForegroundCommand => resetForegroundCommand ??= new RelayCommand(() => Dialog.ShowWarning("Reset background", new("Are you sure you want to reset?"), i => i.If(0, () => SelectedTiles.ForEach(j => j.Foreground = null)), Buttons.YesNo));

    ICommand tileOpacityCommand;
    [MenuItem(Parent = Menu.Tile, Category = Category.Properties, Header = "Opacity", Icon = SmallImages.Opacity,
        CanSlide = true,
        SlideHeader = "{0}%",
        SlideMaximum = 1,
        SlideMinimum = 0,
        SlidePath = nameof(SelectedTilesOpacity),
        SlideTick = 0.01)]
    public ICommand TileOpacityCommand => tileOpacityCommand ??= new RelayCommand<double>(i => SelectedTiles.ForEach(j => j.Opacity = i));

    [MenuItemCollection(Parent = Menu.Tile, Category = Category.Properties,

        Header = "Shape",

        Icon = SmallImages.Shape,

        IsInline = false,

        ItemCommandName = nameof(ShapeCommand),
        ItemCommandParameterPath = ".",

        ItemHeaderPath = nameof(NamableCategory<Core.Media.Shape>.Name),

        ItemIconPath = nameof(NamableCategory<Core.Media.Shape>.Value),
        ItemIconTemplateSource = typeof(XShape),
        ItemIconTemplateKey = nameof(XShape.IconTemplateKey),

        /*
        ItemToolTipPath = ".",
        ItemToolTipTemplateKeyType = typeof(XColor),
        ItemToolTipTemplateKeyName = nameof(XColor.ToolTipTemplateKey),
        */

        ItemType = typeof(NamableCategory<Core.Media.Shape>))]
    public object _Shapes => Current.Get<Options>().Shapes;

    #endregion

    #region Transform

    ICommand resizeCommand;
    [MenuItem(Category = Category.Resize, Header = "Resize...", Icon = SmallImages.Resize, Parent = Menu.Transform)]
    public ICommand ResizeCommand => resizeCommand ??= new RelayCommand(() =>
    {
        var result = new ResizeForm(SelectedTiles);
        Dialog.ShowObject("Resize", result, Resource.GetImageUri(SmallImages.Resize), i =>
        {
            if (i == 0)
            {
                SelectedTiles.ForEach(i =>
                {
                    i.Size.Height = M.NearestFactor(result.Height, Current.Get<Options>().TileResizeSnap);
                    i.Size.Width = M.NearestFactor(result.Width, Current.Get<Options>().TileResizeSnap);
                });
            }
        },
        Buttons.SaveCancel);
    });

    ICommand rotateCommand;
    [MenuItem(Category = Category.Rotate, Header = "Rotate...", Icon = SmallImages.Rotate, Index = -2, Parent = Menu.Transform)]
    public ICommand RotateCommand => rotateCommand ??= new RelayCommand<object>(i =>
    {
        if (i == null)
        {
            var rotation = new RotateForm(SelectedTiles);
            Dialog.ShowObject("Rotate", rotation, Resource.GetImageUri(SmallImages.Reset), i =>
            {
                if (i == 0)
                    SelectedTiles.ForEach(j => j.Rotation = rotation.Angle);
            },
            Buttons.SaveCancel);
        }
        else
        {
            var increment = int.Parse(i.ToString());
            if (increment == 0)
            {
                SelectedTiles.ForEach(j => j.Rotation = 0);
                return;
            }
            SelectedTiles.ForEach(j => j.Rotation += increment);
        }
    },
    i => SelectedTiles?.Any() == true);

    ICommand rotate0Command;
    [MenuItem(Category = Category.Rotate, Header = "Rotate 0°", Icon = SmallImages.Rotate, Index = -1, Parent = Menu.Transform)]
    public ICommand Rotate0Command => rotate0Command ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.Rotation = 0), () => SelectedTiles?.Any() == true);

    ICommand rotate45LeftCommand;
    [MenuItem(Category = Category.Rotate, Header = "Rotate -45°", Icon = SmallImages.Rotate45Left, Parent = Menu.Transform)]
    public ICommand Rotate45LeftCommand => rotate45LeftCommand ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.Rotation -= 45), () => SelectedTiles?.Any() == true);

    ICommand rotate45RightCommand;
    [MenuItem(Category = Category.Rotate, Header = "Rotate +45°", Icon = SmallImages.Rotate45Right, Parent = Menu.Transform)]
    public ICommand Rotate45RightCommand => rotate45RightCommand ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.Rotation += 45), () => SelectedTiles?.Any() == true);

    ICommand rotate90LeftCommand;
    [MenuItem(Category = Category.Rotate, Header = "Rotate -90°", Icon = SmallImages.Rotate90Left, Parent = Menu.Transform)]
    public ICommand Rotate90LeftCommand => rotate90LeftCommand ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.Rotation -= 90), () => SelectedTiles?.Any() == true);

    ICommand rotate90RightCommand;
    [MenuItem(Category = Category.Rotate, Header = "Rotate +90°", Icon = SmallImages.Rotate90Right, Parent = Menu.Transform)]
    public ICommand Rotate90RightCommand => rotate90RightCommand ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.Rotation += 90), () => SelectedTiles?.Any() == true);

    ICommand rotate180Command;
    [MenuItem(Category = Category.Rotate, Header = "Rotate 180°", Icon = SmallImages.Rotate180, Index = 1, Parent = Menu.Transform)]
    public ICommand Rotate180Command => rotate180Command ??= new RelayCommand(() => SelectedTiles.ForEach(i => i.Rotation += 180), () => SelectedTiles?.Any() == true);

    #endregion

    #endregion

    #region Commands

    ICommand backgroundColorCommand;
    public ICommand BackgroundColorCommand => backgroundColorCommand ??= new RelayCommand<ByteVector4>(i => SelectedTiles.ForEach(j => { j.Background = i; }), i => AnySelectedTiles);

    ICommand blendCommand;
    public ICommand BlendCommand => blendCommand ??= new RelayCommand<BlendModes>(i => SelectedTiles.ForEach(j => j.BlendMode = i), i => SelectedTiles?.Any() == true);

    ICommand cancelCommand;
    public ICommand CancelCommand => cancelCommand ??= new RelayCommand(() => Drawing = false, () => Drawing);
    
    ICommand effectCommand;
    public ICommand EffectCommand => effectCommand ??= new RelayCommand<ImageEffect>(i =>
    {
        var effect = i.Clone();
        Dialog.ShowObject($"Add '{i.Name}' effect", effect, Resource.GetImageUri(SmallImages.Fx), j =>
        {
            if (j == 0)
                SelectedTiles.ForEach(j => j.Effects.Add(effect.Clone()));
        }, 
        Buttons.SaveCancel);
    }, 
    i => AnySelectedTiles && i != null);

    ICommand openCommand;
    public ICommand OpenCommand => openCommand ??= new RelayCommand<string>(i =>
    {
        if (File.Long.Exists(i))
            OpenFileCommand.Execute(i);

        if (Folder.Long.Exists(i))
            OpenFolderCommand.Execute(i);
    }, 
    i => File.Long.Exists(i) || Folder.Long.Exists(i));

    ICommand openFileCommand;
    public ICommand OpenFileCommand => openFileCommand ??= new RelayCommand<string>(i => File.Long.Open(i), File.Long.Exists);

    ICommand openFolderCommand;
    public ICommand OpenFolderCommand => openFolderCommand ??= new RelayCommand<string>(i => Computer.OpenInWindowsExplorer(i), Folder.Long.Exists);

    ICommand foregroundColorCommand;
    public ICommand ForegroundColorCommand => foregroundColorCommand ??= new RelayCommand<ByteVector4>(i => SelectedTiles.ForEach(j => j.Foreground = i), i => AnySelectedTiles);

    ICommand minimizeTileCommand;
    public ICommand MinimizeTileCommand => minimizeTileCommand ??= new RelayCommand<Tile>(i =>
    {
        Screen.Remove(i);
        Screen.Minimized.Add(i);
    },
    i => i != null && Screen?.Minimized.Contains(i) == false);

    ICommand restoreCommand;
    public ICommand RestoreCommand => restoreCommand ??= new RelayCommand<Tile>(i =>
    {
        Screen.Minimized.Remove(i);
        Screen.Add(i);
    },
    i => i != null && Screen?.Minimized.Contains(i) == true);

    ICommand selectCommand;
    public ICommand SelectCommand => selectCommand ??= new RelayCommand<DoubleRegion>(i =>
    {
        var selection = i;

        var position
            = new Point2(M.NearestFactor(selection.X, Current.Get<Options>().TileResizeSnap), M.NearestFactor(selection.Y, Current.Get<Options>().TileResizeSnap));
        var size
            = new DoubleSize(M.NearestFactor(selection.Height, Current.Get<Options>().TileResizeSnap), M.NearestFactor(selection.Width, Current.Get<Options>().TileResizeSnap));

        var types
            = XAssembly.GetAssembly(AssemblyType.Current).GetDerivedTypes<Tile>($"{nameof(Imagin)}.{nameof(Apps)}.{nameof(Desktop)}", true, true);

        var form = new AddTileForm();
        Dialog.ShowObject("Add tile".Translate(), form, Resource.GetImageUri(SmallImages.Plus), i =>
        {
            if (i == 0)
            {
                Type type = types.FirstOrDefault(i => i.GetAttribute<TileTypeAttribute>().Type == form.Tile);

                var offset = new Point2(0, 0);
                for (var j = 0; j < form.Count; j++)
                {
                    var tile = Activator.CreateInstance(type) as Tile;
                    tile.Position.X
                        = position.X + offset.X;
                    tile.Position.Y 
                        = position.Y + offset.Y;
                    tile.Size
                        = size;

                    Screen.Add(tile);

                    if (tile is ImageTile imageTile)
                    {
                        StorageDialog.Show(out string path, "Select a file...", StorageDialogMode.OpenFile, ImageFormat.GetReadable().Select(j => j.Extension));
                        imageTile.Path = path;
                    }

                    offset = new(offset.X + 16, offset.Y + 16);
                }
            }

            selection.X = selection.Y = selection.Height = selection.Width = 0;
            Drawing = false;
        },
        Buttons.ContinueCancel);
    });

    ICommand selectScreenCommand;
    public ICommand SelectScreenCommand 
        => selectScreenCommand ??= new RelayCommand<Screen>(i => Screen = i, i => i is Screen);

    ICommand shapeCommand;
    public ICommand ShapeCommand => shapeCommand ??= new RelayCommand<NamableCategory<Core.Media.Shape>>(i => SelectedTiles.ForEach(j => j.Shape = Current.Get<Options>().Shapes.IndexOf(i)), i => i != null && SelectedTiles?.Any() == true);

    ICommand showTileOptionsCommand;
    public ICommand ShowTileOptionsCommand
        => showTileOptionsCommand ??= new RelayCommand<Tile>(i => Dialog.ShowObject("Tile options", i, Resource.GetImageUri(SmallImages.Options), Buttons.Done), i => i != null);

    #endregion
}