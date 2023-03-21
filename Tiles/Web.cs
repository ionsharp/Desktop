using Imagin.Core;
using System;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Globe), Name("Web"), Serializable, TileType(TileTypes.Web)]
public class WebTile : Tile
{
    public const string Google = @"http://www.google.com";

    [Hide, Modify, XmlAttribute]
    public string Address 
    { 
        get => Get(Google); 
        set
        {
            if (!value.StartsWith("http://"))
                value = $"http://{value}";

            Set(value); 
        }
    }

    public WebTile() : base() { }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(Address):
                OnChanged();
                break;
        }
    }
}