using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modules.Toolbag.Variables.Lists
{
    public abstract class AssetListVariable<T> : ScriptableObject where T : class
    {
        [AssetList]
        [SerializeField]
        private List<T> _items;

        public IReadOnlyList<T> Items => _items;

        public T Get(int index)
        {
            return Items.Count > index ? Items[index] : null;
        }

        public T GetRandom()
        {
            return Items[Random.Range(0, Items.Count)];
        }
    }
}