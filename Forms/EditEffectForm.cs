using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Controls;
using Imagin.Core.Effects;

namespace Imagin.Apps.Desktop;

[Categorize(false), ViewSource(ShowHeader = false)]
public class EditEffectForm : SingleForm
{
    [Editable, HideName]
    [CollectionStyle(AddItems = nameof(EffectSource), AddType = typeof(ImageEffect))]
    public EffectCollection Effects => Tile.Effects;

    [Hide]
    public object EffectSource => Current.Get<MainViewModel>().EffectView;

    public EditEffectForm(Tile tile) : base(tile) { }
}

[Categorize(false), ViewSource(ShowHeader = false)]
public class AddEffectForm : Form
{
    [Editable, HideName, Name("Effect")]
    [Int32Style(Int32Style.Index, nameof(EffectSource), "Name")]
    public int SelectedIndex { get => Get(0); set => Set(value); }

    [Hide]
    public ObservableCollection<ImageEffect> EffectSource => Current.Get<MainViewModel>().EffectSource;

    [Editable]
    public ImageEffect SelectedEffect { get => Get<ImageEffect>(); set => Set(value); }

    public AddEffectForm() : base() { }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(SelectedIndex))
        {
            if (SelectedIndex >= 0 && SelectedIndex < EffectSource.Count)
            {
                var oldEffect = EffectSource[SelectedIndex];
                var newEffect = oldEffect.Clone(true);

                SelectedEffect = newEffect;
            }
        }
    }
}