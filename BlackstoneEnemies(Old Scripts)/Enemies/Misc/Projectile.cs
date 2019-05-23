using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public float destroyAfter = 2f;

	[HideInInspector] public Rigidbody2D rb;

	protected float projectileDamage;
	public GameObject damageSource;
	public LayerMask groundLayer;

	public void SetParams(float damage, GameObject source) // Set the damage and source
	{
		projectileDamage = damage;
		damageSource = source;
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	internal virtual void Start()
	{
		if (!damageSource) return;
		var sourceCollider = damageSource.GetComponents<Collider2D>();
		var sourceCollider2 = damageSource.GetComponentsInChildren<Collider2D>();
		var projectileCollider = GetComponent<Collider2D>();
		foreach (var item in sourceCollider)
		{
			Physics2D.IgnoreCollision(item, projectileCollider);
		}
		foreach (var item in sourceCollider2)
		{
			Physics2D.IgnoreCollision(item, projectileCollider);
		}
		StartCoroutine(ToggleCollision());
		Destroy(gameObject, destroyAfter);
	}

	internal virtual void FixedUpdate()
	{
		if (!damageSource) return;
		UpdateRotation();
	}

	internal virtual void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject == damageSource) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
			Destroy(gameObject);

		if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
		{
			var tarHealth = other.gameObject.GetComponent<Health>();
			if (tarHealth == null) other.transform.root.GetComponentInChildren<Health>();
			if (!tarHealth) return;
			tarHealth.Damage(projectileDamage, gameObject);
			Destroy(gameObject);
		}
	}

	internal virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject == damageSource) return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
			Destroy(gameObject);

		if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
		{
			var tarHealth = other.gameObject.GetComponent<Health>();
			if (tarHealth == null) other.transform.root.GetComponentInChildren<Health>();
			if (!tarHealth) return;
			tarHealth.Damage(projectileDamage, gameObject);
			Destroy(gameObject);
		}
	}

	private void UpdateRotation()
	{
		var moveDirection = rb.velocity;
		if (moveDirection == Vector2.zero) return;
		var angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	private IEnumerator ToggleCollision()
	{
		var col = Physics2D.OverlapCircle(transform.position, 2, groundLayer);
		Physics2D.IgnoreCollision(col, GetComponent<Collider2D>(), true);
		yield return new WaitForSeconds(1f);
		Physics2D.IgnoreCollision(col, GetComponent<Collider2D>(), false);
	}
}
