using Imagin.Core;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Storage;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Image), Name("Image"), Serializable, TileType(TileTypes.Image)]
public class ImageTile : Tile
{
    [Category(DefaultCategory.General), RightText("seconds")]
    public double Interval { get => Get(5.0); set => Set(value); }

    public virtual string Path { get => Get(StoragePath.Root); set => Set(value); }

    [Hide, XmlIgnore]
    public override string Title { get => base.Title; set => base.Title = value; }

    [Category(DefaultCategory.General), XmlIgnore]
    public Stretch Stretch { get => Get(Stretch.UniformToFill); set => Set(value); }

    [Category(DefaultCategory.General)]
    public Transitions Transition { get => Get(Transitions.Random); set => Set(value); }

    public ImageTile() : base() { }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(Interval):
            case nameof(Path):
            case nameof(Transition):
                OnChanged();
                break;
        }
    }

    [field: NonSerialized]
    ICommand transitionCommand;
    [Hide, XmlIgnore]
    public ICommand TransitionCommand => transitionCommand ??= new RelayCommand<object>(i => Transition = (Transitions)i, i => i is Transitions);
}