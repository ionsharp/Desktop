using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Media;
using Imagin.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Imagin.Apps.Desktop;

[Categorize(false), Explicit, Serializable, ViewSource(ShowHeader = false)]
public class ShapeCollection : ObservableCollection<NamableCategory<Shape>>
{
    public override NamableCategory<Shape>[] DefaultItems
    {
        get
        {
            var result = new List<NamableCategory<Shape>>
            {
                new(nameof(Circle),
                    nameof(Circle),
                    new Circle()),

                new(nameof(Heart),
                    nameof(Heart),
                    new Heart()),
            };

            ///

            uint sides = 3;
            foreach (var i in Shape.GetPolygons(3, 8, 90))
            {
                result.Add(new(Shape.PolygonNames[sides], "Polygon", i));
                sides++;
            }

            ///

            var rectangle = new PointShape(Shape.GetPolygon(Angle.GetRadian(45), new Point(1, 1), new Size(1, 1), 4, 0));
            rectangle.Translate(); rectangle.Normalize();

            result.Add(new("Rectangle", "Polygon", rectangle));

            ///

            sides = 3;
            foreach (var i in Shape.GetStars(3, 8))
            {
                result.Add(new($"{sides}-Star", "Star", i));
                sides++;
            }

            return result.ToArray();
        }
    }

    public ShapeCollection() : base() { }
}