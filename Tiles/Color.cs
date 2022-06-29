using Imagin.Core;
using Imagin.Core.Controls;
using System;

namespace Imagin.Apps.Desktop
{
    [DisplayName("Color"), Serializable]
    public class ColorTile : Tile
    {
        ColorDocument document = new ColorDocument(new());
        [Hidden]
        public ColorDocument Document
        {
            get => document;
            set => this.Change(ref document, value);
        }

        public ColorTile() : base() { }
    }
}