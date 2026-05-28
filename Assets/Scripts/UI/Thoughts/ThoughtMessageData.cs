public readonly struct ThoughtMessageData
{
    public string Message { get; }

    public float Duration { get; }

    public int Priority { get; }

    public bool CanInterrupt { get; }

    public ThoughtType Type { get; }

    public ThoughtMessageData(
        string message,
        float duration,
        int priority,
        bool canInterrupt,
        ThoughtType type
    )
    {
        Message = message;
        Duration = duration;
        Priority = priority;
        CanInterrupt = canInterrupt;
        Type = type;
    }
}
