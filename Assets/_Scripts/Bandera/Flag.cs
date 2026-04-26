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
    Collider grabTrigger;
    public Team OwnerTeam => ownerTeam;
    public FlagState State { get; private set; } = FlagState.Home;
    public IFlagCarrier Carrier { get; private set; }

    private void Awake()
    {
        grabTrigger = GetComponent<Collider>();
        grabTrigger.enabled = true;
    }

    private void Update()
    {

        if (State == FlagState.Carried && Carrier != null && !Carrier.notDead)
        {
            Drop(Carrier.Transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var carrier = other.GetComponent<IFlagCarrier>();
        if (carrier == null) return;
        if (!carrier.notDead) return;
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
        transform.SetParent(carrier.FlagHolder);
    }

    public void Drop(Vector3 position)
    {
        Carrier = null;
        State = FlagState.Dropped;

        Carrier?.ClearFlag();
        transform.SetParent(null);
        transform.position = position;
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
