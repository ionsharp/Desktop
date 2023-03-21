using Imagin.Core;
using Imagin.Core.Controls;
using Imagin.Core.Storage;
using System;
using System.Windows;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Console), Name("Console"), Serializable, TileType(TileTypes.Console)]
public class ConsoleTile : Tile, IElementReference
{
    [field: NonSerialized]
    public static readonly ReferenceKey<ConsoleBox> ConsoleReferenceKey = new();

    [Hide, XmlIgnore]
    public ConsoleBox Console { get; private set; }

    [Name("Console"), XmlIgnore]
    public ConsoleOptions ConsoleOptions { get => Get(new ConsoleOptions()); set => Set(value); }

    [Hide]
    public string Path { get => Get(StoragePath.Root); set => Set(value); }

    [Hide, XmlIgnore]
    public override string Title { get => base.Title; set => base.Title = value; }

    public ConsoleTile() : base() { }

    public ConsoleTile(string path) : base()
    {
        Path = path;
    }

    void IElementReference.SetReference(IElementKey key, FrameworkElement element)
    {
        if (key == ConsoleReferenceKey)
            Console = (ConsoleBox)element;
    }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(Path):
                OnChanged();
                break;
        }
    }
}