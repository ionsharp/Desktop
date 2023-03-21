using Imagin.Core;
using System;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Video), Name("Video"), Serializable, TileType(TileTypes.Video)]
public class VideoTile : Tile
{
    public VideoTile() : base() { }
}