using System.Collections.Generic;
using System.Linq;
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
            }

            foreach (var ability in abilities)
            {
                ability.Initialize();
            }
        }

        public T GetAbility<T>() where T : AnimalAbility
        {
            return _abilities.OfType<T>().FirstOrDefault();
        }
    }
}