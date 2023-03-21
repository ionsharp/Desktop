using Imagin.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imagin.Apps.Desktop;

public class SingleForm : Form
{
    [Hide]
    public readonly Tile Tile;

    public SingleForm(Tile tile) : base()
    {
        Tile = tile;
        if (tile == null)
            throw new NotSupportedException();
    }
}

public class MultipleForm : Form
{
    [Hide]
    public readonly IEnumerable<Tile> Tiles;

    public MultipleForm(IEnumerable<Tile> tiles) : base()
    {
        Tiles = tiles;
        if (Tiles.Count() == 0)
            throw new NotSupportedException();
    }
}