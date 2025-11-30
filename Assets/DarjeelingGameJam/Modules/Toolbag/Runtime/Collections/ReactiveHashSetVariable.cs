using ObservableCollections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modules.Toolbag.Collections
{
    public abstract class ReactiveHashSetVariable<T>: ScriptableObject
    {
        [ShowInInspector]
        private readonly ObservableHashSet<T> _value = new();
        
        public ObservableHashSet<T> Values => _value;

        public void Add(T value)
        {
            _value.Add(value);
        }
        
        public void Remove(T value)
        {
            _value.Remove(value);
        }
        
        public void Clear()
        {
            _value.Clear();
        }
    }
}