using Imagin.Core;
using Imagin.Core.Numerics;

namespace Imagin.Apps.Desktop;

[Categorize(false), ViewSource(ShowHeader = false)]
public class ArrangeForm : Form
{
    public DoubleRegion Region { get => Get(new DoubleRegion()); set => Set(value); }

    public double Spacing { get => Get(.0); set => Set(value); }

    public ArrangeForm() : base() { }
}