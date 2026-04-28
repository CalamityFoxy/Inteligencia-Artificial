using UnityEngine;

public enum FlagState
{
    Home,
    Carried,
    Dropped
}
public class Flag : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Team ownerTeam;
    [SerializeField] private Transform homePoint;

    Vector3 startRotation;
    Collider grabTrigger;
    public Team OwnerTeam => ownerTeam;
    public FlagState State { get; private set; } = FlagState.Home;
    public IFlagCarrier Carrier { get; private set; }

    private void Awake()
    {
        grabTrigger = GetComponent<Collider>();
        grabTrigger.enabled = true;
        startRotation = transform.rotation.eulerAngles;
    }

    private void OnTriggerEnter(Collider other)
    {
        var carrier = other.GetComponent<IFlagCarrier>();
        if (carrier == null) return;
        if (State == FlagState.Carried) return;
        if (carrier.Team == ownerTeam && State != FlagState.Dropped) return;

        PickUp(carrier);
    }

    public void PickUp(IFlagCarrier carrier)
    {
        Carrier = carrier;
        grabTrigger.enabled = false;
        carrier.SetFlag(this);
        State = FlagState.Carried;
    }

    public void Drop(Vector3 position)
    {
        Carrier?.ClearFlag();
        Carrier = null;
        transform.SetParent(null);
        transform.SetPositionAndRotation(position, Quaternion.Euler(startRotation));
        State = FlagState.Dropped;
        grabTrigger.enabled = true;
    }

    public void ReturnHome()
    {
        Carrier = null;
        State = FlagState.Home;

        transform.SetParent(null);
        transform.position = homePoint.position;
        grabTrigger.enabled = true;
    }
}
