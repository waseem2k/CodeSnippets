using UnityEngine;

public class ThirdPersonMotor : MonoBehaviour
{
	public bool useRootMotion;

	[Header("Movement Speed")]
	public float walkSpeed = 2.5f;
	public float runSpeed = 3f;
	public float movementLerpMultiplier = 1;
	protected float movementLerpState;

	[Header("Rotation")]
	public float rotationSpeed = 10f;

	[Header("Jumping")]
	public float jumpForward = 3f; // Forward jump speed
	public float jumpHeight = 4f; // Jump height
	public bool jumpAirControl = true;
	public LayerMask groundLayer = 1 << 0;
	protected const float jumpTimer = 0.3f; // Time before we start falling
	protected float jumpCounter;

	[Header("Grounded")]
	public float groundMinDistance = 0.2f;
	public float groundMaxDistance = 0.5f;
	protected const float extraGravity = -10f;

	[Header("Slope")]
	public float slopeLimit;
	public float groundDistance;
	public RaycastHit groundHit;
	public float stepOffsetEnd = 0.45f;
	public float stepOffsetStart = 0.05f;
	public float stepSmooth = 4f;

	// Movement action bools
	public bool IsMoving { get; protected set; }
	public bool IsGrounded { get; protected set; }
	public bool IsRunning { get; protected set; }
	public bool IsSliding { get; protected set; }
	public bool IsJumping { get; protected set; }
	public bool LockMovement { get; set; } // Locks player movement, for like cutscenes and such

	public Vector2 Input { get; set; } // Input from ThirdPersonInput
	protected Vector3 targetDirection;
	protected Quaternion targetRotation;
	protected Quaternion freeRotation;
	protected bool keepDirection;

	protected Animator animator; // Animator for animations
	protected Rigidbody rb; // Rigidbody for movement physics
	protected PhysicMaterial maxFrictionPhysics, frictionPhysics, slippyPhysics; // Physics materials for the Rigidbody
	protected CapsuleCollider col; // Capsule collider for collisions
	protected float colliderHeight; // storage capsule collider extra information

	protected float speed, direction, verticalVelocity; // general variables to the locomotion
	protected float velocity; // vel to apply to rigidbody


	public void Init() // Set up the motor, called by
	{
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		col = GetComponent<CapsuleCollider>();
		colliderHeight = col.height;

		// slides the character through walls and edges
		frictionPhysics = new PhysicMaterial
		{
			name = "frictionPhysics",
			staticFriction = .25f,
			dynamicFriction = .25f,
			frictionCombine = PhysicMaterialCombine.Multiply
		};

		// prevents the collider from slipping on ramps
		maxFrictionPhysics = new PhysicMaterial
		{
			name = "maxFrictionPhysics",
			staticFriction = 1f,
			dynamicFriction = 1f,
			frictionCombine = PhysicMaterialCombine.Maximum
		};

		// air physics
		slippyPhysics = new PhysicMaterial
		{
			name = "slippyPhysics",
			staticFriction = 0f,
			dynamicFriction = 0f,
			frictionCombine = PhysicMaterialCombine.Minimum
		};
	}

	public void UpdateMotor()
	{
		CheckGround();
		ControlJumpBehaviour();
		ControlLocomotion();
	}

	// Locomotion
	private void ControlLocomotion()
	{
		Movement();   // move forward, backwards, strafe left and right
	}

	private void Movement()
	{
		float _speed = Mathf.Clamp(Input.y, -1f, 1f);
		float _direction = Mathf.Clamp(Input.x, -1f, 1f);
		speed = _speed;
		direction = _direction;
	}

	protected void ControlSpeed(float vel)
	{
		if (useRootMotion)
		{
			Vector3 v = (animator.deltaPosition * (vel > 0 ? vel : 1f)) / Time.deltaTime;
			v.y = rb.velocity.y;
			rb.velocity = Vector3.Lerp(rb.velocity, v, 20f * Time.deltaTime);
		}
		else
		{
			Vector3 v = (transform.TransformDirection(new Vector3(Input.x, 0, Input.y)) * (vel > 0 ? vel : 1f));
			v.y = rb.velocity.y;
			rb.velocity = Vector3.Lerp(rb.velocity, v, 20f * Time.deltaTime);
		}
	}

	// Jumping
	protected void ControlJumpBehaviour()
	{
		if (!IsJumping) return;

		jumpCounter -= Time.deltaTime;
		if (jumpCounter <= 0)
		{
			jumpCounter = 0;
			IsJumping = false;
		}

		Vector3 vel = rb.velocity;
		vel.y = jumpHeight;
		rb.velocity = vel;
	}

	public void AirControl()
	{
		if (IsGrounded) return;

		Vector3 velY = transform.forward * jumpForward * speed;
		velY.y = rb.velocity.y;
		Vector3 velX = transform.right * jumpForward * direction;
		velX.x = rb.velocity.x;
		float forwardJump = movementLerpState > 0.5f ? jumpForward * 2 : jumpForward;

		if (jumpAirControl)
		{

			rb.velocity = new Vector3(velX.x, velY.y, rb.velocity.z);
			Vector3 vel = transform.forward * (speed > 0 ? forwardJump * speed : jumpForward * speed) + transform.right * (jumpForward * direction);
			rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);
		}
		else
		{
			Vector3 vel = transform.forward * (forwardJump);
			rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);
		}
	}

	// Ground Check
	private void CheckGround()
	{
		CheckGroundDistance();

		// change the physics material to very slip when not grounded or maxFriction when is
		if (IsGrounded && Input == Vector2.zero) // When not moving or jumping
			col.material = maxFrictionPhysics;
		else if (IsGrounded && Input != Vector2.zero) // When grounded and moving
			col.material = frictionPhysics;
		else //
			col.material = slippyPhysics;

		float magVel = (float)System.Math.Round(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude, 2);
		magVel = Mathf.Clamp(magVel, 0, 1);

		float groundCheckDistance = groundMinDistance;
		if (magVel > 0.25f) groundCheckDistance = groundMaxDistance;

		// clear the checkground to free the character to attack on air
		bool onStep = StepOffset();

		if (groundDistance <= 0.1f)
		{
			//startJumping = false;
			IsGrounded = true;
			Sliding();
		}
		else
		{
			if (groundDistance >= groundCheckDistance)
			{
				IsGrounded = false;
				// check vertical vel
				verticalVelocity = rb.velocity.y;
				// apply extra gravity when falling
				if (!onStep && !IsJumping)
					rb.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
			}
			else if (!onStep && !IsJumping)
			{
				rb.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
			}
		}
	}

	private void CheckGroundDistance()
	{
		if (col != null)
		{
			// radius of the SphereCast
			float radius = col.radius * 0.9f;
			float dist = 10f;
			// position of the SphereCast origin starting at the base of the capsule
			Vector3 pos = transform.position + Vector3.up * (col.radius);
			// ray for RayCast
			Ray ray1 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
			// ray for SphereCast
			Ray ray2 = new Ray(pos, -Vector3.up);
			// raycast for check the ground distance
			if (Physics.Raycast(ray1, out groundHit, colliderHeight / 2 + 2f, groundLayer))
				dist = transform.position.y - groundHit.point.y;
			// sphere cast around the base of the capsule to check the ground distance
			if (Physics.SphereCast(ray2, radius, out groundHit, col.radius + 2f, groundLayer))
			{
				// check if sphereCast distance is small than the ray cast distance
				if (dist > (groundHit.distance - col.radius * 0.1f))
					dist = (groundHit.distance - col.radius * 0.1f);
			}
			groundDistance = (float)System.Math.Round(dist, 2);
		}
	}

	private float GroundAngle()
	{
		float groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
		return groundAngle;
	}

	// Sliding
	private void Sliding()
	{
		bool onStep = StepOffset();
		float groundAngleTwo = 0f;
		RaycastHit hit;
		Ray ray = new Ray(transform.position, -transform.up);

		if (Physics.Raycast(ray, out hit, colliderHeight * 0.5f, groundLayer))
		{
			groundAngleTwo = Vector3.Angle(Vector3.up, hit.normal);
		}

		if (GroundAngle() > slopeLimit + 1f && GroundAngle() <= 85 &&
			groundAngleTwo > slopeLimit + 1f && groundAngleTwo <= 85 &&
			groundDistance <= 0.05f && !onStep)
		{
			IsSliding = true;
			IsGrounded = false;
			float slideVelocity = (GroundAngle() - slopeLimit) * 2f;
			slideVelocity = Mathf.Clamp(slideVelocity, 0, 10);
			rb.velocity = new Vector3(rb.velocity.x, -slideVelocity, rb.velocity.z);
		}
		else
		{
			IsSliding = false;
			IsGrounded = true;
		}
	}

	private bool StepOffset()
	{
		if (Input.sqrMagnitude < 0.1 || !IsGrounded) return false;

		RaycastHit hit;
		Vector3 moveDirection = Input.magnitude > 0 ? (transform.right * Input.x + transform.forward * Input.y).normalized : transform.forward;
		Ray rayStep = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + moveDirection * ((col).radius + 0.05f)), Vector3.down);

		if (Physics.Raycast(rayStep, out hit, stepOffsetEnd - stepOffsetStart, groundLayer) && !hit.collider.isTrigger)
		{
			if (hit.point.y >= (transform.position.y) && hit.point.y <= (transform.position.y + stepOffsetEnd))
			{
				float _speed = Mathf.Clamp(Input.magnitude, 0, 1);
				Vector3 velocityDirection = hit.point - transform.position;
				rb.velocity = velocityDirection * stepSmooth * (_speed * (velocity > 1 ? velocity : 1));
				return true;
			}
		}
		return false;
	}

	// Cam methods
	public void RotateToTarget(Transform target)
	{
		if (target)
		{
			Quaternion rot = Quaternion.LookRotation(target.position - transform.position);
			Vector3 newPos = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
			targetRotation = Quaternion.Euler(newPos);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newPos), rotationSpeed * Time.deltaTime);
		}
	}

	public void UpdateTargetDirection(Transform referenceTransform = null)
	{
		if (referenceTransform)
		{
			Vector3 forward = keepDirection ? referenceTransform.forward : referenceTransform.TransformDirection(Vector3.forward);
			forward.y = 0;

			forward = keepDirection ? forward : referenceTransform.TransformDirection(Vector3.forward);
			forward.y = 0; //set to 0 because of referenceTransform rotation on the X axis

			//get the right-facing direction of the referenceTransform
			Vector3 right = keepDirection ? referenceTransform.right : referenceTransform.TransformDirection(Vector3.right);

			// determine the direction the player will face based on input and the referenceTransform's right and forward directions
			targetDirection = Input.x * right + Input.y * forward;
		}
		else
			targetDirection = keepDirection ? targetDirection : new Vector3(Input.x, 0, Input.y);
	}

	public void UpdateLerpState()
	{
		movementLerpState = Mathf.Clamp(movementLerpState, 0, 1);
		int lDir = IsRunning && IsMoving ? 1 : -1;
		movementLerpState += movementLerpMultiplier * lDir * Time.deltaTime;
	}
}
