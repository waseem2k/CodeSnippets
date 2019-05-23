using System.Collections;
using UnityEngine;

public class TemporalBeast : Boss
{

	[Header("Temporal Beast Mechanics")]
	public float floatingSpeed;
	[Range(1,10)] public float verticalDistance; // The vertical distance to limit the beasts movement
	[Range(1, 10)] public float horizontalDistance; // Horzontal movement limit
	public AudioClip castingSound;

	public LayerMask groundMask;

	private bool leftHit, rightHit, upHit, downHit; // Hits detected

	public bool debug;

	private Transform teleportTarget;

	public AfterImageGenerator afterImage;
	private float castDuration;
	private bool castingAbility;

	protected override void OnEnable()
	{
		base.OnEnable();
		OnPhaseChange += ResetForPhase;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		OnPhaseChange -= ResetForPhase;
	}
	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!attackRoutineStarted) return;

		HitDetection();
		BeastMovement();
	}

	private void BeastMovement()
	{
		if (castingAbility || currentPhase == 1)
		{
			rb.velocity = Vector2.zero;
			return;
		}
		ObstacleAvoidance();
		MoveToPlayer(player);
	}

	private void ObstacleAvoidance()
	{

		if (downHit)
		{
			rb.AddForce(Vector2.up * floatingSpeed, ForceMode2D.Force);
		}
		else
		{
			rb.AddForce(Vector2.down * floatingSpeed, ForceMode2D.Force);
		}
		if (leftHit)
		{
			rb.AddForce(Vector2.right * floatingSpeed, ForceMode2D.Force);
		}
		if (rightHit)
		{
			rb.AddForce(Vector2.left * floatingSpeed, ForceMode2D.Force);
		}
		if (upHit)
		{
			rb.AddForce(Vector2.down * floatingSpeed, ForceMode2D.Force);
		}
	}

	private void HitDetection() // Checks for walls and obstacles for avoidance
	{
		leftHit = CastRay(Vector2.left, horizontalDistance);
		rightHit = CastRay(Vector2.right, horizontalDistance);
		upHit = CastRay(Vector2.up, verticalDistance);
		downHit = CastRay(Vector2.down, verticalDistance);
	}

	private bool CastRay(Vector2 dir, float distance) // casts a ray with given direction and distance
	{
		return Physics2D.Raycast(transform.position, dir, distance, groundMask);
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		var dir = other.transform.position - transform.position;
		dir.Normalize();
		rb.AddForce(dir*floatingSpeed*10);
	}

	public override void SetPositions(Transform[] tars)
	{
		foreach (var tar in tars)
		{
			if (!tar.GetComponent<JumpTarget>().master) continue;
			teleportTarget = tar;
			return;
		}
	}

	private void OnDrawGizmos()
	{
		if (debug)
		{
			Gizmos.color = leftHit ? Color.red : Color.green;
			Gizmos.DrawRay(transform.position, Vector2.left * horizontalDistance); // Left ray

			Gizmos.color = rightHit ? Color.red : Color.green;
			Gizmos.DrawRay(transform.position, Vector2.right * horizontalDistance); // Right ray

			Gizmos.color = upHit ? Color.red : Color.green;
			Gizmos.DrawRay(transform.position, Vector2.up * verticalDistance); // Up ray

			Gizmos.color = downHit ? Color.red : Color.green;
			Gizmos.DrawRay(transform.position, Vector2.down * verticalDistance); // Down ray
			//Gizmos.color = Color.magenta;

		}
	}

	internal override void StartAttackRoutine() // Starts the attack routine for the boss
	{
		StartCoroutine(AttackSequence());
	}

	private IEnumerator AttackSequence()
	{
		attackRoutineStarted = true;
		while (health.CurrentHealth > 0)
		{
			CheckAbility();
			yield return new WaitUntil(() => castingAbility == false);
			//CastAbility(currentAbility);
			yield return new WaitForSeconds(attackRate);
		}
	}

	private IEnumerator BeginCasting(Ability ability)
	{
		animator.SetTrigger("CastAbility");
		animator.SetBool("Casting", true);
		yield return new WaitForSeconds(castDuration);
		castingAbility = false;
		if(castingSound != null) audio.PlayOneShot(castingSound);
		CastAbility(ability);
	}

	private void CheckAbility()
	{
		if (phases.Length < 1) return;

		foreach (var ability in phases[currentPhase].phaseAbilities)
		{
			if (!(Time.time > ability.abilityTimeUsed + ability.abilityBaseCooldown)) continue;

			if (ability.abilityType == Ability.AbilityType.motion)
			{
				ability.SetTargetPos(teleportTarget);
				abilityMachine.SetTarget(player);
			}
			ability.BeginCasting();
			castingAbility = true;
			//currentAbility = ability;
			castDuration = ability.castTime;
			StartCoroutine(BeginCasting(ability));
			return;
		}
	}

	private void CastAbility(Ability ability)
	{
		ability.QueRandomSound();
		ability.SetParams(gameObject);
		ability.SetTargetPos(player);
		ability.abilityTimeUsed = Time.time;
		ability.TriggerAbility();
		animator.SetBool("Casting", false);
	}

	private void ResetForPhase(int phase)
	{
		castingAbility = false;
	}

	#region BossIntro
	public override void BossIntro()
	{
		StartCoroutine(IntroSequence());
	}

	private IEnumerator IntroSequence()
	{
		// do stuff

		bossDialogue.InitiateDialogue();
		DialogueManager.instance.OnDialogueEnded += ToggleDiscussion;
		yield return new WaitUntil(() => discussionOver);
		DialogueManager.instance.OnDialogueEnded -= ToggleDiscussion;

		EndCurrentPhase();
		StartAttackRoutine();
		SetBossToUI(0);

		afterImage.SpawnTrail(true);
	}
	#endregion
}
