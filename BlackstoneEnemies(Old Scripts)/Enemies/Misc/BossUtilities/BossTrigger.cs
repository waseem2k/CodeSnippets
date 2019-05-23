using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossTrigger : MonoBehaviour
{
	public Vector2 areaSize; // Boss spawn area size;
	[Space]
	public Texture2D backgroundTexture; // The background texture for gizmo
	[Space]
	[HideInInspector] public BoxCollider2D areaCollider; // The collider to manipulate

	private void Awake()
	{
		if (areaCollider == null) areaCollider = GetComponent<BoxCollider2D>();
	}

	#region Set-Up
	private void OnDrawGizmos()
	{
		if (backgroundTexture == null) return;

		transform.position = new Vector3(transform.position.x.RoundToNearest(1), transform.position.y.RoundToNearest(1), 0);

		var gizmoSize = new Vector2(areaSize.x.RoundToNearest(2), areaSize.y.RoundToNearest(2));
		var texturePos = new Vector2(transform.position.x - gizmoSize.x / 2, transform.position.y - gizmoSize.y / 2);

		Gizmos.DrawGUITexture(new Rect(texturePos, gizmoSize), backgroundTexture);
	}

	private void OnValidate() // Updates the boss area based on the given dimensions 
	{
		if (areaCollider == null) areaCollider = GetComponent<BoxCollider2D>();
		areaCollider.size = new Vector2(areaSize.x.RoundToNearest(2), areaSize.y.RoundToNearest(2));
	}
	#endregion
}
