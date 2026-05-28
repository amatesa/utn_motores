/// <summary>
/// High-level pacing phases for Silent Escape.
/// These phases are intentionally broad so enemy pressure, lantern logic,
/// ghost events, UI, and audio can react without depending on each other.
/// </summary>
public enum GamePhase
{
    /// <summary>
    /// Opening/tutorial phase before the player is fully exposed to danger.
    /// </summary>
    Intro,

    /// <summary>
    /// Low-pressure orphanage exploration and narrative discovery.
    /// </summary>
    Exploration,

    /// <summary>
    /// Progression phase focused on finding keys and unlocking routes.
    /// </summary>
    KeyHunt,

    /// <summary>
    /// Late-game escape pressure after major progression has opened.
    /// </summary>
    Escape,

    /// <summary>
    /// Highest-pressure final sequence leading to the main entrance escape.
    /// </summary>
    FinalEscape
}
