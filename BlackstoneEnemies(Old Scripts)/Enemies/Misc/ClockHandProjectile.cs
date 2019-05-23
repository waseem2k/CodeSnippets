using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ClockHandProjectile : Projectile
{
	public float rotationSpeed;
	private bool autoRotate;

	internal override void Start()
	{
		base.Start();

		AutoRotate(true);
	}

	public void AutoRotate(bool _rotate)
	{
		autoRotate = _rotate;
	}

	internal override void FixedUpdate()
	{
		//base.FixedUpdate();

		if (autoRotate)
		{
			transform.Rotate(0,0, 6.0f * rotationSpeed * Time.deltaTime);
		}
	}
	internal override void OnCollisionEnter2D(Collision2D other)
	{
		//base.OnCollisionEnter2D(other);

		if (other.gameObject == damageSource) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Player") ||
			other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
		{
			var tarHealth = other.gameObject.GetComponent<Health>();
			if (tarHealth == null) other.transform.root.GetComponentInChildren<Health>();
			if (!tarHealth) return;
			tarHealth.Damage(projectileDamage, gameObject);
			Destroy(gameObject);
		}

	}

	internal override void OnTriggerEnter2D(Collider2D other)
	{
		//base.OnTriggerEnter2D(other);

		if (other.gameObject == damageSource) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Player") ||
			other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
		{
			var tarHealth = other.gameObject.GetComponent<Health>();
			if (tarHealth == null) other.transform.root.GetComponentInChildren<Health>();
			if (!tarHealth) return;
			tarHealth.Damage(projectileDamage, gameObject);
			Destroy(gameObject);
		}
	}

}
