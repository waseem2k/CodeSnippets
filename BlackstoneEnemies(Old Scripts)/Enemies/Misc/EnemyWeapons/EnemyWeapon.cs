using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
	//[HideInInspector] 
	public bool doDamage; // Set by animator on the frame the weapon does it's forward thrust

	private float weaponDamage = 5, slamForce = 1; // Damage and force to apply to the target
	private GameObject damageSource; // The source object that initiated the attack
	//[HideInInspector]
	public Player player;
//	[HideInInspector]
	public bool damageDealt;

	private AudioSource audioSource;
	private AudioClip queuedClip;

	// Main attack to be called from Ability Machine
	public void DoAttack(float damage, float force, GameObject source)
	{
		SetParams(damage, force, source);
	}

	public void QueueSoundClip(AudioSource _source, AudioClip _clip)
	{
		audioSource = _source;
		queuedClip = _clip;
	}

	private void PlayeQueuedSound()
	{
		if (queuedClip != null) audioSource.PlayOneShot(queuedClip);
	}

	// Sets the damage, force and damage source
	private void SetParams(float damage, float force, GameObject source)
	{
		weaponDamage = damage;
		damageSource = source;
		slamForce = force;
		damageDealt = false;
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (!doDamage) return; // Check if doing damage

		if (player == null) player = other.transform.root.GetComponentInChildren<Player>(); // Look for player contact
		if (player == null) return;

		if (!damageDealt)
		{
			// Apply Force
			var dir = player.transform.position - transform.position;
			player.gameObject.GetComponent<Rigidbody2D>().AddForce(dir * slamForce, ForceMode2D.Impulse);

			// Do Damage
			player.gameObject.GetComponent<Health>().Damage(weaponDamage, damageSource);

			// Play Audio
			PlayeQueuedSound();

			// Disable attack
			damageDealt = true;

			// Reset
			player = null;
		}

		doDamage = false;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!doDamage) return; // Check if doing damage

		player = other.transform.root.GetComponentInChildren<Player>(); // Look for player contact
		if (player == null) return;
		Debug.Log("Making Contact");
		if (!damageDealt)
		{
			// Apply Force
			var dir = player.transform.position - transform.position;
			player.gameObject.GetComponent<Rigidbody2D>().AddForce(dir * slamForce, ForceMode2D.Impulse);

			// Do Damage
			player.gameObject.GetComponent<Health>().Damage(weaponDamage, damageSource);

			// Play Audio
			PlayeQueuedSound();

			// Disable attack
			damageDealt = true;

			// Reset
			player = null;
		}

		doDamage = false;
	}
}
