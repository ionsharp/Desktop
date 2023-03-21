using Imagin.Core;
using System;

namespace Imagin.Apps.Desktop;

[Serializable]
public class SearchEngine : Namable<string>
{
    [Name("Url")]
    public override string Value { get => base.Value; set => base.Value = value; }

    public SearchEngine() : this("Untitled", "") { }

    public SearchEngine(string name, string value) : base(name, value) { }
}