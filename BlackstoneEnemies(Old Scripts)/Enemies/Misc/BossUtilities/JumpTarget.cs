using UnityEngine;

public class JumpTarget : MonoBehaviour
{
	public bool master;
	public bool teleportTarget;

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(transform.position, "JumpTarget.psd");
	}
}
