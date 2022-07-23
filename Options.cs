using Imagin.Core;
using Imagin.Core.Collections;
using Imagin.Core.Collections.Serialization;
using Imagin.Core.Controls;
using Imagin.Core.Models;
using Imagin.Core.Numerics;
using Imagin.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Imagin.Apps.Desktop
{
    [Serializable]
    public class Options : MainViewOptions
    {
        enum Category { Screens, Search, Tiles, Window }

        #region Properties

        #region Screens 

        int screen = 0;
        [Hidden]
        public int Screen
        {
            get => screen;
            set => this.Change(ref screen, value);
        }

        [field: NonSerialized]
        XmlWriter<Screen> screens;
        [Hidden]
        public XmlWriter<Screen> Screens
        {
            get => screens;
            set => this.Change(ref screens, value);
        }

        Transitions screenTransition = Transitions.LeftReplace;
        [Category(Category.Screens)]
        [DisplayName("Transition")]
        public Transitions ScreenTransition
        {
            get => screenTransition;
            set => this.Change(ref screenTransition, value);
        }

        #endregion

        #region Search 

        ObservableCollection<SearchEngine> searchEngines = new() 
        { 
            new("DuckDuckGo",
                @"https://duckduckgo.com/?q="),
            new("Google", 
                @"https://www.google.com/search?q="), 
            new("Bing", 
                @"https://www.bing.com/search?q=") 
        };
        [Category(Category.Search)]
        [DisplayName("Search engines")]
        [Setter(nameof(MemberModel.ItemType), typeof(SearchEngine))]
        public ObservableCollection<SearchEngine> SearchEngines
        {
            get => searchEngines;
            set => this.Change(ref searchEngines, value);
        }

        #endregion

        #region Tiles 

        ByteVector4 tileBackground = new ByteVector4(255);
        [Category(Category.Tiles), DisplayName("Background")]
        public ByteVector4 TileBackground
        {
            get => tileBackground;
            set => this.Change(ref tileBackground, value);
        }

        ByteVector4 tileForeground = new ByteVector4(255);
        [Category(Category.Tiles), DisplayName("Foreground")]
        public ByteVector4 TileForeground
        {
            get => tileForeground;
            set => this.Change(ref tileForeground, value);
        }

        HorizontalAlignment tileHeaderAlignment = HorizontalAlignment.Left;
        [Category(Category.Tiles)]
        [DisplayName("Header alignment")]
        public HorizontalAlignment TileHeaderAlignment
        {
            get => tileHeaderAlignment;
            set => this.Change(ref tileHeaderAlignment, value);
        }

        TopBottom tileHeaderPlacement = TopBottom.Top;
        [Category(Category.Tiles)]
        [DisplayName("Header placement")]
        public TopBottom TileHeaderPlacement
        {
            get => tileHeaderPlacement;
            set => this.Change(ref tileHeaderPlacement, value);
        }

        double tileOpacity = 0.8;
        [Category(Category.Tiles)]
        [DisplayName("Opacity")]
        [SliderUpDown]
        [Range(0.0, 1.0, 0.01)]
        public double TileOpacity
        {
            get => tileOpacity;
            set => this.Change(ref tileOpacity, value);
        }

        bool tileOverrideTheme = false;
        [Category(Category.Tiles)]
        [DisplayName("Override theme")]
        public bool TileOverrideTheme
        {
            get => tileOverrideTheme;
            set => this.Change(ref tileOverrideTheme, value);
        }

        double tileSnap = 16.0;
        [Category(Category.Tiles)]
        [DisplayName("Snap")]
        [SliderUpDown]
        [Range(1.0, 32.0, 1.0)]
        public double TileSnap
        {
            get => tileSnap;
            set => this.Change(ref tileSnap, value);
        }

        #endregion

        #region Window

        WindowPlacements windowPlacement = WindowPlacements.None;
        [DisplayName("Window placement")]
        public WindowPlacements WindowPlacement
        {
            get => windowPlacement;
            set => this.Change(ref windowPlacement, value);
        }

        double optionsWindowHeight = 720;
        [Hidden]
        public double OptionsWindowHeight
        {
            get => optionsWindowHeight;
            set => this.Change(ref optionsWindowHeight, value);
        }

        double optionsWindowWidth = 420;
        [Hidden]
        public double OptionsWindowWidth
        {
            get => optionsWindowWidth;
            set => this.Change(ref optionsWindowWidth, value);
        }

        [Hidden]
        public override bool WindowShowInTaskBar
        {
            get => base.WindowShowInTaskBar;
            set => base.WindowShowInTaskBar = value;
        }

        #endregion

        #endregion

        #region Methods

        protected override IEnumerable<IWriter> GetData()
        {
            yield return Screens;
        }

        protected override void OnLoaded()
        {
            Screens = new XmlWriter<Screen>(nameof(Screen), Get.Current<App>().Properties.FolderPath, "Screens", "xml", "xml", new Limit(10, Limit.Actions.ClearAndArchive),
                typeof(CalendarTile), typeof(ClockTile), typeof(CountDownTile), typeof(FolderTile), typeof(ImageTile), typeof(NoteTile), typeof(SearchTile), typeof(Tile));
            base.OnLoaded();
        }

        #endregion
    }
}