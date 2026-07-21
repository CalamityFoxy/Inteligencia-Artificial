using UnityEngine;

public class BaseZone : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private Flag ownFlag;

    private void OnTriggerEnter(Collider other)
    {
        var carrier = other.GetComponentInParent<IFlagCarrier>();
        if (carrier == null) return;

        if (carrier.Team != team) return;      // esta base es solo para los de mi equipo
        if (!carrier.HasFlag) return;

        Flag carriedFlag = carrier.CurrentFlag;
        if (carriedFlag.OwnerTeam == team) return;   // no capturo mi propia bandera
        
      

        CTF_GameManager.Instance.AddScore(carrier.Team);
        carriedFlag.ReturnHome();
    }
}