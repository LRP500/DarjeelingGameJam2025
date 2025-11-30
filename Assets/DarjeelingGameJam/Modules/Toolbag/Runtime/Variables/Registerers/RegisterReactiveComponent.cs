using Modules.Toolbag.Variables.Lists;
using R3;
using R3.Triggers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modules.Toolbag.Variables.Registerers
{
    /// <summary>
    /// Register runtime component of type T to a ScriptableObject Reactive Variable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RegisterReactiveComponent<T> : MonoBehaviour where T : Behaviour
    {
        [SerializeField]
        private RegisterMode _mode;
        
        [SerializeReference]
        [ShowIf("@ _mode == RegisterMode.Single")]
        private ReactiveVariable<T> _runtimeReference;

        [SerializeReference]
        [ShowIf("@ _mode == RegisterMode.List")]
        private ReactiveListVariable<T> _runtimeList;
        
        [SerializeField]
        private bool _clearOnDestroy;
        
        private T _component;

        private void Awake()
        {
            if (_clearOnDestroy)
            {
                this.OnDestroyAsObservable()
                    .Subscribe(_ => Unregister());
            }
            
            Register();
        }

        private void Register()
        {
            _component = GetComponent();
            
            if (_mode == RegisterMode.Single)
            {
                _runtimeReference.SetValue(_component);
            }
            else if (_mode == RegisterMode.List)
            {
                _runtimeList.Values.Add(_component);
            }
        }
        
        private void Unregister()
        {
            if (_mode == RegisterMode.Single)
            {
                _runtimeReference.Clear();
            }
            else if (_mode == RegisterMode.List)
            {
                _runtimeList.Values.Remove(_component);
            }
        }
        
        protected virtual T GetComponent()
        {
            return GetComponent<T>();
        }
    }
}