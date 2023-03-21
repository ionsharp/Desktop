using Imagin.Core;
using System;
using System.Timers;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

public enum ClockTileType { Analog, Digital }

[Image(SmallImages.Clock), Name("Clock"), Serializable, TileType(TileTypes.Clock)]
public class ClockTile : Tile
{
    public string Format { get => Get("ddd, MMM d, yyyy • h:mm:ss tt"); set => Set(value); }

    [Hide]
    public string DateTime => System.DateTime.Now.ToString(Format);

    [XmlIgnore]
    public ClockTileType Type { get => Get(ClockTileType.Analog); set => Set(value); }

    public ClockTile() : base()
    {
        timer.Enabled = true;
    }

    protected override void OnUpdate(ElapsedEventArgs e)
    {
        base.OnUpdate(e);
        XPropertyChanged.Update(this, () => DateTime);
    }
}