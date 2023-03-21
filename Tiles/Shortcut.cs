using Imagin.Core;
using System;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Shortcut), Name("Shortcut"), Serializable, TileType(TileTypes.Shortcut)]
public class ShortcutTile : Tile
{
    [StringStyle(StringStyle.Path)]
    public string Path { get => Get($@"C:\"); set => Set(value); }

    public string OverrideName { get => Get<string>(null); set => Set(value); }

    public bool ShowName { get => Get(true); set => Set(value); }

    public ShortcutTile() : base() { }
}