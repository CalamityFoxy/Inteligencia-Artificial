using UnityEngine;

public interface IFlagCarrier
{
    Transform Transform { get; }
    Transform FlagHolder { get; }
    Team Team { get; }
    bool HasFlag { get; }
    Flag CurrentFlag { get; }
    void SetFlag(Flag flag);
    void ClearFlag();
}