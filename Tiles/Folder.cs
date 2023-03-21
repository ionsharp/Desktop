using Imagin.Core;
using Imagin.Core.Controls;
using Imagin.Core.Storage;
using System;
using System.Windows;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Folder), Name("Folder"), Serializable, TileType(TileTypes.Folder)]
public class FolderTile : Tile, IElementReference
{
    [field: NonSerialized]
    public static readonly ReferenceKey<Browser> BrowserReferenceKey = new();

    [Hide, XmlIgnore]
    public Browser Browser { get; private set; }

    [Name("Folder options"), XmlIgnore]
    public virtual FolderOptions FolderOptions { get => Get(new FolderOptions()); set => Set(value); }

    [Name("Read only")]
    public virtual bool IsReadOnly { get => Get(true); set => Set(value); }

    [Hide]
    public string Path { get => Get(StoragePath.Root); set => Set(value); }

    [Hide, XmlIgnore]
    public override string Title { get => base.Title; set => base.Title = value; }

    public FolderTile() : base() { }

    public FolderTile(string path) : base()
    {
        Path = path;
    }

    void IElementReference.SetReference(IElementKey key, FrameworkElement element)
    {
        if (key == BrowserReferenceKey)
            Browser = (Browser)element;
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