using Imagin.Core;
using Imagin.Core.Media;
using System;
using System.Windows.Media;

namespace Imagin.Apps.Desktop
{
    [DisplayName("Color")]
    [Serializable]
    public class ColorTile : Tile
    {
        ColorViewModel color = new(Colors.White);
        [Hidden]
        public ColorViewModel Color
        {
            get => color;
            set => this.Change(ref color, value);
        }

        public ColorTile() : base() { }
    }
}