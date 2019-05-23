using UnityEngine;

public class ParticleProjectile : Projectile
{
	[Header("Particles")]
	public ParticleSystem startingParticles;
	public GameObject contactParticles;





	internal override void Start()
	{
		base.Start();

		startingParticles.Play();

	}

	internal override void OnCollisionEnter2D(Collision2D other)
	{
		base.OnCollisionEnter2D(other);

		if (other.gameObject == damageSource) return;

		startingParticles.Stop();
		var go = Instantiate(contactParticles, transform.position, transform.rotation);
		Destroy(go, 1f);
	}

	internal override void OnTriggerEnter2D(Collider2D other)
	{
		base.OnTriggerEnter2D(other);

		if (other.gameObject == damageSource) return;

		startingParticles.Stop();
		var go = Instantiate(contactParticles, transform.position, transform.rotation);
		Destroy(go, 1f);
	}


}
