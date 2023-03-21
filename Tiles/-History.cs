using Imagin.Core;
using Imagin.Core.Collections.ObjectModel;
using Imagin.Core.Linq;
using System;
using System.Collections.Generic;

namespace Imagin.Apps.Desktop;

public class ChangedValue
{
    public readonly object Source;

    ///

    public readonly string Name;

    public readonly ReadOnlyValue Value;

    ///

    public ChangedValue(object source, string name, object oldValue, object newValue) : base()
    {
        Source = source; Name = name; Value = new(oldValue, newValue);
    }
}

public class Change
{
    public virtual string Title { get; }

    public Change() : base() { }
}

public abstract class ValueChange : Change
{
    public override string Title => $"Changed '{Change.Source.GetType().Name}.{Change.Name}'";

    public readonly ChangedValue Change;

    public ValueChange(ChangedValue input) : base() => Change = input;
}

public abstract class MultipleValueChange : Change
{
    public override string Title => $"Changed '{Changes.First().Source.GetType().Name}.{Changes.First().Name}' (multiple)";

    public readonly ChangedValue[] Changes;

    public MultipleValueChange(ChangedValue[] input) : base() => Changes = input;
}

public class FieldChange : ValueChange
{
    public FieldChange(ChangedValue input) : base(input) { }
}

public class PropertyChange : ValueChange
{
    public PropertyChange(ChangedValue input) : base(input) { }
}

public class MultipleFieldChange : MultipleValueChange
{
    public MultipleFieldChange(ChangedValue[] input) : base(input) { }
}

public class MultiplePropertyChange : MultipleValueChange
{
    public MultiplePropertyChange(ChangedValue[] input) : base(input) { }
}

public class History : ObservableHistory<Change>
{
    public History()
    {
        Current.Add(this);
        Limit = new(10, Core.Collections.Limit.Actions.RemoveFirst);
    }

    public void Add<TOwner, TValue>(IEnumerable<TOwner> items, string memberName, Func<TOwner, TValue> get)
    {
        var result = new List<ChangedValue>();

        items.ForEach(i =>
        {
            var oldValue = i.GetPropertyValue(memberName);
            var newValue = get(i);

            result.Add(new(i, memberName, oldValue, newValue));
            i.SetPropertyValue(memberName, newValue);
        });

        Add(new MultiplePropertyChange(result.ToArray()));
    }

    public void Redo()
    {
        Redo(i =>
        {
            if (i is FieldChange a)
                a.Change.Source.SetFieldValue(a.Change.Name, a.Change.Value.New);

            if (i is PropertyChange b)
                b.Change.Source.SetPropertyValue(b.Change.Name, b.Change.Value.New);

            if (i is MultipleFieldChange c)
                c.Changes.ForEach(j => j.Source.SetFieldValue(j.Name, j.Value.New));

            if (i is MultiplePropertyChange d)
                d.Changes.ForEach(j => j.Source.SetPropertyValue(j.Name, j.Value.New));
        });
    }

    public void Undo()
    {
        Undo(i =>
        {
            if (i is FieldChange a)
                a.Change.Source.SetFieldValue(a.Change.Name, a.Change.Value.Old);

            if (i is PropertyChange b)
                b.Change.Source.SetPropertyValue(b.Change.Name, b.Change.Value.Old);

            if (i is MultipleFieldChange c)
                c.Changes.ForEach(j => j.Source.SetFieldValue(j.Name, j.Value.Old));

            if (i is MultiplePropertyChange d)
                d.Changes.ForEach(j => j.Source.SetPropertyValue(j.Name, j.Value.Old));
        });
    }
}