using UnityEngine;

namespace Modules.Toolbag.Variables
{
    [CreateAssetMenu(menuName = ContextMenuPath.Variables + "Float")]
    public class FloatVariable : Variable<float>
    {
        public void Add(float value)
        {
            SetValue(Value + value);
        }

        public void Subtract(float value)
        {
            SetValue(Value - value);
        }

        public void MultiplyBy(float value)
        {
            SetValue(Value * value);
        }

        public void DivideBy(float value)
        {
            SetValue(value == 0 ? Value : Value / value);
        }
    }
}
