using Imagin.Core;
using Imagin.Core.Collections;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Collections.Serialization;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using Imagin.Core.Media;
using Imagin.Core.Models;
using Imagin.Core.Numerics;
using Imagin.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Imagin.Apps.Desktop;

[Serializable, View(MemberView.Tab, typeof(Tab))]
public class Options : MainViewOptions
{
    enum Category { Background, Color, Drag, General, Header, Minimized, Navigation, Screen, Snap, Search, Shadow, Tile, ToolTip, Visibility, Window }

    enum Tab { Color, Menu, Screen, Tile, Warning }

    #region Color

    [Category(Category.General), Tab(Tab.Color)]
    [CollectionStyle(AddType = typeof(ByteVector4)), HideName]
    public ColorCollection Colors { get => Get<ColorCollection>(); set => Set(value); }

    [Category(Category.ToolTip), Name("Models"), Tab(Tab.Color), StringStyle(StringStyle.Tokens)]
    public string ColorToolTip { get => Get("RGB;CMYK"); set => Set(value); }

    [Category(Category.ToolTip), Name("Models (Normalize)"), Tab(Tab.Color)]
    public bool ColorToolTipNormalize { get => Get(false); set => Set(value); }

    [Category(Category.ToolTip), Name("Models (Precision)"), Range(0, 6, 1, Style = RangeStyle.Both), Tab(Tab.Color)]
    public int ColorToolTipPrecision { get => Get(2); set => Set(value); }

    #endregion

    #region Menu

    [Category(Category.Background), Tab(Tab.Menu)]
    [Name("Opacity"), Range(0.0, 1.0, 0.01, Style = RangeStyle.Both)]
    public double MenuBackgroundOpacity { get => Get(1.0); set => Set(value); }

    #endregion

    #region Screen

    [Category(Category.Navigation), Tab(Tab.Screen)]
    [Name("Alignment")]
    public RelativeDirection ScreenNavigationAlignment { get => Get(RelativeDirection.Down); set => Set(value); }

    [Category(Category.Navigation), Tab(Tab.Screen)]
    [Name("Visible")]
    public bool IsScreenNavigationVisible { get => Get(true); set => Set(value); }

    [Category(Category.Navigation), Tab(Tab.Screen)]
    [Name("Size")]
    public double ScreenNavigationSize { get => Get(12.0); set => Set(value); }

    [Category(Category.Navigation), Tab(Tab.Screen)]
    [Name("Margin")]
    public double ScreenNavigationMargin { get => Get(10.0); set => Set(value); }

    [Category(Category.Navigation), Tab(Tab.Screen)]
    [Name("Shape")]
    [Int32Style(Int32Style.Index, nameof(Shapes), nameof(IName.Name))]
    public int ScreenNavigationShape { get => Get(0); set => Set(value); }

    [Int32Style(Int32Style.Index, nameof(Screens))]
    public int Screen { get => Get(0); set => Set(value); }

    [CollectionStyle(AddType = typeof(Screen))]
    public XmlWriter<Screen> Screens { get => Get<XmlWriter<Screen>>(); set => Set(value); }

    [Name("Transition"), Tab(Tab.Screen)]
    public Transitions ScreenTransition { get => Get(Transitions.LeftReplace); set => Set(value); }

    #endregion

    #region Search 

    [Category(Category.Search), Name("Search engines")]
    [CollectionStyle(AddType = typeof(SearchEngine))]
    public ObservableCollection<SearchEngine> SearchEngines { get => Get(new ObservableCollection<SearchEngine>()
    {
        new("DuckDuckGo",
            @"https://duckduckgo.com/?q="),
        new("Google",
            @"https://www.google.com/search?q="),
        new("Bing",
            @"https://www.bing.com/search?q=")
    }); set => Set(value); }

    #endregion

    #region Shape

    [Hide]
    public ShapeCollection Shapes { get => Get<ShapeCollection>(); set => Set(value); }
    
    #endregion

    #region Tile

    [Category(Category.Drag), Tab(Tab.Tile)]
    [Name("Can drag outside")]
    public bool CanDragTilesOutside { get => Get(false); set => Set(value); }

    [Category(Category.Color), Tab(Tab.Tile), Name("Background")]
    public ByteVector4 TileBackground { get => Get(new ByteVector4(255)); set => Set(value); }

    [Category(Category.Color), Tab(Tab.Tile), Name("Blend mode")]
    public BlendModes TileBlendMode { get => Get(BlendModes.Normal); set => Set(value); }

    [Category(Category.Color), Tab(Tab.Tile), Name("Foreground")]
    public ByteVector4 TileForeground { get => Get(new ByteVector4(255)); set => Set(value); }

    [Tab(Tab.Tile), Category(Category.Color)]
    [Name("Opacity")]
    [Range(0.0, 1.0, 0.01, Style = RangeStyle.Both)]
    public double TileOpacity { get => Get(0.8); set => Set(value); }

    [Category(Category.Color)]
    [Name("Override theme")]
    public bool TileOverrideTheme { get => Get(false); set => Set(value); }

    [Category(Category.Minimized), Tab(Tab.Tile)]
    [Name("Placement")]
    public TopBottom TileMinimizedPlacement { get => Get(TopBottom.Bottom); set => Set(value); }

    [Category(Category.Minimized), Tab(Tab.Tile)]
    [Name("Show")]
    public bool TileMinimizedShow { get => Get(true); set => Set(value); }

    [Category(Category.Snap), Tab(Tab.Tile)]
    [Name("MoveSnap")]
    [Range(1.0, 64.0, 1.0, Style = RangeStyle.Both)]
    public double TileMoveSnap { get => Get(16.0); set => Set(value); }

    [Category(Category.Snap), Tab(Tab.Tile)]
    [Name("ResizeSnap")]
    [Range(1.0, 64.0, 1.0, Style = RangeStyle.Both)]
    public double TileResizeSnap { get => Get(16.0); set => Set(value); }

    [Category(Category.Snap), Tab(Tab.Tile)]
    [Name("RotateSnap")]
    [Range(1.0, 180.0, 1.0, Style = RangeStyle.Both)]
    public double TileRotateSnap { get => Get(11.25); set => Set(value); }

    [Category(Category.Shadow), Tab(Tab.Tile)]
    [Name("Shadow color")]
    public ByteVector4 TileShadowColor { get => Get(ByteVector4.Black); set => Set(value); }

    [Category(Category.Shadow), Tab(Tab.Tile)]
    [Name("Shadow depth")]
    [Range(.0, double.MaxValue, 1.0, Style = RangeStyle.UpDown)]
    public double TileShadowDepth { get => Get(5.0); set => Set(value); }

    [Category(Category.Shadow), Tab(Tab.Tile)]
    [Name("Shadow direction")]
    [Range(.0, 360.0, 1.0, Style = RangeStyle.Both)]
    public double TileShadowDirection { get => Get(315.0); set => Set(value); }

    [Category(Category.Shadow), Tab(Tab.Tile)]
    [Name("Shadow opacity")]
    [Range(.0, 1.0, 0.01, Style = RangeStyle.Both)]
    public double TileShadowOpacity { get => Get(1.0); set => Set(value); }

    [Category(Category.Shadow), Tab(Tab.Tile)]
    [Name("Shadow radius")]
    [Range(.0, double.MaxValue, 1.0, Style = RangeStyle.UpDown)]
    public double TileShadowRadius { get => Get(25.0); set => Set(value); }

    #endregion

    #region Warning

    [Category(Category.Screen), Name("Before removing screen"), Tab(Tab.Warning)]
    public bool WarnBeforeRemovingScreen { get => Get(true); set => Set(value); }

    [Category(Category.Screen), Name("Before removing all screens"), Tab(Tab.Warning)]
    public bool WarnBeforeRemovingAllScreens { get => Get(true); set => Set(value); }

    [Category(Category.Tile), Name("Before removing tile"), Tab(Tab.Warning)]
    public bool WarnBeforeRemovingTile { get => Get(true); set => Set(value); }

    [Category(Category.Tile), Name("Before removing a selection of tiles"), Tab(Tab.Warning)]
    public bool WarnBeforeRemovingASelectionOfTiles { get => Get(true); set => Set(value); }

    [Category(Category.Screen), Name("Before removing all tiles"), Tab(Tab.Warning)]
    public bool WarnBeforeRemovingAllTiles { get => Get(true); set => Set(value); }
    
    #endregion

    #region Window

    [Name("Window placement")]
    public WindowPlacements WindowPlacement { get => Get(WindowPlacements.None); set => Set(value); }

    [Hide]
    public double OptionsWindowHeight { get => Get(720); set => Set(value); }

    [Hide]
    public double OptionsWindowWidth { get => Get(420); set => Set(value); }

    [Hide]
    public override bool TaskbarIconVisibility { get => base.TaskbarIconVisibility; set => base.TaskbarIconVisibility = value; }

    #endregion

    ///

    #region Methods

    protected override IEnumerable<IWriter> GetData()
    {
        yield return Screens;
    }

    protected override void OnLoaded()
    {
        _ = Dispatch.BeginInvoke(() =>
        {
            ///Colors
            if (Colors == null)
            {

                Colors = new();
                Colors.Reset();
            }

            ///Shapes
            if (Shapes == null)
            {
                Shapes = new();
                Shapes.Reset();
            }
        });

        ///Screens
        Screens = new XmlWriter<Screen>(nameof(Screen), Current.Get<App>().DataFolderPath, "Screens", "xml", "xml", new Limit(10, Limit.Actions.ClearAndArchive),
            new Type[] { typeof(Tile) }.Concat(XAssembly.GetDerivedTypes<Tile>(AssemblyType.Current)).ToArray());

        base.OnLoaded();
    }

    [NonSerialized]
    ICommand newShapeCommand;
    public ICommand NewShapeCommand => newShapeCommand ??= new RelayCommand(() =>
    {
        var result = new ShapeForm();
        Dialog.ShowObject("New shape", result, Resource.GetImageUri(SmallImages.Plus), i =>
        {
            if (i == 0)
            {
                if (result.Preview != null)
                    Shapes.Add(new NamableCategory<Core.Media.Shape>("Untitled", "Custom", result.Preview));
            }
        },
        Buttons.SaveCancel);
    });

    
    #endregion
}