using System.Linq;
using UnityEngine;

public class CircleDetector : DetectorBase
{
	[Header("Circle Detector")]
	[SerializeField] internal float detectionRadius = 10f;

	public Collider2D[] DetectionCircle(LayerMask targetLayer)
	{
		return Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
	}

	internal override void FixedUpdate()
	{
		/*if (!enemy.isActive)
		{
			if (target) target = null;

			return;
		}*/

		FindTarget();
		CheckSightToTarget();
		DisengageCheck();
		//UpdateTargetInfo();
		//UpdateSightInfo();
	}

	internal override void FindTarget()
	{
		if (target != null) return;

		var targets = DetectionCircle(playerLayer);
		target = (from tar in targets where tar.transform.root.GetComponent<Player>() select tar.transform).FirstOrDefault();
	}

	private void OnValidate()
	{
		if (detectionRadius < 0) detectionRadius = 0;
	}

	internal override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		if (!showGizmos) return;

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position, detectionRadius);
	}
}
