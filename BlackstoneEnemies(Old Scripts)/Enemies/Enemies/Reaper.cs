using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reaper : MonoBehaviour
{
	public float moveSpeed; // The speed at which the reaper moves

	[HideInInspector] public Transform target;
	private Rigidbody2D rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate()
	{
		if (target == null) return;

		Vector2 dir = target.position - transform.position;
		rb.AddForce(dir * moveSpeed, ForceMode2D.Force);
		moveSpeed *= 1.001f;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		Player player = other.transform.root.GetComponentInChildren<Player>();

		if (player == null) return;

		player.health.invulnerable = false;
		player.health.Damage(100000, gameObject);
	}
}
