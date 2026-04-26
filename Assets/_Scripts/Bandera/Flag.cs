using UnityEngine;

public enum FlagState
{
    Home,
    Carried,
    Dropped
}
[RequireComponent(typeof(Collider))]
public class Flag : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Team ownerTeam;
    [SerializeField] private Transform homePoint;
    [SerializeField] private Vector3 carryOffset = new Vector3(0, 1.5f, 0); // acá es para que quede un poquito arriba del player / enemigo.

    public Team OwnerTeam => ownerTeam;
    public FlagState State { get; private set; } = FlagState.Home;
    public IFlagCarrier Carrier { get; private set; }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (homePoint != null)
            transform.position = homePoint.position;
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
        State = FlagState.Carried;

       
        transform.SetParent(carrier.Transform);
        transform.localPosition = carryOffset;
    }

    public void Drop(Vector3 position)
    {
        Carrier = null;
        State = FlagState.Dropped;

      
        transform.SetParent(null);
        transform.position = position;
    }

    public void ReturnHome()
    {
        Carrier = null;
        State = FlagState.Home;

        transform.SetParent(null);
        transform.position = homePoint.position;
    }
}