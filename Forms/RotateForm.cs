using Imagin.Core;
using Imagin.Core.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Imagin.Apps.Desktop;

[Categorize(false), ViewSource(ShowHeader = false)]
public class RotateForm : MultipleForm
{
    [Index(1), Name("Formula")]
    public Formulas AngleFormula { get => Get(Formulas.None); set => Set(value); }

    [EnableTrigger(nameof(AngleFormula), Operators.Equal, Formulas.None)]
    [Range(.0, 360.0, 1.0, Style = RangeStyle.Both)]
    public double Angle { get => Get(.0); set => Set(value); }

    public RotateForm(IEnumerable<Tile> tiles) : base(tiles)
    {
        Update(() => AngleFormula);
    }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(AngleFormula):
                Angle = Tiles.Select(i => i.Rotation).Calculate(AngleFormula);
                break;
        }
    }
}