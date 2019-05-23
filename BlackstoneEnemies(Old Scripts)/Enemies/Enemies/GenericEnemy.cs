using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using VikingCrewTools;

public class GenericEnemy : Enemy
{
	public enum EnemyState { Idle, Chase, Attack, ReturnToOrigin, Dead }
	public enum EnemyType { Melee, Ranged, Flying }
	public enum IdleType { Idling, Patrolling }

	[Header("Enemy Info")]
	public EnemyType enemyType; // The type pf enemy
	public IdleType idleType; // The type of enemy
	[SerializeField] internal EnemyState enemyState; // Enemy state

	[Header("Origin")]
	public bool guardPosition; // Set to true if target must guard a position rather than give chase
	public float maxDistance = 5f; // The distance to disengage player. If guarding position then this will be the max distance from origin. Else it is the max distance from the player
	public float teleportTime = 5f; // Time to attempt running back before teleporting
	internal Vector2 pointOfOrigin; // Position the enemy started
	private bool wasFacingRight; // The direction enemy was facing at the start

	[Header("Patrolling")]
	public float patrolDistance = 1f; // Distance to patrol from origin
	public float patrolSpeed = 1f; // The speed at which to patrol
	public float patrolMaxSpeed = 2f;
	
	[Header("Brain")]
	public float checksPerSecond = 5;
	//public float enableEnemyDistance; // Enemy won't do anything unless the player is within this range
	public float engagePlayerDistance; // The distance required for the enemy to engage player
	public LayerMask sightLayer; // The layer to check for line of sight


	//protected bool enemyEnabled; // Allows the enemy to perform tasks when the player is close enough
	protected bool playerClose; // Enabled when player is close enough to the enemy
	protected bool playerInSight; // True when the player can be seen by the enemy
	protected bool playerEngaged; // True when the AI has engaged the player in combat

	#region Detectors
	[Header("Detectors")]
	public Transform eyes; // The position the character will use to cast rays from
	[Space]
	[SerializeField]private Vector2 upperFeelerSize = new Vector2(0.1f, 0.5f);
	[SerializeField] private Vector2 upperFeelerPos = new Vector2(0.1f, 0.125f);
	protected bool isDetectingHighObstacle;
	[Space]
	[SerializeField] private Vector2 lowerFeelerSize = new Vector2(0.1f, 0.5f);
	[SerializeField] private Vector2 lowerFeelerPos = new Vector2(0.1f, 0.5f);
	protected bool isDetectingLowObstacle;

	public LayerMask feelerDetectMask;
	#endregion Detectors

	[Header("Abilities")]
	public Ability[] abilities;


	[Header("Pathfinding")]
	//public float targetEnemyDistance = 5; // The distance to the player	the ai will try to maintain
	public float maxDistanceForEnemyToMoveBeforeRequestingNewPath = 5; // The max distance the player must move before requesting a new path
	public float minGetCloserPerSecond = 0.1f; // The minimum distance we need to close towards our target before considering the path a failure
	public PathfindingAgent pathfinding;

	[Header("Death")]
	public float destroyTime = 5f;

	[Header("Debugging")]
	public bool showOrigin;
	public bool showPatrol;
	public bool showDetectors;
	public bool showSensors;
	public bool debugPathfinding;
	public bool verboseDebug;

	#region Generic

	private void Update()
	{
		PlayerCheck();
		if (Input.GetKeyDown(KeyCode.J))
		{
			TriggerAbility();
		}
	}

	//TODO: Need to check player sight

	private void PlayerCheck() // Checks for the players distance and determines how this AI behaves
	{
		playerDistance = GetDistanceToEnemy(); // Gets the current distance to the player
		if (player == null || playerEngaged) return; // If the AI is enaged in combat or if the player is missing for some reason, this does nothing
		
		//enemyEnabled = playerDistance < enableEnemyDistance; // Checks if the player is within a certain distance to determine if this AI should be doing anything at all
		playerClose = playerDistance < engagePlayerDistance; // Checks if the player is close to prepare for sight check
		playerInSight = playerClose && CheckSightPlayer(eyes.position, player.position, sightLayer); // Performs a sight check if the player is close
	}

	private void FixedUpdate()
	{
		//if (!enemyEnabled) return;
		if (!health.isAlive) return;

		Idle(); // Idle or Patrol based on state
		CheckState(); // Checks the state 
		UpdateAnimationController();
		DetectObstacles(); //TODO: move this to Think when we do not need to debug it


		if (playerEngaged && player != null)
		{
			if (enemyState != EnemyState.ReturnToOrigin)
			{
				CheckTargetDirection(player.position);
				UpdateGrounded();
				UpdateFriction();
			}
		}

		if (doJump)
		{
			Jump();
			doJump = false;
		}
		Run(run, movementSpeed, movementMaxSpeed, delayedAbilityStarted);
	}

	private void Awake()
	{
		pointOfOrigin = transform.position;
	}

	public virtual void Start()
	{
#if !UNITY_EDITOR
		showOrigin = false;
		showPatrol = false;
		showDetectors = false;
		showSensors = false;
		debugPathfinding = false;
		verboseDebug = false;
#endif
		InitializeAbilities();
		InitializeCharacter();
		InitializeAi();
	}

	protected virtual void OnEnable()
	{
		if (health == null) health = GetComponent<Health>();
		health.OnDeath += OnDeathCallback;
		health.OnDamaged += OnDamage;
		health.OnDamaged += EngagePlayer;
		StartCoroutine(BehaviourRoutine());
	}

	private void EngagePlayer(GameObject obj)
	{
		if (!playerEngaged) playerEngaged = true;
		if (!attackRoutineStarted) StartCoroutine(AttackSequence());
		if (!playerClose) playerClose = true;
		if (!playerInSight) playerInSight = true;
		EnterAIState(EnemyState.Attack);
	}

	protected virtual void OnDisable()
	{
		health.OnDeath -= OnDeathCallback;
		health.OnDamaged -= OnDamage;
		health.OnDamaged -= EngagePlayer;
		ResetEnemy(false);
	}

	protected void InitializeAi()
	{
		wasFacingRight = rightFacing; // Sets the starting direction of player
		enemyState = EnemyState.Idle;
		pathfinding.Setup(this);
		player = Player.instance.transform;
	}

	private void ResetEnemy(bool starRoutines)
	{
		if (!initialized) InitializeCharacter();
		StopAllCoroutines();
		transform.position = pointOfOrigin;		
		EnterAIState(EnemyState.Idle);
		playerEngaged = false;
		playerInSight = false;
		attackRoutineStarted = false;
		run = 0;
		Velocity = Vector2.zero;
		rightFacing = wasFacingRight;
		RotateMe(rightFacing);
		pathfinding.SetPathUnvalid();
		if(starRoutines) StartCoroutine(BehaviourRoutine());
	}
	#endregion

	#region Gizmos
	private void OnDrawGizmos()
	{
		if (showPatrol)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay(new Vector2(transform.position.x + patrolDistance, transform.position.y), Vector2.down * 2);
			Gizmos.DrawRay(new Vector2(transform.position.x - patrolDistance, transform.position.y), Vector2.down * 2);

			Gizmos.color = Color.magenta;
			Gizmos.DrawRay(feet.transform.position, Vector2.right * movementDirection * (patrolDistance * 0.5f));
		}

		if (showOrigin)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, maxDistance);
		}

		if (showSensors)
		{

			Gizmos.color = isDetectingLowObstacle ? Color.red : Color.cyan;
			Gizmos.DrawWireCube(Position + lowerFeelerPos, lowerFeelerSize);


			Gizmos.color = isDetectingHighObstacle ? Color.red : Color.cyan;
			Gizmos.DrawWireCube(Position + upperFeelerPos, upperFeelerSize);
		}
	}
	#endregion

	#region Behaviour_States

	protected void CheckState()
	{
		if (!health.isAlive) return;

		if (playerClose) // Engages player if the player is close and in sight
		{
			if (enemyType != EnemyType.Flying && playerInSight || enemyType == EnemyType.Flying)
			{
				if (!playerEngaged && enemyState != EnemyState.ReturnToOrigin)
				{
					playerEngaged = true;
					if(!attackRoutineStarted) StartCoroutine(AttackSequence());
				}
			}
		}

		if (playerEngaged) // Checks if max distance has been achieved and disengages the player
		{
			if (guardPosition && GetDistanceToOrigin() > maxDistance) // If guarding position then checks the distance between AI and Origin
			{
				playerEngaged = false;
				enemyState = EnemyState.ReturnToOrigin;
				//StartCoroutine(TeleportToOrigin());
			}
			if (!guardPosition && GetDistanceToEnemy() > maxDistance) // If not guarding position then checks the distance
			{
				playerEngaged = false;
				enemyState = EnemyState.ReturnToOrigin;
				//StartCoroutine(TeleportToOrigin());
			}
		}

		if (playerEngaged) // Enters attack state if player is engaged and not already in attack state
		{
			if (enemyState == EnemyState.Idle || enemyState == EnemyState.ReturnToOrigin)
			{
				enemyState = EnemyState.Attack; // Enter attack state
			}
		}
	}

	private IEnumerator BehaviourRoutine()
	{
		WaitForSeconds wait = new WaitForSeconds(1f / checksPerSecond);

		while (true)
		{
			doJump = false;

			switch (enemyState)
			{
				case EnemyState.Idle:
					IdleState();
					break;
				case EnemyState.Chase:
					ChaseState();
					break;
				case EnemyState.Attack:
					AttackState();
					break;
				case EnemyState.ReturnToOrigin:
					ReturnToOriginState();
					break;
			}

			yield return wait;
		}
	}

	private void IdleState()
	{
		switch (idleType)
		{
			case IdleType.Idling:
				IdleCheck();
				break;
			case IdleType.Patrolling:
				PatrolCheck();
				break;
		}
	}

	private void ChaseState() // Chase target until target becomes null
	{
		if (playerEngaged)
			MoveTowardsEnemy();
		else
		{
			EnterAIState(EnemyState.ReturnToOrigin);
			//StartCoroutine(TeleportToOrigin());
		}
	}

	private void AttackState()
	{
		if(playerEngaged)
			MoveTowardsEnemy();
		else
		{
			EnterAIState(EnemyState.ReturnToOrigin);
			//StartCoroutine(TeleportToOrigin());
		}
	}

	private void ReturnToOriginState()
	{
		MoveTowardsOrigin(); // Moves the Ai along a path towards it's origin point
		// If distance to origin is less than 5, return to idle state
/*		if (GetDistanceToOrigin() < 2)
		{
			ResetEnemy();
		}*/
		
	}

	public void EnterAIState(EnemyState newState)
	{
		enemyState = newState;
	}
	#endregion Behaviour_States


	#region Return_To_Origin
	private float GetDistanceToOrigin()
	{
		return (pointOfOrigin - Position).magnitude;
	}

	private Vector2 GetDirectionToOrigin()
	{
		return (pointOfOrigin - Position).normalized;
	}

	private bool IsPathToOriginStillValid()
	{
		float targetMovedDistance = Vector3.Distance(pointOfOrigin, pathfinding.targetPosWhenRequesting);
		return targetMovedDistance < 5f;
	}

	private void MoveTowardsOrigin()
	{
		if (pathfinding.HasPath())
		{//If we have no path then request one
			//First, if we are too close (and can see the target) then we need to create some distance!
			if (GetDistanceToOrigin() < 4 && CheckLineOfSight(eyes.position, pointOfOrigin))
			{
				run = GetDirectionToOrigin().x < 0 ? -1 : 1;
				return;
			}

			if (IsPathToOriginStillValid())
			{
				if (FollowPath())
				{
					if (verboseDebug)
						Debug.Log(name + ": " + enemyState + ": I reached the end of my path so requested a new one");
				}
			}
			else
			{
				if (verboseDebug)
					Debug.Log(name + ": " + enemyState + ": my path was no longer valid so I requested a new one");
				pathfinding.SetPathUnvalid();
			}
		}
		else
		{//We have a path
			//First, if we are too close then we need to create some distance!

			if (!pathfinding.hasRequestedPath)
			{
				pathfinding.RequestPath(pointOfOrigin);
				//Debug.Log("Requested Path");
			}
			if (pointOfOrigin.x > transform.position.x)
				run = 1;
			else
				run = -1;
		}
	}

	/*private IEnumerator TeleportToOrigin()
	{
		float startTime = Time.time;
		if(pathfinding.IsOnPath()) CheckTargetDirection(pathfinding.GetCurrentWaypoint());

		while (GetDistanceToOrigin() > 2)
		{
			if (enemyState == EnemyState.ReturnToOrigin)
			{
				if (pathfinding.IsOnPath())
				{
					if (Vector2.Distance(transform.position, pathfinding.GetCurrentWaypoint()) < 4)
					{
						CheckTargetDirection(pathfinding.GetCurrentWaypoint());
					}
				}
			}

			if (Time.time > startTime + teleportTime)
			{
				ResetEnemy(true);
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
		ResetEnemy(true);
	}*/
	#endregion Return_To_Origin

	#region Move_Towards_Enemy

	private bool IsPathToEnemyStillValid()
	{
		if (player == null) return false;
		float targetMovedDistance = Vector3.Distance(player.position, pathfinding.targetPosWhenRequesting);
		return targetMovedDistance < 5f;
	}

	private void MoveTowardsEnemy()
	{
		//First, check if we can see the enemy and if we are close enough to hit!
		if (CheckLineOfSight(eyes.position, player) && GetDistanceToEnemy() < followDistance)
		{
			//if we are too close then we need to create some distance!
			if (GetDistanceToEnemy() < followDistance)
			{
				run = (-GetDirectionToEnemy().x) < 0 ? -1 : 1;
			}
		}
		else if (pathfinding.HasPath())
		{//We have a path

			//Check if path is valid
			if (IsPathToEnemyStillValid())
			{
				//Follow the path to the end (Note that we didn't keep running closer if we got too close to the enemy though)
				if (FollowPath())
				{
					if (verboseDebug)
						Debug.Log(name + ": " + enemyState + ": I reached the end of my path so I'll just stay here and shoot for a while");
				}
			}
			else
			{//Path NOT valid so request a new one
				pathfinding.SetPathUnvalid();
			}
		}
		else
		{//If we have no path then request one
			if (!pathfinding.hasRequestedPath)
			{
				pathfinding.RequestPath(player);
			}
			//but we can't wait for that path for too long so start running while we wait
			if (player.position.x > transform.position.x)
				run = 1;
			else
				run = -1;
		}
	}
	#endregion Move_Towards_Enemy

	#region Patrolling_Idling

	private void IdleCheck()
	{
		
	}
	private void PatrolCheck() // Check distance from origin and detect obstacles
	{
		if (rightFacing && Position.x > pointOfOrigin.x + patrolDistance) ChangeDirection(); // Is to the right of right patrol point


		if (!rightFacing && Position.x < pointOfOrigin.x - patrolDistance) ChangeDirection(); // Is to the left of left patrol point

		RaycastHit2D rayHit = Physics2D.Raycast(feet.transform.position, Vector2.right * movementDirection, patrolDistance * 0.5f, groundLayer);

		if (rayHit) ChangeDirection();
	}

	private void Idle() // Movement based on patrol type or idle type
	{
		if (enemyState != EnemyState.Idle) return;
		switch (idleType)
		{
			case IdleType.Idling:
				break;
			case IdleType.Patrolling:
				Run(movementDirection, patrolSpeed, patrolMaxSpeed, false);
				//AddForce(movementDirection * patrolSpeed);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	protected void Patrol(bool _randomPosition)
	{
		if (_randomPosition)
		{
			//If we have a path then follow it
			if (pathfinding.HasPath())
			{
				//Follow the path to the end
				if (FollowPath())
				{
					//Reached the end
					pathfinding.SetPathUnvalid();
					if (verboseDebug)
						Debug.Log(name + ": " + enemyState + ": I reached the end of my path so I'll immediately request a new patrol path to a random position");
				}
			}
			else
			{//If we have no path then request one
				//if (!pathfinding.hasRequestedPath)
					//pathfinding.RequestRandomPath(worldBounds);
				//TODO: Request random path
			}
		}
		else //TODO: CONDENSE
		{
			//rb.velocity = new Vector2(patrolSpeed * movementDirection, velocity.y);
			if (rightFacing)
			{
				if (Position.x > pointOfOrigin.x + patrolDistance) // Is to the right of right patrol point
				{
					//rb.velocity = new Vector2(velocity.x * 0.25f, velocity.y);
					rb.velocity = Vector2.zero;
					rb.AddForce(Vector2.right * -movementDirection * patrolSpeed * 2, ForceMode2D.Impulse);
					ChangeDirection();
					
				}
			}
			else
			{
				if (Position.x < pointOfOrigin.x - patrolDistance)
				{
					//rb.velocity = new Vector2(velocity.x * 0.25f, velocity.y);
					rb.velocity = Vector2.zero;
					rb.AddForce(Vector2.right * -movementDirection * patrolSpeed*2, ForceMode2D.Impulse);
					ChangeDirection();
					//rb.AddForce(Vector2.right * movementDirection * patrolSpeed, ForceMode2D.Impulse);
				}
			}
			rb.AddForce(Vector2.right * patrolSpeed * movementDirection, ForceMode2D.Force);
		}
	}
	#endregion Patrolling_Idling

	#region Abilities
	protected void InitializeAbilities()
	{
		for (var i = 0; i < abilities.Length; i++)
		{
			if (abilities[i] == null)
			{
				Debug.LogError("Ability missing!");
				continue;
			}
			var abilityClone = Instantiate(abilities[i]);
			abilityClone.Initialize(gameObject);
			abilities[i] = abilityClone;
		}
	}

	private IEnumerator AttackSequence()
	{
		attackRoutineStarted = true;
		while (true)
		{
			// To break out of coroutine
			if (!attackRoutineStarted) yield break;
			
			// Get distance to target
			float tarDistance = GetDistanceToEnemy();

			// Check if target is in sight and is within attack range
			if (playerInSight && tarDistance < maxAttackDistance && 
				tarDistance > minAttackDistance || enemyType == EnemyType.Flying)
			{
				// Set attacking to true when initiating an attack
				// if it becomes false during the pre emt time, the attack will cancel
				isAttacking = true;

				// Draws the weapon for ranged enemies
				if (enemyType == EnemyType.Ranged)
					animator.SetTrigger("DrawWeapon"); 

				// Enable laser sight for some enemies
				if (laserSight != null)
					laserSight.SetTarget(player);

				// Waits for aim time or cast time
				yield return new WaitForSeconds(chargeUpTime); 

				// Trigger ability if it hasn't been interrupted and the target is still in sight
				if (isAttacking && !CheckLineOfSight(eyes.position, player.position))
					TriggerAbility();

				// Hides the laser sight if attached
				if (laserSight != null)
					laserSight.HideLine();

				// Set attacking to false
				isAttacking = false; 

				// Wait for a few seconds before we're ready to attack again
				yield return new WaitForSeconds(attackRate); 
			}
			animator.ResetTrigger("DrawWeapon");			
			yield return new WaitForEndOfFrame();
		}
	}


	private void TriggerAbility()
	{
		foreach (var ability in abilities)
		{
			if (!(Time.time > ability.abilityTimeUsed + ability.abilityBaseCooldown)) continue;
			//abilitySource.clip = ability.aSound;
			//abilitySource.Play();
			ability.SetParams(gameObject);
			ability.SetTargetPos(player);
			ability.QueRandomSound();
			if (ability.rangeRequired == Ability.RangeRequirement.Ranged)
			{
				StartCoroutine(DelayedTrigger(ability, true));
				return;
			}
			ability.abilityTimeUsed = Time.time;
			ability.TriggerAbility();
			return;
		}
	}

	private IEnumerator DelayedTrigger(Ability ability, bool rangedAbility)
	{
		delayedAbilityStarted = true;
		if (rangedAbility)
		{
			var tarDist = Vector2.Distance(transform.position, player.position);
			while (tarDist < ability.requiredRange)
			{
				var dir = player.position - transform.position;
				AddForce(movementSpeed * -dir);
				tarDist = Vector2.Distance(transform.position, player.position);
				yield return new WaitForEndOfFrame();
			}
		}
		ability.abilityTimeUsed = Time.time;
		ability.TriggerAbility();
		
		delayedAbilityStarted = false;
	}


	#endregion

	#region Detection
	public void DetectObstacles(float angle = 0)
	{
		//if (run == 0) return;
		Vector2 direction;
		if (run > 0)
		{
			direction = Vector2.right;
			upperFeelerPos.x = Mathf.Abs(upperFeelerPos.x);
			lowerFeelerPos.x = Mathf.Abs(lowerFeelerPos.x);
		}
		else
		{
			direction = Vector2.left;
			upperFeelerPos.x = -Mathf.Abs(upperFeelerPos.x);
			lowerFeelerPos.x = -Mathf.Abs(lowerFeelerPos.x);
		}

		RaycastHit2D hit = Physics2D.BoxCast(Position + upperFeelerPos, upperFeelerSize, angle, direction, 0, feelerDetectMask);
		if (hit.collider != null)
		{
			isDetectingHighObstacle = true;
			//if (showSensors) DebugDrawPhysics.DebugDrawBoxCast(position + upperFeelerPos, upperFeelerSize, angle, direction, 0, Color.red);
		}
		else
		{
			//if (showSensors) DebugDrawPhysics.DebugDrawBoxCast(position + upperFeelerPos, upperFeelerSize, angle, direction, 0, Color.green);
			isDetectingHighObstacle = false;
		}

		hit = Physics2D.BoxCast(Position + lowerFeelerPos, lowerFeelerSize, angle, direction, 0, feelerDetectMask);
		if (hit.collider != null)
		{
			//if (showSensors) DebugDrawPhysics.DebugDrawBoxCast(position + lowerFeelerPos, lowerFeelerSize, angle, direction, 0, Color.red);
			isDetectingLowObstacle = true;
		}
		else
		{
			//if (showSensors) DebugDrawPhysics.DebugDrawBoxCast(position + lowerFeelerPos, lowerFeelerSize, angle, direction, 0, Color.green);
			isDetectingLowObstacle = false;
		}
	}

	
	#endregion

	#region Pathfinding
	private bool FollowPath() // If is on path it returns true. Also checks for collision and direction to move towards
	{
		//return true;
		if (!pathfinding.IsOnPath())
		{
			pathfinding.SetPathUnvalid();
			if (verboseDebug)
				Debug.Log(name + ": " + enemyState + ": I was no longer on my path so requested a new one");
			return false;
		}
		//Find next waypoint that is on the ground. If the last one is in the air then we should jump to it
		pathfinding.FindNextVisibleGroundedWaypoint(groundLayer);

		//Direction to the next waypoint
		Vector3 dist = (pathfinding.GetCurrentWaypoint() - Position);
		Vector3 dir = dist.normalized;

		//If verbose is on then we can draw some helper symbols to see what the character is currently trying to do
		if (showDetectors)
		{
			DebugDrawPhysics.DebugDrawCircle(pathfinding.GetCurrentWaypoint(), 1, Color.yellow);
		}

		//If there's a hole in the ground and the waypoint is not below us then jump
		doJump = DetectHolesInGround() && dir.y >= 0;

		//---Run---
		if (dir.x > 0)
		{
			run = 1;
			//Jump if we need to move up, double jumps will occur when we reach the max of the parabola and thus the upwards velocity is zero
			//The next waypoint is at least one meter up
			if (dist.y > 1 && Vector2.Angle(Vector2.right, dir) > angleToJump && Velocity.y <= 0) //A velocity zero we reached the top of the jump parabola
			{
				//Debug.Log("Jumping | Right");
				doJump = true;
			}

		}
		else
		{
			run = -1;
			//Jump if we need to move up
			if (dist.y > 1 && Vector2.Angle(Vector2.left, dir) > angleToJump && Velocity.y <= 0)
			{
				//Debug.Log("Jumping | Left");
				doJump = true;
			}
		}

		//If we have a low obstacle then we should try jumping
		//Note that we will jump regardless of whether we detect an upper obstacle or not
		//We will only jump when we do not have upwards velocity so that we do not waste any double jumps

		if (isDetectingLowObstacle && Velocity.y <= 0 && !isDetectingHighObstacle)
		{
			//Debug.Log("Jumping | Detecting low angle");
			doJump = true;
		}

		//Check if we are close enough to the next waypoint
		//If we are, proceed to follow the next waypoint
		return pathfinding.SelectNextWaypointIfCloseEnough();
	}
	#endregion

	public virtual void OnDeathCallback()
	{
		StopAllCoroutines();
		EnterAIState(EnemyState.Dead);
		playerEngaged = false;
		pathfinding.SetPathUnvalid();
		Velocity = Vector2.zero;
		if(laserSight != null) laserSight.HideLine();
		StartCoroutine(DeathRoutine(destroyTime));
	}

	public string GetStateDescription() // Used for getting the current state info of the AI, useful for debugging
	{
		return
			"Name: " + name + "\n" +
			"State: " + enemyState + "\n" +
			"Has path: " + (pathfinding.HasPath() ? "Yes" : "No") + "\n" +
			"Current enemy: " + player.name;

	}
}

/*private bool ShootIfEnemyVisible() //TODO: Fix this and add to archers
{
	if (currentEnemy != null &&
		CheckLineOfSight(currentEnemy))
	{
		aimDirection = (currentEnemy.transform.position - transform.position).normalized;
		return true;
	}
	aimDirection = run > 0 ? Vector3.right : Vector3.left;
	return false;
}*/
