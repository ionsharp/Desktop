using Imagin.Core;
using Imagin.Core.Controls;
using System;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Color), Name("Color"), Serializable, TileType(TileTypes.Color)]
public class ColorTile : Tile
{
    [Hide, XmlIgnore]
    public ColorDocument Document { get => Get(new ColorDocument()); set => Set(value); }

    public ColorTile() : base() { }
}