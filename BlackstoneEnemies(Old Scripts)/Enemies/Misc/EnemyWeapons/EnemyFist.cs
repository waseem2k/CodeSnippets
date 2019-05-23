using UnityEngine;

public class EnemyFist : MonoBehaviour
{
	public float fistForce = 100f; // The force of the fist being thrown
	public float maxDistance = 1.5f; // The max distance the fist can travel before returning
	
	private Transform returnPosition; // The position the fist needs to return to
	//private Rigidbody2D rb; // The rigidbody to do stuff to
	private bool isAttacking; // Check if attacking

	// Set by ability machine
	private float damage; // The amount of damage being done
	private float force; // The amount of force applied to target
	private Transform targetPosition; // The position to throw the fist towards
	private GameObject fistOrigin; // Who threw the fist
	private Animator animator;
	private float attackTime;

	private void Start()
	{
		//rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		returnPosition = transform.parent;
	}

	public void ThrowFist(float _damage, float _force, Transform _target, GameObject _origin)
	{
		// Sets the variables required to do damage
		damage = _damage;
		force = _force;
		targetPosition = _target;
		fistOrigin = _origin;
		attackTime = Time.time;

		isAttacking = true; // Set it to true so OnTriggerEnter can do it's thing
		//animator.SetTrigger("Attack01");
		//var dir = (targetPosition.position - transform.position).normalized; // Gets the direction of the target
		//rb.AddForce(dir * fistForce, ForceMode2D.Impulse); // Adds force to the fist relative to the targets position
	}

	private void Update()
	{
		if (isAttacking && Time.time > attackTime + 1.5f) // Check if attacking
		{
			isAttacking = false;
			//animator.SetTrigger("ReturnToWalk");
		}
	}

	/*// Checks the distance the fist has traveled doesn't exceed the max distance set
	if (Vector2.Distance(transform.position, returnPosition.position) > maxDistance) 
	{
		//rb.velocity = Vector2.zero; // Sets the fists velocity to zero
		isAttacking = false; // Sets is attacking to false so no damage is dealt while the fist is retracting
	}*/
		//}
		/*if (!isAttacking) // If not attacking the fist is going to keep moving back to it's original position
		{
			//rb.MovePosition(returnPosition.position);
		}*/
	//}//TODO: Reset attack state when animation ends

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!isAttacking) return; // Won't do anything unless set to attack

		var tar = other.gameObject.GetComponent<Player>() ?? other.GetComponentInParent<Player>();
		if (tar == null) return; // Checks if the target is the player

		var tarObject = tar.transform;

		//rb.velocity = Vector2.zero; // Sets the velocity to zero if it hits a target
		tarObject.GetComponent<Health>().Damage(damage, fistOrigin); // Deals damage to target
		tarObject.GetComponent<Rigidbody2D>().AddForce((other.transform.position - returnPosition.position).normalized * force, ForceMode2D.Impulse); // Adds knockback force to target

		//animator.SetTrigger("ReturnToWalk");
		isAttacking = false; // Sets is attacking to false to make sure this only runs once
	}
}
