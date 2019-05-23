using System.Linq;
using UnityEngine;

/// <summary>
/// Grid check component makes it easier for us to find the position and check for collisions
/// </summary>
public class GridCheck : MonoBehaviour
{
	public Transform[] points; // A list of points that are used to check for collisions
	private Obstacle obstacle; // The obstacle component that is attached to this gameobject

	/// <summary>
	/// Find a safe position for the object to spawn, points are set manually in the prefab
	/// </summary>
	public bool CanPlace()
	{
		Collider2D[] myOwnColliders = GetComponentsInChildren<Collider2D>();

		// Disable own colliders so we don't get false hits
		foreach (Collider2D col in myOwnColliders)
		{
			col.enabled = false;
		}

		// Set each point to the center of the grid box
		foreach (Transform p in points)
		{
			int X = Mathf.FloorToInt(p.position.x);
			int Y = Mathf.FloorToInt(p.position.y);
			p.position = GridManager.Grid.GetCellCenterLocal(new Vector3Int(X, Y, 0));
		}

		// Check each point for a collision
		bool canPlace = points.Select(p => Physics2D.OverlapCircleAll(p.position, 0.5f)).All(cols => cols.All(c => c.gameObject == gameObject)); ;

		// Enable colliders if a safe position has been found, otherwise this gameobject is destroyed by the generator
		if (canPlace)
		{
			foreach (Collider2D col in myOwnColliders) // Disable own colliders
			{
				col.enabled = true;
			}
		}

		return canPlace;
	}
	/// <summary>
	/// Gets position of object on the grid based on the shape of the object
	/// </summary>
	public Vector3 GetPosition(Grid grid = null)
	{
		if (obstacle == null) obstacle = GetComponent<Obstacle>(); // Find Obstacle component

		// If obstacle isn't found, this either isn't an obstalce or hasn't been set up correctly
		// We just check the center of this object
		if (obstacle == null && grid != null)
		{
			Vector3 helper = grid.GetCellCenterLocal(Vector3Int.FloorToInt(transform.position));
			helper.z = 0;
			return helper;
		}

		// If we do find Obstacle, we call the method on the obstacle
		return obstacle != null ? obstacle.GetCurrentPosOnGrid() : Vector3.zero;
	}
}
