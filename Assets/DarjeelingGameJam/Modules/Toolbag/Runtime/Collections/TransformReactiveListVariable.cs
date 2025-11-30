using Modules.Toolbag;
using Modules.Toolbag.Variables.Lists;
using UnityEngine;

namespace UnityTools.Runtime.Lists
{
    [CreateAssetMenu(menuName = ContextMenuPath.ReactiveLists + "Transform")]
    public class TransformReactiveListVariable : ReactiveListVariable<Transform>
    { }
}