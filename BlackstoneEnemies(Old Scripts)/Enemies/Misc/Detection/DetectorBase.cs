using System;
using UnityEngine;

public class DetectorBase : MonoBehaviour
{
	[Header("Detection Gizmos (Debugging)")]
	public bool showGizmos;

	// Layer masks
	[Header("Layer Masks")]
	[SerializeField] internal LayerMask playerLayer; // Layer to check for target
	[SerializeField] internal LayerMask sightLayer; // Layers to check line of sight with
	[SerializeField] internal float disengageTime;
	[SerializeField] internal bool ignoreSightCheck; // Set to true if target doesn't care about line of sight

	public Enemy enemy;
	public Transform target;
	internal bool targetInSight;
	internal Vector3 targetDir;
	internal float timeSinceLosingSight;

	internal virtual void OnEnable()
	{
		enemy = GetComponent<Enemy>();
	}

	internal virtual void FixedUpdate()
	{
		throw new NotImplementedException();
	}

	internal virtual void CheckSightToTarget() // Check sight to target
	{
		//if (!enemy.isActive) return;

		if (target == null) return;
		targetDir = target.transform.position - transform.position;
		var hit = Physics2D.Raycast(transform.position, targetDir, 50f, sightLayer);

		if (!hit) return;

		if (hit.transform.root.GetComponent<Player>())
		{
			if (targetInSight) return;
			targetInSight = true;
		}
		else
		{
			if (!targetInSight) return;
			targetInSight = false;
			timeSinceLosingSight = Time.time;
		}
	}

	internal virtual void DisengageCheck() // Checks if target has been out of sight for a set period, sets target to null if time has passed
	{
		if (ignoreSightCheck) return; // Ignores sight check
		if (target == null) return;
		if (targetInSight) return;
		if (Time.time > timeSinceLosingSight + disengageTime)
		{
			target = null;
		}
	}

	internal void CheckTargetDistance(float maxDist, out bool outOfRange) // Check distance to target and disengage
	{
		if (target == null)
		{
			outOfRange = false;
			return;
		}

		if (Vector2.Distance(transform.position, target.position) > maxDist)
		{
			target = null;
			targetInSight = false;
			outOfRange = false;

		}
		else
		{
			outOfRange = true;
		}
	}

/*	internal virtual void UpdateTargetInfo() // Updates the target info in the enemy class
	{
		if (target != null && enemy.target == null) // If my target is not null and the enemy class target is null then set target
		{
			enemy.target = target;
		}
		else if (target == null && enemy.target != null) // If my target is null and enemy target is not null then set enemy target to null
		{
			enemy.target = null;
		}
	}

	internal virtual void UpdateSightInfo() // Updates the enemies sight info in the enemy class
	{
		if (!enemy.targetInSight && targetInSight) // If my target is in sight and enemy target is not then set the enemy to true
		{
			enemy.targetInSight = true;
		}
		else if (enemy.targetInSight && !targetInSight) // If my target is no longer in sight and enemy target is in sight then set enemy to false
		{
			enemy.targetInSight = false;
		}
	}*/

	internal virtual void FindTarget()
	{
		throw new NotImplementedException();
	}

	internal virtual void OnDrawGizmosSelected()
	{
		if (!showGizmos) return;
		if (enemy == null) enemy = GetComponent<Enemy>();
		if (target == null) return;

		// Target line check, show green if target is in sight, red if not
		Gizmos.color = targetInSight ? Color.green : Color.red;
		Gizmos.DrawRay(transform.position, targetDir);
	}

}
