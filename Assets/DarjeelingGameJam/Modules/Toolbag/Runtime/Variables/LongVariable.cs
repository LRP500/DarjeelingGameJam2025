using UnityEngine;

namespace Modules.Toolbag.Variables
{
    [CreateAssetMenu(menuName = ContextMenuPath.Variables + "Long")]
    public class LongVariable : Variable<long>
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