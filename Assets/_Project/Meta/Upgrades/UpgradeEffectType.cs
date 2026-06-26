namespace AutoBattle.Meta.Upgrades
{
    /// <summary>
    /// Tipos de efecto de un nodo. Dos familias:
    ///  - Efectos META: los aplica de verdad esta capa (mutan el MetaGameState).
    ///  - Flags de COMBATE: solo se registran como "desbloqueado"; los lee la
    ///    capa Combat (otra persona) para habilitar contenido. effectTargetId
    ///    indica QUÉ se desbloquea (id de orden, pasiva, etc.).
    /// </summary>
    public enum UpgradeEffectType
    {
        None,

        // --- Efectos meta (aplicados aquí) ---
        IncreaseBaseLevel,        // effectValue = niveles a sumar.
        IncreaseRosterCap,        // effectValue = ranuras extra de roster.
        RecruitQualityBonus,      // effectValue = desplazamiento de percentil [0..1] al reclutar.
        InheritanceQualityBonus,  // effectValue = se suma a statInheritPercent.

        // --- Flags de combate (solo se registran) ---
        UnlockOrder,              // effectTargetId = id de la orden.
        UnlockPassive,            // effectTargetId = id de la pasiva.
        UnlockGlobalOrder,        // effectTargetId = id de la orden global.
        UnlockThirdOrderSlot,     // 3.ª ranura de orden (sin target).
        IncreaseStatCap           // effectTargetId = stat; effectValue = incremento.
    }
}
