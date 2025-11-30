using System;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Modules.Toolbag.Extensions
{
    public static class DisposableExtensions
    {
        public static void AddToOnDisable(this IDisposable source, Component component)
        {
            source.AddTo(component);

            component
                .OnDisableAsObservable()
                .Take(1)
                .Subscribe(_ => source.Dispose());
        }
    }
}