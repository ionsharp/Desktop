using Imagin.Core;

namespace Imagin.Apps.Desktop;

[Categorize(false), ViewSource(ShowHeader = false)]
public class AddScreenForm : Form
{
    [Range(0, int.MaxValue, 1, Style = RangeStyle.UpDown)]
    public int InsertAt { get => Get(0); set => Set(value); }

    public string Name { get => Get("Untitled screen"); set => Set(value); }

    public AddScreenForm() : base() { }
}