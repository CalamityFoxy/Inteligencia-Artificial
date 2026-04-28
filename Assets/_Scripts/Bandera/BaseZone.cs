using UnityEngine;

public class BaseZone : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private Flag ownFlag;
    private void OnTriggerEnter(Collider other)
    {
        var carrier = other.GetComponent<IFlagCarrier>();
        if (carrier == null) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null || !player.HasFlag) return;

        Flag carriedFlag = player.CurrentFlag;

        if (carriedFlag.OwnerTeam == team) return;

        if (ownFlag.State != FlagState.Home) return;

        Capture(player, carriedFlag);
    }

    private void Capture(PlayerController player, Flag enemyFlag)
    {
        Debug.Log("CAPTURE!");

        CTF_GameManager.Instance.AddScore(player.Team);

        player.ClearFlag();
        enemyFlag.ReturnHome();
    }
}