using Imagin.Core;
using System;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Calendar), Name("Calendar"), Serializable, TileType(TileTypes.Calendar)]
public class CalendarTile : Tile
{
    [Hide]
    public DateTime Date { get => Get(DateTime.Now); set => Set(value); }

    public CalendarTile() : base() { }
}