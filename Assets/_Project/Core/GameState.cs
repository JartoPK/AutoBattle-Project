namespace AutoBattle.Core
{
    /// <summary>
    /// Fases de alto nivel del juego. El combate (Combat) es un estado más, al
    /// que la capa Meta entra desde Deployment y del que vuelve a PostCombat.
    /// </summary>
    public enum GameState
    {
        Boot,         // Arranque: carga datos persistentes, inicializa sistemas.
        MainMenu,     // Menú principal.
        CampaignMap,  // Mapa de conquista: elegir nodo (Fase 7).
        Base,         // Gestión: roster, árbol de mejoras, herencia (Fase 8).
        Deployment,   // Preparación pre-combate: ver rival, elegir 3 tropas, cargar órdenes.
        Combat,       // En batalla (lo gestiona la capa Combat).
        PostCombat    // Resolución: bajas permanentes, cicatrices, recompensas.
    }
}
