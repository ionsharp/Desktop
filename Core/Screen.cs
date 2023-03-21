using Imagin.Core;
using Imagin.Core.Collections;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Controls;
using Imagin.Core.Linq;
using System;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop;

[Categorize(false), Serializable]
public class Screen : ObservableCollection<Tile>, ILimit
{
    public static ReferenceKey<SlideshowControl> SlideshowKey = new();

    public static Limit DefaultLimit = new(25, Limit.Actions.RemoveFirst);

    public int Index => Current.Get<MainViewModel>().Screens.IndexOf(this) + 1;

    Limit limit = DefaultLimit;
    [XmlIgnore]
    public Limit Limit
    {
        get => limit;
        set
        {
            limit = value;
            limit.Coerce(this);
        }
    }
    
    public bool IsLocked { get => this.Get(false); set => this.Set(value); }

    public string Name { get => this.Get(""); set => this.Set(value); }

    public ObservableCollection<Tile> Minimized { get => this.Get<ObservableCollection<Tile>>(new()); set => this.Set(value); }

    public Screen() : base() { }

    public Screen(params Tile[] tiles) : base() => tiles?.ForEach(i => Add(i));

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                limit.Coerce(this);
                break;

            case NotifyCollectionChangedAction.Remove:
                (e.OldItems[0] as Tile).Dispose();
                break;
        }
    }
}