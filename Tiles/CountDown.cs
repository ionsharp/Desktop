using Imagin.Core;
using Imagin.Core.Controls;
using Imagin.Core.Input;
using Imagin.Core.Models;
using Imagin.Core.Storage;
using System;
using System.Timers;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Image(SmallImages.Number), Name("Count down"), Serializable, TileType(TileTypes.CountDown)]
public class CountDownTile : Tile
{
    enum Category { General }

    [Category(Category.General)]
    public DateTime Date { get => Get(DateTime.Now); set => Set(value); }

    [Category(Category.General)]
    public string Image { get => Get(""); set => Set(value); }

    [Hide, XmlIgnore]
    public bool IsEditable { get => Get(false); set => Set(value); }

    public CountDownTile() : base()
    {
        timer.Enabled = true;
    }

    protected override void OnUpdate(ElapsedEventArgs e)
    {
        base.OnUpdate(e);
        if (!IsEditable)
            XPropertyChanged.Update(this, () => Date);
    }

    [field: NonSerialized]
    ICommand changeImageCommand;
    [Hide, XmlIgnore]
    public ICommand ChangeImageCommand => changeImageCommand ??= new RelayCommand(() =>
    {
        var image = new ViewModel<string>(Image);
        Dialog.ShowObject("Change image", image, Resource.GetImageUri(SmallImages.Image), i =>
        {
            if (i == 0)
                Image = image.View;
        },
        Buttons.SaveCancel);
    });
}