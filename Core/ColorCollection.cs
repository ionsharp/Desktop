using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Colors;
using Imagin.Core.Linq;
using Imagin.Core.Numerics;
using System;
using System.Collections.Generic;

namespace Imagin.Apps.Desktop;

[Categorize(false), Explicit, Serializable, ViewSource(ShowHeader = false)]
public class ColorCollection : ObservableCollection<ByteVector4>
{
    public override ByteVector4[] DefaultItems
    {
        get
        {
            var result = new List<ByteVector4>() { ByteVector4.Black, ByteVector4.White };
            new Type[] { typeof(Colors1), typeof(Colors2), typeof(Colors3), typeof(Colors4), typeof(Colors5) }
                .ForEach(i => i.GetFields().ForEach(j => result.Add(new ByteVector4((string)j.GetValue(null)))));
            return result.ToArray();
        }
    }

    public ColorCollection() : base() { }
}