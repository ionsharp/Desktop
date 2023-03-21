using Imagin.Core;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Search), Name("Search"), Serializable, TileType(TileTypes.Search)]
public class SearchTile : Tile
{
    [XmlIgnore]
    IList<SearchEngine> SearchEngines => Current.Get<Options>().SearchEngines;

    [XmlIgnore]
    string Url => $"{SearchEngines[SearchEngine].Value}{Text}";

    ///

    [Hide]
    public int SearchEngine { get => Get(0); set => Set(value); }

    [Hide]
    public string Text { get => Get(""); set => Set(value); }

    ///

    public SearchTile() : base() { }

    ///

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(SearchEngine):
            case nameof(Text):
                OnChanged();
                break;
        }
    }

    [field: NonSerialized]
    ICommand searchCommand;
    [Hide, XmlIgnore]
    public ICommand SearchCommand => searchCommand ??= new RelayCommand(() => Process.Start(new ProcessStartInfo(Url)), () => SearchEngine >= 0 && SearchEngine < SearchEngines.Count && !Text.NullOrEmpty());
}