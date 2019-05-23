using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
	public Transform targetPosition;
	private bool hasTriggered;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (hasTriggered) return;

		var tar = other.GetComponent<Player>() ?? other.GetComponentInParent<Player>();

		if (!tar) return;

		tar.transform.position = targetPosition.position;
		tar.enabled = false;
		hasTriggered = true;
	}
}
