using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using VikingCrewTools;
using VikingCrewTools.Sidescroller;

[RequireComponent(typeof(AbilityMachine))]
public class Enemy : CharacterBase
{
	[Header("Dimension")]
	public Dimension activeInDimension;

	[Header("Movement")]
	public float movementSpeed = 5f; // Movement force
	public float movementMaxSpeed;
	public float stoppingFriction;
	
	public float inclineForce = 500f; // The force used to go up slopes
	public float followDistance = 2f;
	public bool rightFacing; // Bool to tell which way the enemy is facing
	protected bool autoRotate = true; // If the enemy should rotate on its own
	internal int movementDirection = -1; // Direction integer to multiply directions
	protected float run;

	[Header("Edge Detection")]
	public float rearEdgeDistance = 2f;
	public float frontEdgeDistance = 2f; // Distance for edge detection

	[Header("Jumping")]
	public float jumpForce = 15f; // THe force used to calculate jumps
	public int maxDoubleJumps; // The number of times the character can jump after it's initial jump. 0 means it will only jump once
	public LayerMask groundLayer;
	public float angleToJump = 55; // Ai will jump if the angle to the next node is greater than this angle
	protected int currentDoubleJumps;
	protected bool isGrounded;
	protected bool doJump;

	[Header("Attacking")]
	public float attackRate; // The rate at which to attack
	public float chargeUpTime; // The time to wait before executing an attack. Eg. Can be used as the draw time for archers
	public float minAttackDistance = 1f; // Min attack distance
	public float maxAttackDistance = 5f; // Nax attack distance
	public LaserSight laserSight;
	internal bool attackRoutineStarted;
	internal bool isAttacking; // Check if an attack is being executed
	protected bool delayedAbilityStarted;

	[Header("Character")]
	public Limb mainBody;
	public Limb feet;
	protected Limb[] limbs;
	public Animator animator; // All animation related
	public SpriteRenderer spriteRenderer; // For onDamage event
	protected AbilityMachine abilityMachine; // For using abilities
	[HideInInspector] public Health health; // Health cache for quick access

	[Header("Audio")] // Audio source and clips
	public new AudioSource audio;
	//public AudioClip jumpSound;
	//public AudioClip landSound;

	[Header("Target")]
	public Transform player; // The player transform to check
	protected float playerDistance; // The current distance to the player

	protected bool initialized;
	protected bool rearHit, frontHit;

	#region Physics
	protected Rigidbody2D rb; // Rigidbody for physics stuff

	public Rigidbody2D RB
	{
		get { return rb; }
	}

	public Vector2 Velocity
	{
		get { return rb.velocity; }
		set { rb.velocity = value; }
	}

	public Vector2 Position
	{
		get { return transform.position; }
		set { transform.position = value; }
	}
	public float Mass
	{
		get { return rb.mass; }
		set { rb.mass = value; }
	}
	public float Drag
	{
		get { return rb.drag; }
		set { rb.drag = value; }
	}


	#endregion

	protected virtual void OnValidate()
	{
		RotateMe(rightFacing);
		if (movementSpeed < 0) movementSpeed = 0;
	}

	public void InitializeCharacter()
	{
		abilityMachine = GetComponent<AbilityMachine>(); // Gets the ability machine
		rb = GetComponent<Rigidbody2D>(); // Gets the rigidbody for movement
		health = GetComponent<Health>(); // Gets the health to be used for checking
		audio = GetComponent<AudioSource>();
		if (feet != null) limbs = feet.transform.parent.GetComponentsInChildren<Limb>();
		if (animator != null) spriteRenderer = animator.gameObject.GetComponent<SpriteRenderer>();
		initialized = true;
	}

	protected void CheckTargetDirection(Vector2 tar) // Check the direction of target
	{
		//if (tar == null) return; // Don't rotate unless there is a target
		if (!autoRotate) return; // Checks if the target needs to rotate on it's own
		if (isAttacking && !delayedAbilityStarted) return; // Don't rotate if executing attack

		if (tar.x > transform.position.x) // Target is to the right
		{
			if (rightFacing) return;
			RotateMe(true);
		}
		else // Target is to the left
		{
			if (!rightFacing) return;
			RotateMe(false);
		}
	}

	protected void ChangeDirection() // Changes the direction we're currently facing
	{
		rightFacing = !rightFacing;
		RotateMe(rightFacing);
	}

	protected void RotateMe(bool faceRight)
	{
		if (faceRight)
		{
			transform.rotation = Quaternion.Euler(0, 0, 0);
			movementDirection = 1;
			rightFacing = true;
		}
		else
		{
			transform.rotation = Quaternion.Euler(0, 180, 0);
			movementDirection = -1;
			rightFacing = false;
		}
	}

	#region Movement
	protected void MoveToPlayer(Transform tar)
	{
		if (tar == null) return;
		if (Mathf.Abs(tar.position.x - transform.position.x) > followDistance)
		{
			rb.AddForce(Vector2.right * movementDirection * movementSpeed, ForceMode2D.Force);
		}

		if (Mathf.Abs(tar.position.x - transform.position.x) < followDistance)
		{
			rb.AddForce(Vector2.right * -movementDirection * movementSpeed * 2, ForceMode2D.Force);
		}
	}

	protected void AddForce(float horizontalForce, ForceMode2D mode = ForceMode2D.Force)
	{
		rb.AddForce(new Vector2(horizontalForce, 0), mode);
	}

	protected void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
	{
		rb.AddForce(force, mode);
	}

	protected void UpdateFriction()
	{
		bool isRunningForward = Velocity.x * movementDirection > 0;
		//Apply friction
		if (Mathf.Abs(Velocity.x) > 0 && !isRunningForward && isGrounded)
		{
			//if velocity is very low then we might add so much friction that the body changes direction, like a little bounce. We don't want that.
			float maxForce = 0.5f * Mass * Velocity.x * Velocity.x / Time.fixedDeltaTime;
			float frictionForce = Mass * 9.82f * stoppingFriction;
			frictionForce = Mathf.Min(frictionForce, maxForce);

			if (Velocity.x > 0) frictionForce *= -1;

			AddForce(frictionForce);
		}
		 
		movementDirection = 0;
	}

	protected void Run(float direction, float speed, float maxSpeed, bool delayedAbility)
	{
		// If character is casting an ability, do nothing
		if (isAttacking) return;

		bool isRunningForward = Velocity.x * direction > 0;

		// If velocity and direction point the same direction then we are trying to increase speed
		if (isRunningForward && Mathf.Abs(Velocity.x) > maxSpeed)
		{
			//If we are at max speed already don't increase speed any more
			direction = 0;
		}

		// Get distance to target
		float dist = GetDistanceToEnemy();

		// If character is casting an ability, we do nothing
		if (delayedAbility) return;
		
		// If within attacking range then we stop
		if (dist < maxAttackDistance && dist > minAttackDistance) direction = 0f; 
		// If too close, we move backwards
		if (dist < minAttackDistance) direction *= -1f;

		if (Math.Abs(direction) < 0.1f) return;

		//If we're on an incline add some force upwards
		if (IsIncline(direction > 0))
		{
			Vector2 normal = GetInclineNormal(direction > 0);
			float angle = Vector2.Angle(Vector2.up, normal);
			if (angle > 10 && angle < 60)
			{
				Vector2 force = Vector2.up * inclineForce;
				AddForce(force);
			}
		}

		// Failsafe incase direction is set too high
		if (Mathf.Abs(direction) > 1)
			direction /= Mathf.Abs(direction);
		
		// Add force to the rigidbody
		AddForce(direction * speed);
	}

	protected void CheckIfNearEdge()
	{
		var rearRay = new Vector2(transform.position.x - rearEdgeDistance * movementDirection, transform.position.y);
		var frontRay = new Vector2(transform.position.x + frontEdgeDistance * movementDirection, transform.position.y);

		rearHit = Physics2D.Raycast(rearRay, Vector2.down, 5, groundLayer);
		frontHit = Physics2D.Raycast(frontRay, Vector2.down, 5, groundLayer);
	}


	#endregion

	#region Jumping
	public void Jump()
	{
		if (isGrounded || currentDoubleJumps < maxDoubleJumps)
		{
			//This represents a velocity change up to the velocity of jumpVelocity
			AddForce(Vector2.up * (jumpForce - Velocity.y) * Mass, ForceMode2D.Impulse);
			//if(jumpSound != null) audio.PlayOneShot(jumpSound);
			animator.SetTrigger("Jump");
			animator.SetBool("Grounded", false);
			currentDoubleJumps++;
		}
	}

	protected void UpdateGrounded()
	{

		if (CheckForGround(true))
		{
			if (!isGrounded)
			{
				//Just landed so need to set collider sizes
				foreach (var bodypart in limbs)
				{
					bodypart.SetOriginalSizeAndPos();
				}
				//if(landSound != null) audio.PlayOneShot(landSound);
			}
			isGrounded = true;
			currentDoubleJumps = 0;
		}
		else
		{

			if (isGrounded)
			{//Just took off so need to set collider sizes
				foreach (var bodypart in limbs)
				{
					bodypart.SetJumpSizeAndPos();
				}
			}
			isGrounded = false;
		}
	}

	protected bool CheckForGround(bool debug)
	{
		Vector2 cStart = (Vector2)feet.transform.position + 0.5f * Vector2.up;
		Vector2 cDirection = Vector2.down;
		float cRadius = feet.GetWidth() / 2;
		float cDistance = 0.5f + (transform.position - feet.transform.position).magnitude + feet.GetWidth() / 2f;
		
		// Using circle cast because we don't want the characters jumping on tiny inconsistencies in colliders
		RaycastHit2D hit = Physics2D.CircleCast(cStart, cRadius, cDirection, cDistance, groundLayer);
		var didHit = hit.collider != null;

		// Visual for the colliders
		if (debug)
		{
			DebugDrawPhysics.DebugDrawCircleCast(cStart, cRadius, cDirection, cDistance,
				didHit ? Color.red : Color.green);
		}
		return didHit;
	}
	#endregion

	#region Animation
	protected void UpdateAnimationController()
	{
		// bodyAnimator.SetBool("OnGround", isGrounded);
		// bodyAnimator.SetFloat("Forward", (isFacingRight ? velocity.x : -velocity.x));
		animator.SetFloat("WalkSpeed", Mathf.Abs(Velocity.x));
		animator.SetBool("Grounded", isGrounded);
		animator.SetFloat("JumpVelocity", Velocity.y);
	}
	#endregion

	#region Tools
	protected bool IsIncline(bool rightSide) // Checks to see if character is trying to walk up a slope
	{
		return feet.FeelInFront(rightSide, groundLayer);
	}

	protected Vector2 GetInclineNormal(bool rightSide) // Returns the normal direction of world collider
	{
		Vector2 normal;
		feet.FeelInFront(rightSide, groundLayer, out normal);
		return normal;
	}
	protected void SetBodypartLayers(int newLayer) // Sets the limbs to a separate layer
	{
		foreach (var b in limbs)
		{
			b.gameObject.layer = newLayer;
		}
	}

	protected void BodypartToggleTrigger(bool isTrigger)
	{
		feet.GetComponent<Collider2D>().isTrigger = isTrigger;
		/*foreach (var b in limbs)
		{
			b.GetComponent<Collider2D>().isTrigger = isTrigger;
		}*/
	}
	protected bool DetectHolesInGround() // returns true if there is no ground detected under the AI
	{
		if (!isGrounded) return false;

		Ray2D ray = new Ray2D(Position, Vector2.down);
		RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 3, groundLayer);
		//if (!hit) Debug.Log("Ground Found");
		return hit.collider == null;

	}

	protected float GetDistanceToEnemy()
	{
		return player != null ? (player.position - transform.position).magnitude : float.PositiveInfinity;
	}

	protected Vector2 GetDirectionToEnemy()
	{
		return (player.position - transform.position).normalized;
	}

	protected float GetDistanceToTarget(Transform _tar) // Gets the current distance to the target
	{
		return Vector2.Distance(transform.position, _tar.position);
	}
	#endregion

	protected IEnumerator DeathRoutine(float waitTime)
	{
		animator.SetTrigger("Killed");
		yield return new WaitForSeconds(waitTime);
		Destroy(gameObject);
	}

	protected void OnDamage(GameObject obj)
	{
		Debug.Log("I am hit");
		spriteRenderer.color = Color.red;
		spriteRenderer.DOColor(Color.white, 0.2f).SetEase(Ease.InQuint);

		if (health.CurrentHealth < 0) return;
		if (isAttacking) return;
		
		animator.SetTrigger("Damaged");
	}

	#region Check_Line_Of_Sight
	// Checks if line of sight is clear to a target.Returns true only if ai can see the target withiin maxdist
	public bool CheckLineOfSight(Vector3 rayPos, Vector3 pos) // Check if the line of sight is clear towards a position
	{
		
		return CheckLineOfSight(rayPos, pos, null);
	}

	public bool CheckLineOfSight(Vector3 rayPos, Transform tar, float maxDist = float.PositiveInfinity)
	{
		return tar != null && CheckLineOfSight(rayPos, tar.position, tar, maxDist);
	}

	public bool CheckLineOfSight(Vector3 rayPos, Vector3 pos, Transform tar, float maxDist = float.PositiveInfinity)
	{
		Ray2D ray = new Ray2D(rayPos, pos - rayPos);
		int oldLayer = feet.gameObject.layer;

		//Temporarily ignore own body when raycasting
		SetBodypartLayers(Physics2D.IgnoreRaycastLayer);
		RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxDist);
		SetBodypartLayers(oldLayer);

		//Looking for target
		if (tar != null)
		{
			//Saw target
			//Note that the collider transform is not the same as the taarget, but the rigidbody transform is
			return hit.rigidbody != null && hit.rigidbody.transform == tar;
		}
		//if we are not looking for a target then we need to see nothing in order to have free line of sight
		//If we saw nothing then there is free line of sight to position
		return hit.collider == null;
	}

	public bool CheckSightPlayer(Vector3 rayPos, Vector3 pos, LayerMask mask, float maxDist = float.PositiveInfinity)
	{
		Ray2D ray = new Ray2D(rayPos, pos - rayPos);

		RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxDist, mask);
		bool playerInFront;
		if (rightFacing)
		{
			playerInFront = pos.x > rayPos.x;
		}
		else
		{
			playerInFront = pos.x < rayPos.x;
		}

		return hit.transform.root.GetComponent<Player>() && playerInFront;
	}

	public bool CheckSightTarget(Vector2 origin, Vector2 target, LayerMask layer)
	{
		if (Physics2D.Linecast(origin, target, layer))
			return false;
		return true;
	}
	#endregion Check_Line_Of_Sight
}
