using UnityEngine;
using System.Linq;

public class FOVDetector : CircleDetector
{
	[Header("FOV Detector")]
	[SerializeField] private float maxDistance = 15f;
	[SerializeField] private float minDetectionAngle = 10f;
	[SerializeField] private float maxDetectionAngle = 10f;

	public bool targetInFOV;


	internal override void FixedUpdate()
	{
		FindTarget(); // Looks for a target

		DetectionAngle();
		CheckTargetDistance(maxDistance, out targetInFOV); // Check target distance
		CheckSightToTarget();
		DisengageCheck();
		//UpdateTargetInfo();
	//	UpdateSightInfo();
	}

	private void DetectionAngle() // Checks if target is within a certain angle
	{
		if (target == null) return;

		var dir = target.position - transform.position;
		var angle = Vector2.Angle(dir, Vector2.up);
		var realAngle = Vector3.Cross(dir, Vector2.up);
		if (realAngle.z > 0)
		{
			angle = 360 - angle;
		}
		CalculateAngles(angle, minDetectionAngle, maxDetectionAngle);
	}

	private void CalculateAngles(float angle, float minAngle, float maxAngle) // Calculate angles differently if facing left or right
	{
		if (enemy.rightFacing)
		{
			var flippedMinAngle = GetFlippedAngle(minAngle);
			var flippedMaxAngle = GetFlippedAngle(maxAngle);
			if (angle < flippedMinAngle && angle > flippedMaxAngle)
			{
				targetInFOV = true;
			}
			else
			{
				targetInFOV = false;
			}
		}
		else
		{
			if (angle > minAngle && angle < maxAngle)
			{
				targetInFOV = true;
			}
			else
			{
				targetInFOV = false;
			}
		}
	}

	private float GetFlippedAngle(float ang) // Flips the angles if facing right
	{
		return 360 - ang;
	}



/*	internal override void UpdateTargetInfo()
	{
		if (target != null && enemy.target == null) // If my target is not null and the enemy class target is null then set target
		{
			if (targetInFOV)
			{
				enemy.target = target;
			}
		}
		else if (target == null && enemy.target != null) // If my target is null and enemy target is not null then set enemy target to null
		{
			enemy.target = null;
			targetInFOV = false;
		}
	}*/

/*	internal override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		if (!showGizmos) return;
		var pos = transform.position;

		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(pos, maxDistance); // Draw the max range circle

		var minAngle = Quaternion.Euler(0, 0, minDetectionAngle * enemy.movementDirection) * Vector2.up * detectionRadius;
		var maxAngle = Quaternion.Euler(0, 0, maxDetectionAngle * enemy.movementDirection) * Vector2.up * detectionRadius;

		Gizmos.color = Color.yellow;
		Gizmos.DrawRay(transform.position, minAngle); // Draw min angle line
		Gizmos.DrawRay(transform.position, maxAngle); // Draw max angle line
	}*/

}
