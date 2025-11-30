using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modules.Toolbag.Variables
{
    public abstract class ReactiveVariable<T> : ScriptableObject
    {
        [ReadOnly]
        [SerializeField]
        private SerializableReactiveProperty<T> _property = new();
        
        public virtual T Value => _property.CurrentValue;
        public Observable<T> Property => _property;
        
        public virtual void SetValue(T value)
        {
            _property.Value = value;
        }
        
        public virtual void Clear()
        {
            SetValue(default);
        }
    }
}
