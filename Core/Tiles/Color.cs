using Imagin.Core;
using Imagin.Core.Paint;
using System;
using System.Windows.Media;

namespace Imagin.Apps.Desktop
{
    [DisplayName("Color")]
    [Serializable]
    public class ColorTile : Tile
    {
        ColorViewModel color = new(Colors.White, new());
        [Hidden]
        public ColorViewModel Color
        {
            get => color;
            set => this.Change(ref color, value);
        }

        public ColorTile() : base() { }
    }
}