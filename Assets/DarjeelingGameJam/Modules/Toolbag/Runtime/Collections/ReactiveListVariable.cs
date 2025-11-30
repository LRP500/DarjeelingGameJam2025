using System.Collections.Generic;
using ObservableCollections;
using UnityEngine;

namespace Modules.Toolbag.Variables.Lists
{
    public abstract class ReactiveListVariable : ScriptableObject
    {
        public abstract int Count { get; }
    }
    
    public abstract class ReactiveListVariable<T> : ReactiveListVariable
    {
        public ObservableList<T> Values { get; } = new();
        public override int Count => Values.Count;

        public void SetValues(IEnumerable<T> values)
        {
            Values.Clear();
            Values.AddRange(values);
        }

        public void Add(T value)
        {
            Values.Add(value);
        }

        public void AddRange(IEnumerable<T> values)
        {
            Values.AddRange(values);
        }
        
        public void Remove(T value)
        {
            Values.Remove(value);
        }

        public void Clear()
        {
            Values.Clear();
        }
    }
}