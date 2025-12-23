namespace DarjeelingGameJam.Wind
{
    /// <summary>
    /// Interface pour les objets affectables par le vent (plantes, etc.)
    /// Utilisée par le WindTracer avec la Spatial Hash Grid.
    /// </summary>
    public interface IWindAffectable
    {
        /// <summary>
        /// Active ou désactive le vent sur l'objet avec une force donnée.
        /// </summary>
        /// <param name="enabled">True pour activer le vent, False pour le désactiver</param>
        /// <param name="strength01">Force normalisée du vent (0 = loin, 1 = au centre)</param>
        void SetWind(bool enabled, float strength01);
    }
}
