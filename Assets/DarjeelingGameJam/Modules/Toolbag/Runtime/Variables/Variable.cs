using UnityEngine;

namespace Modules.Toolbag.Variables
{
    public abstract class Variable<T> : ScriptableObject
    {
        [SerializeField]
        private T _value;

        public T Value => _value;

        public void SetValue(T value)
        {
            _value = value;
        }

        public void Clear()
        {
            _value = default;
        }
        
        public static implicit operator T(Variable<T> variable)
        {
            return variable.Value;
        }
    }
}