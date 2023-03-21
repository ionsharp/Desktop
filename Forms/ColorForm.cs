using Imagin.Core;
using Imagin.Core.Numerics;

namespace Imagin.Apps.Desktop;

[Categorize(false), Name("Add color"), ViewSource(ShowHeader = false)]
public class ColorForm : Form
{
    public ByteVector4 Color { get => Get(ByteVector4.Black); set => Set(value); }

    public ColorForm() : base() { }
}