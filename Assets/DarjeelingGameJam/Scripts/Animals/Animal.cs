using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace DarjeelingGameJam.Animals
{
    public class Animal : MonoBehaviour
    {
        private readonly List<AnimalAbility> _abilities = new();

        private void Awake()
        {
            var abilities = GetComponentsInChildren<AnimalAbility>();

            foreach (var ability in abilities)
            {
                _abilities.Add(ability);
                ability.Owner = this;
                ability.Initialize();
            }
        }

        private void OnEnable()
        {
            _abilities.ForEach(x => x.Enable());
        }

        private void OnDisable()
        {
            _abilities.ForEach(x => x.Disable());
        }

        public T GetAbility<T>() where T : AnimalAbility
        {
            return _abilities.OfType<T>().FirstOrDefault();
        }
    }
}