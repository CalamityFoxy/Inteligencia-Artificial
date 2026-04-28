using UnityEngine;

public interface IFlagCarrier
{
    Transform Transform { get; }
    Team Team { get; }

    void SetFlag(Flag flag);
    void ClearFlag();
}