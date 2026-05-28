/// <summary>
/// Runtime payload for atmospheric thought popups.
/// Carries priority and interruption intent so future horror systems can share
/// one popup layer without knowing about the UI implementation.
/// </summary>
public readonly struct ThoughtMessageData
{
    /// <summary>
    /// Message displayed to the player.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Time, in seconds, that the message should remain readable between fades.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Higher priority messages are allowed to interrupt lower priority messages.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Whether this message may interrupt the current popup when its priority allows it.
    /// </summary>
    public bool CanInterrupt { get; }

    public ThoughtMessageData(string message, float duration, int priority, bool canInterrupt)
    {
        Message = message;
        Duration = duration;
        Priority = priority;
        CanInterrupt = canInterrupt;
    }
}
