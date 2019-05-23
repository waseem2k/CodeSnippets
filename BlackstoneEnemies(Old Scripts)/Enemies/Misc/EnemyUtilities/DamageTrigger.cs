using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
	public float damageAmount;
	public GameObject source; // The source object dealing this damage
	public bool bypassInvul; // Ignores invulnerability

	private void OnTriggerEnter2D(Collider2D other)
	{
		Player player = other.transform.root.GetComponentInChildren<Player>();

		if (player == null) return;

		if(bypassInvul) player.health.invulnerable = false;
		player.health.Damage(damageAmount, source);
	}
}
