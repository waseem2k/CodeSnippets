using System.Collections;
using DG.Tweening;
using UnityEngine;

public class AbilityMachine : MonoBehaviour
{
	public EnemyWeapon enemyWeapon; // The attached weapon to apply effects to

	public Transform projectileSpawnPoint; // The position where projectiles fire from
	
	public LayerMask playerLayer;
	public LayerMask environmentLayer;
	public Animator animator;
	public bool ignoreAnimationCalls;

	[HideInInspector] public bool holdAbility;

	[HideInInspector] public bool machineReady; // Lets everyone know that the machine is ready to be used again

	[HideInInspector] public float initialForce; // Force the object is propelled at
	[HideInInspector] public float contactForce; // Force applied when arriving at destination
	[HideInInspector] public float damage; // Damage the target received
	[HideInInspector] public float camShakeForce; // The force used to shake the camera

	// Projectile specific
	public enum ProjectileType { Single, RapidFire, Scatter, Potion, Spray}

	[HideInInspector] public ProjectileType projectileType;
	[HideInInspector] public GameObject projectile; // The projectile to propel
	[HideInInspector] public int projectileCount = 1; // the number of projectiles to spawn
	[HideInInspector] public int projectileDistanceApart = 2; // How far apart the projectiles need to land

	[HideInInspector] public bool useHighAngle; // Weather to use a high angle or low
	[HideInInspector] public bool useTime; // whether to use time instead of speed
	[HideInInspector] public float projectileTravelTime = 2f; // The time it takes to reach the target
	[HideInInspector] public float bioArc = 30f;

	// Motion Abilities
	[HideInInspector] public float nodeDistance = 1f;
	[HideInInspector] public float movementForce;
	[HideInInspector] public float motionDistance; // Max distance for some motion abilities
	[HideInInspector] public int motionCount; // The number of times a motion ability is to repeat itself

	// Delayed abilities
	[HideInInspector] public float teleportDelay = 1f; // Time to wait before executing first ability
	[HideInInspector] public float abilityDelay = 1f; // Time to wait before executing second ability
	[HideInInspector] public float abilityDuration = 1f; // The duration an ability will channel or cast
	[HideInInspector] public float aftermathDelay = 1f; // The duration to wait before finishing ability

	// Area Effects
	[HideInInspector] public float abilityRadius = 5f; // The radius used for special affects
	[HideInInspector] public GameObject particlePrefab; // A particle system that needs to be instantiated

	// Audio
	private AudioSource audioSource; // Audio source for playing sounds

	private AudioClip queuedAudioClip;
	private AudioClip soundOne;
	private AudioClip soundTwo;

	private GameObject[] projectileList;

	//[HideInInspector] public GameObject particleSystemTwo; // A second particle system, used for delayed effects

	private int firstPoint; // First point of contact for projectile, projectiles are split based on this
	private Rigidbody2D rb;  // My rigidbody for movement based abilities
	
	private Collider2D colliderTarget; // The collider to modify for certain abilities
	private Transform target; // Position to propel objects towards, generally the player
	private Transform targetPosition; // The target position for boss to move towards
	private Vector3 lastPosition; // The last set position the boss needs to go back to after executing an attack
	private bool positionReached; // When a target jump position is reached;
	private bool channeling; // For channeled abilities
	private bool chargeComplete;

	private delegate void AbilityCast(); // Used to pass abilities to a method

	private void OnEnable()
	{
		rb = GetComponent<Rigidbody2D>();
		colliderTarget = GetComponent<Collider2D>();
		audioSource = GetComponent<AudioSource>();
	}

	private void FixedUpdate()
	{
		if (positionReached == false && targetPosition != null)
		{
			positionReached.CheckIfPositionReached(transform.position, targetPosition.position);
		}
	}

	public void PlayAnimation(string animationToPlay)
	{
		if (ignoreAnimationCalls) return;
		if(animator != null) animator.SetTrigger(animationToPlay);
	}

	#region SetTargets

	public void SetTarget(Transform tar) // Sets the target for attacking
	{
		target = tar;
	}

	public void SetTargetPosition(Transform targetPos) // Sets the position for motion based abilities
	{
		targetPosition = targetPos;
	}

	#endregion

	#region DelayedAbilityCasters
	// Cast a motion ability then wait until the target position is reached before executing the next ability
	private IEnumerator DelayedMotionAbility(AbilityCast ability, AbilityCast abilityTwo)
	{
		positionReached = false;
		ability();
		yield return new WaitUntil(() => positionReached);
		abilityTwo();
	}

	// Casts two abilities with a delay between each
	private IEnumerator DelayedAbility(AbilityCast abilityA, AbilityCast abilityB, float delayOne, float delayTwo)
	{
		abilityA();
		yield return new WaitForSeconds(delayOne);
		abilityB();
		yield return new WaitForSeconds(delayTwo);
	}

	// Casts three abilities with a delay between each
	private IEnumerator DelayedAbility(AbilityCast abilityA, AbilityCast abilityB, AbilityCast abilityC, float delayOne, float delayTwo, float delayThree)
	{
		abilityA();
		yield return new WaitForSeconds(delayOne);
		abilityB();
		yield return new WaitForSeconds(delayTwo);
		abilityC();
		yield return new WaitForSeconds(delayThree);
	}
	#endregion

	#region VortexAbilities

	public void PlanarVortex() // Teleport to a set position, start imploding (Pull enemy towards you), end with an explosion that deals damage to anything close by
	{
		//StartCoroutine(DelayedAbility(TeleportToTarget, VortexImplosion, VortexExplosion, 0.5f, abilityDuration, aftermathDelay));
		VortexExplosion();
	}

	public void VortexImplosion(float pullForce) // Starts the implosion effect to pull targets near
	{
		channeling = true;
		StartCoroutine(SuckTargets(pullForce));
	}

	private IEnumerator SuckTargets(float pullForce)
	{
		Rigidbody2D tarBody = target.GetComponent<Rigidbody2D>();
		transform.position = targetPosition.position;
		var go = Instantiate(particlePrefab, transform.position, transform.rotation);
		go.GetComponent<ParticleSystem>().Play();
		
		yield return new WaitForSeconds(1.5f);
		Destroy(go, abilityDuration);
		CameraManager.instance.ShakeCamera(abilityDuration, camShakeForce);
		while (channeling)
		{
			Vector2 dir = target.position - transform.position;
			dir.y = 0; // Set to zero so the player doesn't float

			tarBody.AddForce(-dir * pullForce, ForceMode2D.Force);

			yield return new WaitForFixedUpdate();
		}
	}

	public void VortexExplosion() // Creates the aftermath particle effect, initiates the explosion effect
	{
		var go = Instantiate(particlePrefab, transform.position, transform.rotation);
		go.GetComponent<ParticleSystem>().Play();
		float _time = go.GetComponent<ParticleSystem>().main.duration;
		Destroy(go, _time);
		channeling = false;
		Knockback();
	}

	#endregion

	#region TeleportAbilities

	public void TeleportAttack() // Teleport to target, attack, teleport back to last position
	{
		StartCoroutine(DelayedAbility(TeleportToTarget, Knockback, TeleportBack, abilityDelay, teleportDelay, 0.1f));
	}

	public void TeleportToTarget() // Used to teleport to a target position
	{
		lastPosition = transform.position;
		transform.position = targetPosition.position;
	}

	public void TeleportBack() // Used to teleport back to the last position that was set elsewhere
	{
		transform.position = lastPosition;
	}

	#endregion

	#region AreaEffects

/*	// An explosion that deals damage to targets and knocks them away
	public void Explode()
	{
		var col = Physics2D.OverlapCircleAll(transform.position, abilityRadius);
		foreach (var c in col)
		{
			if(c.gameObject == gameObject) continue;
			if (c.GetComponent<Health>())
				c.GetComponent<Health>().Damage(damage, gameObject);

			if (c.GetComponent<Rigidbody2D>())
				c.GetComponent<Rigidbody2D>().AddForce((c.transform.position - transform.position).normalized * contactForce, ForceMode2D.Impulse); // Adds knockback force to target
				//TODO: Currently only knocking upwards, needs to knock away also, may have been fixed
		}
	}*/

	public void Knockback()
	{
		PlayeQueuedSound(); // Play sound
		var col = Physics2D.OverlapCircleAll(transform.position, abilityRadius, playerLayer);
		foreach (var c in col)
		{
			var tar = c.transform.root.GetComponentInChildren<Player>();
			if (tar)
			{
				var hp = tar.GetComponent<Health>();
				var tarBody = tar.GetComponent<Rigidbody2D>();

				if (hp && tarBody)
				{
					hp.Damage(damage, gameObject);

					var dir = tar.transform.position - transform.position;
					tarBody.AddForce(dir * contactForce, ForceMode2D.Impulse);
/*					if (tarBody.transform.position.y > transform.position.y && Mathf.Abs(tarBody.transform.position.y - transform.position.y) > 5f)
					{
						tarBody.AddForce((tarBody.transform.position - transform.position).normalized * -contactForce,
							ForceMode2D.Impulse); // Adds knockback force to target in reverse
					}
					else
					{
						tarBody.AddForce((tarBody.transform.position - transform.position).normalized * contactForce,
							ForceMode2D.Impulse); // Adds knockback force to target
					}*/
				}
				break;
			}
		}
	}

	#endregion

	#region Projectiles

	// Launch a projectile towards the target, Calculates arc and can spread the projectiles out if firing more than 1
	public void LaunchProjectile()
	{
		//Check if a target exists
		if (!target) return;

		firstPoint = -projectileCount / 2 * 2;
		for (var i = 0; i < projectileCount; i++)
		{
			// Instantiate a copy of the projectile, set its location and position
			var clonedBullet = Instantiate(projectile, projectileSpawnPoint.position, transform.rotation);

			// Set the damage amount to our projectile
			clonedBullet.GetComponent<Projectile>().SetParams(damage, gameObject);

			var position = new Vector2(target.position.x + firstPoint, target.position.y);
			firstPoint += projectileDistanceApart;

			var cloneRB = clonedBullet.GetComponent<Rigidbody2D>();

			// Calculate trajectory based on time or speed
			PlayeQueuedSound();
			cloneRB.velocity = useTime ? ExtensionMethods.CalculateTrajectoryWithTime(projectileSpawnPoint.position, position, projectileTravelTime) : ExtensionMethods.CalculateTrajectoryWithSpeed(projectileSpawnPoint.position, position, initialForce, useHighAngle);
		}
	}

	public void PotionThrow()
	{
		StartCoroutine(PotionThrowRoutine());
	}

	private IEnumerator PotionThrowRoutine()
	{
		SpawnPotion();
		yield return new WaitForSeconds(abilityDelay);
		SpawnPotion();
	}

	private void SpawnPotion()
	{
		var pot = Instantiate(projectile, projectileSpawnPoint.position, transform.rotation);
		pot.GetComponent<Projectile>().SetParams(damage, gameObject);
		var potRb = pot.GetComponent<Rigidbody2D>();
		potRb.velocity = ExtensionMethods.CalculateTrajectoryWithSpeed(projectileSpawnPoint.position, target.position, initialForce, false);
		PlayeQueuedSound();
	}

	public void BioAttack()
	{
		StartCoroutine(FireBioProjectile());
	}

	public IEnumerator FireBioProjectile()
	{
		yield return new WaitUntil(() => animator.GetBool("AbilityHold"));

		var endTime = Time.time + abilityDuration;
		var number = 0;
		var secondNumber = 1;
		

		while (Time.time < endTime)
		{
			var clonedBullet = Instantiate(projectile, projectileSpawnPoint.position, transform.rotation);
			clonedBullet.GetComponent<Projectile>().SetParams(damage, gameObject);

			var position = new Vector2(target.position.x, target.position.y + number);

			clonedBullet.GetComponent<Rigidbody2D>().velocity = ExtensionMethods.CalculateTrajectoryWithSpeed(projectileSpawnPoint.position, position, initialForce, false);
			number += secondNumber;
			if (number > 3 || number < 0)
			{
				secondNumber = -secondNumber;
			}
			PlayeQueuedSound(); // Play sound
			yield return new WaitForSeconds(0.1f);
		}
		animator.SetBool("AbilityHold", false);
	}

	public void SpawnProjectilesCircle()
	{
		StartCoroutine(TimedProjectileSpawn());
	}

	private IEnumerator TimedProjectileSpawn()
	{
		projectileList = new GameObject[projectileCount];

		float time = abilityDuration * 0.5f / projectileCount;

		for (var i = 0; i < projectileCount; i++)
		{
			var angle = i * (Mathf.PI * 2 / projectileCount);
			//var ang = Random.value * 360;
			Vector2 pos = new Vector2
			{
				x = transform.position.x + 2 * Mathf.Cos(angle),
				y = transform.position.y + 2 * Mathf.Sin(angle)
			}; //new Vector2(Mathf.Cos(transform.position.x + angle), Mathf.Sin(transform.position.y + angle)) * 5);) * 5;

			projectileList[i] = Instantiate(projectile);
			projectileList[i].GetComponent<Projectile>().SetParams(damage, gameObject);
			projectileList[i].transform.position = pos;
			projectileList[i].transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

			yield return new WaitForSeconds(time);
		}
	}

	public void LaunchCircleProjectiles()
	{
		StartCoroutine(CircularProjectileLaunch());
	}

	private IEnumerator CircularProjectileLaunch()
	{
		Vector3 dir = target.position - transform.position;

		foreach (var p in projectileList)
		{
			p.GetComponent<ClockHandProjectile>().AutoRotate(false);
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
			p.transform.rotation = q;//Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 5);
			//p.transform.Rotate(new Vector3(0, 0, dir.z));
		}
		yield return new WaitForSeconds(1f);

		foreach (var p in projectileList)
		{
			Rigidbody2D body = p.GetComponent<Rigidbody2D>();
			body.constraints = RigidbodyConstraints2D.None;
			body.AddForce(dir * initialForce, ForceMode2D.Force);
		}
		PlayeQueuedSound();
	}
	#endregion

	#region SwordAttacks
	public void BasicSwing()
	{
		if (!target) return;
		if (!enemyWeapon) return;
		enemyWeapon.DoAttack(damage, contactForce, gameObject);
		PlayeQueuedSound();
		//enemyWeapon.QueueSoundClip(audioSource, queuedAudioClip);
		//PlayeQueuedSound();
	}

	public void GroundSlam()
	{
		if (!target) return;
		if (!enemyWeapon) return;
		enemyWeapon.DoAttack(damage, contactForce, gameObject);
		PlayeQueuedSound();
		//enemyWeapon.QueueSoundClip(audioSource, queuedAudioClip);
		//PlayeQueuedSound();
	}
	#endregion

	#region Melee Abilities
	public void Punch()
	{
		enemyWeapon.DoAttack(damage, contactForce, gameObject);
		//PlayeQueuedSound();
		enemyWeapon.QueueSoundClip(audioSource, queuedAudioClip);
		//PlayeQueuedSound();
	}
	#endregion

	#region Motion

	public void DrillCharge()
	{
		StartCoroutine(DrillChargeRoutine());
	}

	private IEnumerator DrillChargeRoutine()
	{
		yield return new WaitUntil(() => animator.GetBool("AbilityHold"));
		
		for (int i = 0; i < motionCount; i++)
		{
			int dir = 1;
			if (targetPosition.position.x < transform.position.x) dir = -1;

			enemyWeapon.DoAttack(damage, contactForce, gameObject);
			enemyWeapon.doDamage = true;
			rb.AddForce(Vector2.right * dir * initialForce, ForceMode2D.Impulse);
			enemyWeapon.QueueSoundClip(audioSource, queuedAudioClip);
			float time = 1f;
			if (i == motionCount - 1) time = 0.5f;
			yield return new WaitForSeconds(time);
			rb.velocity = Vector2.zero;
			Debug.Log("Charged");
		}
		animator.SetBool("AbilityHold", false);
		enemyWeapon.doDamage = false;
	}

	public void MightyJump()
	{
		//colliderTarget.isTrigger = true;
		rb.velocity = ExtensionMethods.CalculateTrajectoryWithSpeed(transform.position, targetPosition.position, initialForce, true);
		StartCoroutine(ResetTrigger(0.2f, true, nodeDistance));
	}

	public void ChargeTarget()
	{
		var heightDiff = targetPosition.position.y - transform.position.y;
		var tarPos = new Vector2(targetPosition.position.x, targetPosition.position.y + 3);
		rb.velocity = heightDiff > 2 ? ExtensionMethods.CalculateTrajectoryWithTime(transform.position, tarPos, 0.5f) : ExtensionMethods.CalculateTrajectoryWithSpeed(transform.position, tarPos, initialForce, false);
		StartCoroutine(ResetTrigger(0.2f, false));
		StartCoroutine(ChargingTarget(tarPos));
	}

	public void JumpAttack(Ability.RangeRequirement rangeRequired)
	{
		//rb.velocity = ExtensionMethods.CalculateTrajectoryWithSpeed(transform.position, targetPosition.position, initialForce, false);

		if (rangeRequired == Ability.RangeRequirement.Ranged)
		{
			StartCoroutine(JumpAndAttack());
		}
		else
		{
			rb.AddForce(Vector2.up * (initialForce - rb.velocity.y) * rb.mass, ForceMode2D.Impulse);
			enemyWeapon.DoAttack(damage, contactForce, gameObject);
			enemyWeapon.QueueSoundClip(audioSource, queuedAudioClip);
		}

		//StartCoroutine(DiveAtTarget());
	}

	private IEnumerator JumpAndAttack()
	{
		rb.velocity = ExtensionMethods.CalculateTrajectoryWithSpeed(transform.position, targetPosition.position, initialForce, false);
		var startTime = Time.time;

		while (true)
		{
			float dist = Vector2.Distance(transform.position, targetPosition.position);

			if (dist < 2f)
			{
				rb.velocity = Vector2.zero;
				Knockback();
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private Vector2 CheckCeiling(Vector2 pos) // Checks for a ceiling and reduces the y axis if a ceiling is detected
	{
		while (true)
		{
			RaycastHit2D rayHit = Physics2D.Raycast(pos, Vector2.up, 1f, environmentLayer);

			if (!rayHit) return pos;

			pos.y -= 1f; // Lowers the position by 1 if ceiling is detected
		}
	}

	public void DiveAttack()
	{
		StartCoroutine(DiveAtTarget());
	}

	public void LeapToTarget()
	{
		ChargeTarget(); //TODO: Must change to a separate ability that leaps the max amount requested
	}

	public void LeapAwayFromTarget()
	{
		//StartCoroutine(ResetTrigger(0.2f, false));
		StartCoroutine(LeapAway());
	}

	private IEnumerator LeapAway()
	{
		var startTime = Time.time;
		//colliderTarget.isTrigger = true;
		var tarDir = ExtensionMethods.GetTargetNegativeDirection(target, transform);
		var hit = Physics2D.Raycast(transform.position, Vector2.right * tarDir, motionDistance + 2, environmentLayer);

		//var leapPosition = hit ? new Vector2(hit.point.x + 2 * tarDir, hit.point.y) : new Vector2(transform.position.x + motionDistance * tarDir, transform.position.y);
		var leapPosition = new Vector2(transform.position.x + motionDistance * tarDir, transform.position.y);
		if (hit) // if no room behind, leap forward instead
		{
			//LeapToTarget();
			leapPosition = new Vector2(transform.position.x - motionDistance * tarDir, transform.position.y);
			//yield break;
		}
		rb.velocity = ExtensionMethods.CalculateTrajectoryWithSpeed(transform.position, leapPosition, initialForce, false);

		while (true)
		{
			var tarDist = Vector2.Distance(transform.position, leapPosition);
			if (Time.time > startTime + 2f)
			{
				//colliderTarget.isTrigger = false;
				yield break;
			}
			if (tarDist < 4f)
			{
				rb.velocity = Vector2.zero;
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator DiveAtTarget()
	{
		lastPosition = transform.position;
		var tarPos = targetPosition.position;
		var returning = false;
		//rb.velocity = ExtensionMethods.CalculateTrajectoryWithTime(transform.position, targetPosition.position, projectileTravelTime);
		rb.DOMove(targetPosition.position, projectileTravelTime);
		var startTime = Time.time;

		while (true)
		{
			var dist = Vector2.Distance(transform.position, tarPos);
			if (!returning)
			{
				if (dist < 1.5f)
				{
					rb.velocity = Vector2.zero;
					Knockback();
					yield return new WaitForSeconds(aftermathDelay);
					//rb.velocity = ExtensionMethods.CalculateTrajectoryWithTime(transform.position, lastPosition, projectileTravelTime * 0.5f);
					rb.DOMove(lastPosition, projectileTravelTime * 0.5f);
					tarPos = lastPosition;
					returning = true;
				}
			}
			else
			{
				if (dist < 4f)
				{
					rb.velocity = Vector2.zero;
					yield break;
				}
			}
			if (Time.time > startTime + 2f)
			{
				rb.velocity = Vector2.zero;
				yield break;
			}
			
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator ChargingTarget(Vector3 tarPos)
	{
		var startTime = Time.time;
		colliderTarget.isTrigger = true;
		while (true)
		{
			var tarDist = Vector2.Distance(transform.position, tarPos);
			if (Time.time > startTime + 2f)
			{
				colliderTarget.isTrigger = false;
				yield break;
			}
			if (tarDist < 2f)
			{
				Knockback();
				rb.velocity = Vector2.zero;
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator ResetTrigger(float t, bool distCheck, float dist = 1f)
	{
		colliderTarget.isTrigger = true;
		yield return new WaitForSeconds(t);

		while (true)
		{
			if (distCheck)
			{
				if (rb.velocity.y < 0)
				{
					if (Vector2.Distance(transform.position, targetPosition.position) < dist)
					{
						colliderTarget.isTrigger = false;
						yield break;
					}
				}
			}
			else
			{
				colliderTarget.isTrigger = false;

				yield break;
			}
			
			yield return new WaitForEndOfFrame();
		}
		
	}

	#endregion

/*	private void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
			StopAllCoroutines();
	}*/

	public void SpawnGrowingProjectile()
	{
		StartCoroutine(GrowingProjectile());
	}

	public IEnumerator GrowingProjectile()
	{
		Vector2 maxScale = projectile.transform.localScale;
		GameObject go = Instantiate(projectile,projectileSpawnPoint.position, projectileSpawnPoint.rotation);

		Rigidbody2D body = go.GetComponent<Rigidbody2D>(); 
		if(body) Destroy(body);

		Projectile proj = go.GetComponent<Projectile>();
		if(proj) Destroy(proj);

		ParticleProjectile particleProj = go.GetComponent<ParticleProjectile>();
		if (particleProj) Destroy(particleProj);

		Destroy(go, abilityDuration);
		go.transform.localScale = Vector2.zero;
		Vector2 scale = go.transform.localScale;
		
		while (scale.x < maxScale.x)
		{
			if (!go) yield break;

			scale = new Vector2(scale.x + Time.fixedDeltaTime, scale.y + Time.fixedDeltaTime);
			go.transform.localScale = scale;
			yield return new WaitForFixedUpdate();
		}
	}

	#region Audio

	public void PlayAudioClip(AudioClip clip, bool loop = false)
	{
		if (clip == null) return;
		audioSource.loop = loop;
		audioSource.PlayOneShot(clip);
	}

	public void QueueSound(AudioClip Clip, bool loop = false)
	{
		queuedAudioClip = Clip;
		audioSource.loop = loop;
	}

	private void PlayeQueuedSound()
	{
		if (queuedAudioClip == null) return;
		audioSource.PlayOneShot(queuedAudioClip);
	}

	#endregion

	public void StopCurrentOrders()
	{
		rb.velocity = Vector2.zero;
		StopAllCoroutines();
		
	}
}