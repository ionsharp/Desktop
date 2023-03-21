using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Effects;
using Imagin.Core.Input;
using Imagin.Core.Linq;
using Imagin.Core.Media;
using Imagin.Core.Numerics;
using System;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Serializable]
public abstract class Tile : Updatable, IChange, ILock, IPoint2, IRotate, ISize, ISelect
{
    protected enum DefaultCategory { Color, General, Transform, View }

    #region Events

    [field: NonSerialized]
    public event ChangedEventHandler Changed;

    [field: NonSerialized]
    public event LockedEventHandler Locked;
    public event SelectedEventHandler Selected;

    #endregion

    #region Properties

    #region NonSerializable

    [Hide, XmlIgnore]
    public Core.Media.Shape ActualShape => Shapes != null && Shape >= 0 && Shape < Shapes.Count ? Shapes[Shape].Value : null;

    [Hide, XmlIgnore]
    public bool IsMouseOver { get => Get(false, false); set => Set(value, false); }

    [Hide, XmlIgnore]
    public bool IsSelected { get => Get(false, false); set => Set(value, false); }

    [Hide, XmlIgnore]
    public ObservableCollection<NamableCategory<Core.Media.Shape>> Shapes => Current.Get<Options>()?.Shapes;

    #endregion

    #region Serializable

    [Category(DefaultCategory.Color), XmlAttribute]
    public string Background { get => Get<string>(); set => Set(value); }

    [Category(DefaultCategory.Color), XmlAttribute]
    public BlendModes BlendMode { get => Get(BlendModes.Normal); set => Set(value); }

    [Hide, XmlIgnore]
    public EffectCollection Effects { get => Get(new EffectCollection()); set => Set(value); }

    [Category(DefaultCategory.Color), XmlAttribute]
    public string Foreground { get => Get<string>(); set => Set(value); }

    [Hide]
    public DateTime Created { get => Get(DateTime.Now); set => Set(value); }

    [Pin(Pin.AboveOrLeft), Name("Lock"), Style(BooleanStyle.ToggleButton)]
    public bool IsLocked { get => Get(false); set => Set(value); }

    [Category(DefaultCategory.Color), Range(0.0, 1.0, 0.01, Style = RangeStyle.Both), XmlAttribute]
    public double Opacity { get => Get(1.0); set => Set(value); }

    [Category(DefaultCategory.Transform), Editable]
    public Point2 Position { get => Get(new Point2(0, 0)); set => Set(value); }

    [Int32Style(Int32Style.Index, nameof(Shapes), nameof(IName.Name))]
    [XmlAttribute]
    public int Shape { get => Get(8); set => Set(value); }

    [Category(DefaultCategory.Transform), XmlAttribute]
    public virtual double Rotation { get => Get(.0); set => Set(value); }

    [Category(DefaultCategory.Transform), Editable, Vertical]
    public DoubleSize Size { get => Get(new DoubleSize(250d, 250d)); set => Set(value); }

    [Hide]
    public virtual string Title { get => Get(""); set => Set(value); }

    #endregion

    #endregion

    #region Tile

    public Tile() : base() { }

    #endregion

    #region Methods

    protected virtual void OnChanged() => Changed?.Invoke(this);

    protected virtual void OnSelected() => Selected?.Invoke(this, new SelectedEventArgs(this));

    [Hide]
    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(IsLocked):
                OnChanged();
                Locked?.Invoke(this, new(IsLocked));
                break;

            case nameof(IsSelected):
                if (IsSelected)
                    OnSelected();
                break;

            case nameof(Position):
            case nameof(Size):
            case nameof(Title):
                OnChanged();
                break;

            case nameof(Shape):
                Update(() => ActualShape);
                break;
        }
    }

    public override void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        base.OnPropertyChanging(e);
        //Current.Get<TileHistory>()?.Add(new(this, e.PropertyName, e.OldValue, e.NewValue));
    }

    #endregion

    #region Commands

    [field: NonSerialized]
    ICommand closeCommand;
    [Hide, XmlIgnore]
    public ICommand CloseCommand => closeCommand ??= new RelayCommand(() =>
    {
        var screen = Current.Get<MainViewModel>().Screen;
        if (screen.Minimized.Contains(this))
        { 
            screen.Minimized.Remove(this);
            return;
        }

        for (var i = screen.Count - 1; i >= 0; i--)
        {
            if (screen[i].IsSelected)
                screen.RemoveAt(i);
        }
    },
    () => Current.Get<MainViewModel>().Screen is Screen screen && screen.Any(i => i.IsSelected) && !screen.Any(i => i.IsSelected && i.IsLocked));

    [field: NonSerialized]
    ICommand editCommand;
    [Hide, XmlIgnore]
    public ICommand EditCommand => editCommand ??= new RelayCommand(() => Current.Get<MainViewModel>().EditTilesCommand.Execute(this));

    [field: NonSerialized]
    ICommand hideCommand;
    [Hide, XmlIgnore]
    public ICommand HideCommand => hideCommand ??= new RelayCommand(() =>
    {
        Current.Get<MainViewModel>().Screen.Remove(this);
        Current.Get<MainViewModel>().Screen.Minimized.Add(this);
    },
    () => Current.Get<MainViewModel>().Screen?.Contains(this) == true);

    [field: NonSerialized]
    ICommand showCommand;
    [Hide, XmlIgnore]
    public ICommand ShowCommand => showCommand ??= new RelayCommand(() =>
    {
        Current.Get<MainViewModel>().Screen.Minimized.Remove(this);
        Current.Get<MainViewModel>().Screen.Add(this);
    },
    () => Current.Get<MainViewModel>().Screen?.Minimized.Contains(this) == true);

    #endregion
}