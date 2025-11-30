using UnityEditor;
using UnityEngine;

namespace Modules.Toolbag.Utils
{
#if UNITY_EDITOR

    public static class ScriptableObjectUtils
    {
        public static T CreateSubAsset<T>(ScriptableObject source) where T : ScriptableObject
        {
            var subAsset = ScriptableObject.CreateInstance<T>();
            subAsset.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(subAsset, source);
            AssetDatabase.SaveAssets();
            return subAsset;
        }

        public static T CreateSubAsset<T>(ScriptableObject source, System.Type type) where T : ScriptableObject
        {
            var subAsset = ScriptableObject.CreateInstance(type);
            subAsset.name = type.Name;
            AssetDatabase.AddObjectToAsset(subAsset, source);
            Undo.RegisterCreatedObjectUndo(subAsset, $"CreateSubAsset ({subAsset.name})");
            AssetDatabase.SaveAssets();
            return subAsset as T;
        }
        
        public static void RemoveSubAsset(Object asset)
        {
            Undo.DestroyObjectImmediate(asset);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}