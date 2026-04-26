using UnityEngine;

public interface IFlagCarrier
{
    Team Team { get; }
    Transform Transform { get; }
    bool notDead { get; }
}