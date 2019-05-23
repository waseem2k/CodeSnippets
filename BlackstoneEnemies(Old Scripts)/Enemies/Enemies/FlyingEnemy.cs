using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : GenericEnemy
{
	[Header("Flight Patrol")]
	public float verticalSpeed;
	public float amplitude;

	[Header("Hover")]
	public float hoverForce; // The ammount of force to apply to keep a hover
	public float heightFromPlayer; // The max height from player to attain while hovering

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	private void FixedUpdate()
	{
		if (!health.isAlive) return;
		CheckState();
		UpdateAnimationController();

		if (!playerEngaged)
		{
			Flight();
			Patrol(false);
		}
		else
		{
			Hover(CalculateHoverDirection());
			CheckTargetDirection(player.position);
			MoveToPlayer(player);
		}

	}

	private void Flight() // Flight patrol
	{
		var yPos = Mathf.Sin(Time.realtimeSinceStartup * verticalSpeed) * amplitude + transform.position.y;
		transform.position = new Vector2(transform.position.x, yPos);
	}

	private void Hover(int dir) // AI hovers based on the height diff
	{
		rb.AddForce(Vector2.up * dir * hoverForce, ForceMode2D.Force);
	}

	private int CalculateHoverDirection() // Calculates the height diff from the player
	{
		//float calculatedHeight = player.position.y - Position.y; // The current height difference

		if (Position.y > player.position.y + heightFromPlayer) // Bird is too high, needs to move down
		{
			return -1;
		}

		return 1;
	}
}
