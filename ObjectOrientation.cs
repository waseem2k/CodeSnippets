using UnityEngine;

/// <summary>
/// Used for orienting an object between two pivot points
/// Useful for carrying objects by a handle and placing them on the ground in another position
/// </summary>
public class ObjectOrientation : MonoBehaviour
{
	public Transform objectToMove; // The object being moved
	public Transform handlePivot; // The handle position
	public Transform groundPivot; // The grounded position

	public void MoveToHandle()
	{
		SetPosition(handlePivot);
	}

	public void MoveToGround()
	{
		SetPosition(groundPivot);
	}

	private void SetPosition(Transform parent)
	{
		objectToMove.SetParent(parent);
		objectToMove.localPosition = Vector3.zero;
		objectToMove.localRotation = Quaternion.Euler(0, 0, 0);
	}
}
