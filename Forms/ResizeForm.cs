using Imagin.Core;
using Imagin.Core.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Imagin.Apps.Desktop;

[ViewSource(ShowHeader = false)]
public class ResizeForm : MultipleForm
{
    enum Category { Height, Width }

    [Category(Category.Height), Index(1), Name("Formula")]
    public Formulas HeightFormula { get => Get(Formulas.None); set => Set(value); }

    [EnableTrigger(nameof(HeightFormula), Operators.Equal, Formulas.None)]
    [Category(Category.Height), Range(.0, double.MaxValue, 1.0, Style = RangeStyle.UpDown)]
    public double Height { get => Get(.0); set => Set(value); }

    [Category(Category.Width), Index(1), Name("Formula")]
    public Formulas WidthFormula { get => Get(Formulas.None); set => Set(value); }

    [EnableTrigger(nameof(WidthFormula), Operators.Equal, Formulas.None)]
    [Category(Category.Width), Range(.0, double.MaxValue, 1.0, Style = RangeStyle.UpDown)]
    public double Width { get => Get(.0); set => Set(value); }

    public ResizeForm(IEnumerable<Tile> tiles) : base(tiles)
    {
        Update(() => HeightFormula);
        Update(() => WidthFormula);
    }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(HeightFormula):
                Height = Tiles.Select(i => i.Size.Height).Calculate(HeightFormula);
                break;

            case nameof(WidthFormula):
                Width = Tiles.Select(i => i.Size.Width).Calculate(WidthFormula);
                break;
        }
    }
}