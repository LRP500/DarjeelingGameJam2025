namespace DarjeelingGameJam.Plants
{
    /// <summary>
    /// Compteur statique ultra-léger du nombre de plantes actives.
    /// Coût: juste un int++ et int-- (quasi gratuit).
    /// </summary>
    public static class PlantCounter
    {
        private static int _count = 0;

        public static int Count => _count;

        public static void Increment() => _count++;

        public static void Decrement()
        {
            if (_count > 0) _count--;
        }

        public static void Reset() => _count = 0;
    }
}
