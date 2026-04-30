using UnityEngine;

public class BirdFlyAwayTrigger : MonoBehaviour
{
    [Header("Wwise Event")]
    public AK.Wwise.Event birdFlyAwayEvent;

    [Header("Trigger Settings")]
    public string playerTag = "Player";
    public float cooldown = 20f;

    private bool canTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!canTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        birdFlyAwayEvent.Post(gameObject);
        StartCoroutine(CooldownRoutine());
    }

    private System.Collections.IEnumerator CooldownRoutine()
    {
        canTrigger = false;
        yield return new WaitForSeconds(cooldown);
        canTrigger = true;
    }
}