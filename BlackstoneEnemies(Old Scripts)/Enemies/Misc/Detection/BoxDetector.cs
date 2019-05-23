using System.Linq;
using UnityEngine;

public class BoxDetector : DetectorBase
{
	[Header("Box Detector")]
	[SerializeField] private float horizontalMax = 1f, verticalMax = 1f;

	private Vector2 topLeft, topRight, botLeft, botRight;

	public Collider2D[] DetectionBox(LayerMask targetLayer) // The area in which to detect the player, returns null if nothing found
	{
		return Physics2D.OverlapAreaAll(topLeft, botRight, targetLayer);
	}

	internal override void FixedUpdate()
	{
		FindTarget(); // Looks for a target

		CheckSightToTarget();
		DisengageCheck(); // Checks if the target has been out of sight for a set period
		//UpdateTargetInfo(); // Updates target in enemy
		//UpdateSightInfo(); // Updates sight to player in enemy
	}

	internal override void FindTarget() // Set the target
	{
		if (target != null) return;

		var targets = DetectionBox(playerLayer);
		target = (from tar in targets where tar.gameObject.GetComponent<Player>() select tar.transform).FirstOrDefault();
	}

	private void UpdateBounds()
	{
		topLeft = new Vector2(transform.position.x - horizontalMax, transform.position.y + verticalMax);
		topRight = new Vector2(transform.position.x + horizontalMax, transform.position.y + verticalMax);
		botLeft = new Vector2(transform.position.x - horizontalMax, transform.position.y - verticalMax);
		botRight = new Vector2(transform.position.x + horizontalMax, transform.position.y - verticalMax);
	}

	internal override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		if (!showGizmos) return;
		UpdateBounds();
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(topLeft, topRight);
		Gizmos.DrawLine(botLeft, botRight);
		Gizmos.DrawLine(topLeft, botLeft);
		Gizmos.DrawLine(topRight, botRight);
	}
}
