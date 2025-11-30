using UnityEngine;

namespace Modules.Toolbag.Variables
{
    [CreateAssetMenu(menuName = ContextMenuPath.ReactiveVariables + "Long")]
    public class LongReactiveVariable : ReactiveVariable<long>
    {
        public void Add(long value)
        {
            SetValue(Value + value);
        }

        public void Add(int value)
        {
            SetValue(Value + value);
        }
        
        public void Subtract(long value)
        {
            SetValue(Value - value);
        }
        
        public void Subtract(int value)
        {
            SetValue(Value - value);
        }
    }
}