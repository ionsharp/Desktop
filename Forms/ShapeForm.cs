using Imagin.Core;
using Imagin.Core.Linq;
using Imagin.Core.Media;
using Imagin.Core.Numerics;

namespace Imagin.Apps.Desktop;

public enum ShapeType { Custom, Polygon, Star }

[Name("Add shape"), ViewSource(ShowHeader = false)]
public class ShapeForm : Form
{
    [Pin(Pin.AboveOrLeft)]
    public ShapeType Type { get => Get(ShapeType.Polygon); set => Set(value); }

    [Range(.0, 360.0, 1.0, Style = RangeStyle.Both)]
    public double Angle { get => Get(.0); set => Set(value); }

    [Range(0, 10, 1, Style = RangeStyle.Both)]
    public int Indent { get => Get(0); set => Set(value); }

    [Range(3, 64, 1, Style = RangeStyle.Both)]
    public uint Sides { get => Get((uint)3); set => Set(value); }

    public PointShape Preview { get => Get<PointShape>(new()); set => Set(value); }

    public ShapeForm() : base() { }

    public override void OnPropertyChanged(PropertyEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(Preview): break;
            default:
                Preview.Points = Type switch
                {
                    ShapeType.Custom
                        => new(),
                    ShapeType.Polygon
                        => new(Shape.GetPolygon(Core.Numerics.Angle.GetRadian(Angle), new(1, 1), new(1, 1), Sides, 0)),
                    ShapeType.Star
                        => new(Shape.GetStar(new Int32Region(0, 0, 1, 1), Core.Numerics.Angle.GetRadian(Angle), Sides, Indent, 0))
                };
                break;
        }
    }
}