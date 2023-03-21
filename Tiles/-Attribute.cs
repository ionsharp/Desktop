using System;

namespace Imagin.Apps.Desktop;

[AttributeUsage(AttributeTargets.Class)]
public class TileTypeAttribute : Attribute
{
    public TileTypes Type { get; set; }

    public TileTypeAttribute(TileTypes type) : base()
    {
        Type = type;
    }
}