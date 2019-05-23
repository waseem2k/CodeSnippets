using UnityEngine;

/// <summary>
/// This class sort of encapsulates the collision functionality for a bodypart.
/// It will be able to set the bodypart's size when doing some operations like jumpin and crouching
/// It also tries to move any 2D/3D logic away from the CharacterController2D-class as we want that class to be
/// agnostic regarding whether we use the 2D or 3D physics engine
/// </summary>
public class Limb : MonoBehaviour
{
	private new Collider2D collider;

	[Tooltip("This determines how much smaller the hitbox of the character ill be when jumping")]
	public float jumpSizeRatio = 0.5f;

	[Tooltip("This determines how much the hitbox of the character should move when jumping")]
	public float jumpColliderOffset = 0.375f;

	private Vector2 originalLocalPos;
	private Vector2 originalLocalScale;

	public void Awake()
	{
		collider = GetComponent<Collider2D>();
		originalLocalPos = transform.localPosition;
		originalLocalScale = transform.localScale;
	}

	public void SetJumpSizeAndPos()
	{
		SetCollider(jumpSizeRatio, jumpColliderOffset);
	}

	public void SetOriginalSizeAndPos()
	{
		transform.localScale = originalLocalScale;
		transform.localPosition = originalLocalPos;
	}

	/// <summary>
	/// Set this bodypart to ignore collisions with another bodypart. 
	/// This is useful if you want players on the same team to not collide with each other despite 
	/// being in the same layer
	/// </summary>
	/// <param name="bodypart"></param>
	public void SetToIgnoreCollision(Limb bodypart)
	{
		Physics2D.IgnoreCollision(collider, bodypart.collider);
	}

	public void SetToIgnoreCollision(GameObject other)
	{
		Physics2D.IgnoreCollision(collider, other.GetComponent<Collider2D>());
	}

	/// <summary>
	/// Feels straight down from the front of the collider if there is contact with the ground.
	/// If there is then the body is likely on an upwards incline
	/// </summary>
	/// <param name="rightSide"></param>
	/// <param name="groundableMaterials"></param>
	/// <returns></returns>
	public bool FeelInFront(bool rightSide, LayerMask groundableMaterials)
	{
		Vector2 tmp = Vector2.zero;
		return FeelInFront(rightSide, groundableMaterials, out tmp);
	}

	/// <summary>
	/// Feels straight down from the front of the collider if there is contact with the ground.
	/// If there is then the body is likely on an upwards incline
	/// </summary>
	/// <param name="rightSide"></param>
	/// <param name="groundableMaterials"></param>
	/// <param name="normal">normal of hit position</param>
	/// <returns></returns>
	public bool FeelInFront(bool rightSide, LayerMask groundableMaterials, out Vector2 normal)
	{
		float distance = 0.45f * GetHeight();
		normal = Vector2.zero;
		Vector2 samplePos = (Vector2) transform.position + new Vector2((rightSide ? 0.6f : -0.6f) * GetWidth(), 0);
		bool isHit;

		RaycastHit2D hit = Physics2D.Raycast(samplePos, Vector2.down, distance, groundableMaterials);
		isHit = hit.collider != null;
		if (isHit)
		{
			Debug.DrawLine(samplePos, samplePos + distance * Vector2.down, Color.red);
			normal = hit.normal;
		}
		else
			Debug.DrawLine(samplePos, samplePos + distance * Vector2.down, Color.green);

		return isHit;
	}

	protected void SetCollider(float newScale, float offset = 0)
	{
		Vector3 localScale = transform.localScale;
		Vector3 localPosition = transform.localPosition;

		localScale.y = newScale;
		localPosition.y = originalLocalPos.y + offset;

		transform.localScale = localScale;
		transform.localPosition = localPosition;

	}

	public float GetWidth()
	{
		if (collider is CircleCollider2D)
			return ((CircleCollider2D) collider).radius * 2;

		if (collider is BoxCollider2D)
			return ((BoxCollider2D) collider).size.x;

		throw new System.NotImplementedException();
	}

	public float GetHeight()
	{
		if (collider is CircleCollider2D)
			return ((CircleCollider2D) collider).radius * 2;

		if (collider is BoxCollider2D)
			return ((BoxCollider2D) collider).size.y;

		throw new System.NotImplementedException();
	}

	public void DisableCollider()
	{
		collider.enabled = false;
	}

	public void EnableCollider()
	{
		collider.enabled = true;
	}
}
