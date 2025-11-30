using UnityEngine;

namespace Modules.Toolbag.Variables
{
    [CreateAssetMenu(menuName = ContextMenuPath.Variables + "Int")]
    public class IntVariable : Variable<int>
    {
        public void Increment()
        {
            SetValue(Value + 1);
        }

        public void Decrement()
        {
            SetValue(Value - 1);
        }

        public void Add(int value)
        {
            SetValue(Value + value);
        }

        public void Subtract(int value)
        {
            SetValue(Value - value);
        }

        public void MultiplyBy(int value)
        {
            SetValue(Value * value);
        }

        public void DivideBy(int value)
        {
            SetValue(value == 0 ? Value : Value / value);
        }
    }
}
