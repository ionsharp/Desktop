using Imagin.Core;

namespace Imagin.Apps.Desktop;

[Categorize(false), ViewSource(ShowHeader = false)]
public class AddTileForm : Form
{
    [Range(1, int.MaxValue, 1, Style = RangeStyle.UpDown)]
    public int Count { get => Get(1); set => Set(value); }

    [Vertical]
    public TileTypes Tile { get => Get(TileTypes.Calendar); set => Set(value); }

    public AddTileForm() : base() { }
}