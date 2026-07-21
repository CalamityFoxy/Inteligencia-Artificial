using UnityEngine;

public enum FlagState
{
    Home,
    Carried,
    Dropped
}


public class Flag : MonoBehaviour
{

    [ContextMenu("TEST - Forzar Drop")]
    private void DebugForceDrop()
    {
        Drop(transform.position); //esto es para testear la bandera
    }


    [Header("Configuration")]
    [SerializeField] private Team ownerTeam;
    //[SerializeField] private Transform homePoint;

    Vector3 startRotation;
    Collider grabTrigger;
    public Team OwnerTeam => ownerTeam;
    public FlagState State { get; private set; } = FlagState.Home;
    public IFlagCarrier Carrier { get; private set; }

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private void Awake()
    {
        grabTrigger = GetComponent<Collider>();

        homePosition = transform.position;
        homeRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        var carrier = other.GetComponentInParent<IFlagCarrier>();
        if (carrier == null) return;
        if (State == FlagState.Carried) return;

        
        if (carrier.Team == ownerTeam)
        {
            if (State == FlagState.Dropped) ReturnHome();
            return;
        }

        PickUp(carrier);
    }

    public void PickUp(IFlagCarrier carrier)
    {
        Carrier = carrier;
        grabTrigger.enabled = false;
        carrier.SetFlag(this);
        State = FlagState.Carried;

        
        transform.SetParent(carrier.FlagHolder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
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
        Carrier?.ClearFlag();
        Carrier = null;
        State = FlagState.Home;

        transform.SetParent(null);
        transform.SetPositionAndRotation(homePosition, homeRotation);

        grabTrigger.enabled = true;
    }
}
