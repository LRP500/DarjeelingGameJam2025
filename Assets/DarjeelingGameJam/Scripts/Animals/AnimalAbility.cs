using UnityEngine;

namespace DarjeelingGameJam.Animals
{
    public abstract class AnimalAbility : MonoBehaviour
    {
        public Animal Owner { get; set; }

        public abstract void Initialize();
    }
}