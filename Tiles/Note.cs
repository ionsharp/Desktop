using Imagin.Core;
using Imagin.Core.Media;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Note), Name("Note"), Serializable, TileType(TileTypes.Note)]
public class NoteTile : Tile
{
    enum Category { Font }

    [Category(Category.Font), Name("Font alignment"), XmlIgnore]
    public TextAlignment FontAlignment { get => GetFromString(TextAlignment.Left); set => SetFromString(value); }

    [Category(Category.Font), Name("Font family"), XmlIgnore]
    public FontFamily FontFamily { get => GetFrom(new FontFamily("Calibri"), Core.Conversion.Converter.Get<FontFamilyToStringConverter>()); set => SetFrom(value, Core.Conversion.Converter.Get<FontFamilyToStringConverter>()); }

    [Category(Category.Font), Name("Font size")]
    public double FontSize { get => Get(16.0); set => Set(value); }

    [Hide]
    public string Text { get => Get(""); set => Set(value); }

    public NoteTile() : base() { }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(Text):
                OnChanged();
                break;
        }
    }
}