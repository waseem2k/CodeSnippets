using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseArtorion : Boss
{
	[Header("Chase Artorion")]
	public float jumpTime;

	public float attackDistance;

	[HideInInspector] public List<Transform> jumpTargets;

	[Header("Teleporting Artorion")]
	public float minDistanceBeforeTeleport;
	public GameObject implosionPrefab;
	public GameObject explosionPrefab;

	[HideInInspector] public List<Transform> teleportTargets;
	private bool teleportingPhase;
	private bool doTeleport;

	private bool doAttack;
	private bool isJumping;
	private Transform currentMovementTarget;
	private int jumpIndex;
	private bool positionReached = true;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!attackRoutineStarted) return;

		if (isJumping)
		{
			if (Vector2.Distance(transform.position, currentMovementTarget.position) < 2f)
			{
				Velocity = Vector2.zero;
				isJumping = false;
			}
		}

		var dist = Vector2.Distance(player.position, transform.position);
		if (dist < attackDistance)
		{
			if (!doAttack) doAttack = true;
		}

		if (currentPhase == 0)
		{
			Run(movementDirection, movementSpeed, movementMaxSpeed, false);
			CheckIfNearEdge();
			bool forward = player.position.x > transform.position.x;
			
			if (!frontHit && !isJumping && forward)
			{
				if(positionReached) JumpToPosition();
			}
			if (!forward && !frontHit)
			{
				Velocity = Vector2.zero;
			}

			if (isJumping)
			{
				if (Vector2.Distance(transform.position, currentMovementTarget.position) < 2f)
				{
					Velocity = Vector2.zero;
					isJumping = false;
				}
			}
		}

		if (currentPhase == 1)
		{
			if(!teleportingPhase) StartCoroutine(TeleportRoutine());
			if (dist > minDistanceBeforeTeleport)
			{
				if(!doTeleport) doTeleport = true;
			}
		}

		if (!positionReached)
		{
			positionReached = Vector2.Distance(transform.position, currentMovementTarget.position) < 0.2f;
		}

		if (Velocity.y < 0.1f) // if the distance to the target is less than amount and currently falling down
		{
			BodypartToggleTrigger(false);
		}
	}

	#region Movement
	private void MoveTowardsPlayer()
	{
		var dist = Vector2.Distance(player.position, transform.position);

		if (dist > maxAttackDistance)
		{
			AddForce(Vector2.right * movementDirection);
		}
	}
	#endregion

	#region Jumping
	private void JumpToPosition()
	{
		BodypartToggleTrigger(true);

		currentMovementTarget = jumpTargets[jumpIndex];
		jumpIndex++;
		if (jumpIndex == jumpTargets.Count)
		{
			EndCurrentPhase();
			Velocity = Vector2.zero;
			jumpIndex = 0;
		}
		Velocity = ExtensionMethods.CalculateTrajectoryWithTime(transform.position, currentMovementTarget.position, jumpTime);
		isJumping = true;
		Invoke("ResetPosition", 5f);
	}

	private void ResetPosition()
	{
		if (isJumping)
		{
			transform.position = currentMovementTarget.position;
			Destroy(Instantiate(explosionPrefab, transform.position, transform.rotation), 1f);
		}
	}
	#endregion

	#region Teleporting

	private IEnumerator TeleportRoutine()
	{
		teleportingPhase = true;
		while (teleportingPhase)
		{
			if (doTeleport)
			{
				Destroy(Instantiate(implosionPrefab, transform.position, transform.rotation), 1f);
				transform.position = Vector3.zero;
				yield return new WaitForSeconds(0.2f);
				TeleportToPosition();
				yield return new WaitForSeconds(2f);
				doTeleport = false;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private void TeleportToPosition()
	{
		if (jumpIndex == teleportTargets.Count) return;
		transform.position = teleportTargets[jumpIndex].position;
		jumpIndex++;
		Destroy(Instantiate(explosionPrefab, transform.position, transform.rotation), 1f);
	}

	#endregion

	#region Attacking
	internal override void StartAttackRoutine()
	{
		StartCoroutine(AttackRoutine());
	}

	private IEnumerator AttackRoutine()
	{
		attackRoutineStarted = true;
		while (health.CurrentHealth > 0)
		{
			if (doAttack)
			{
				TriggerAbility();
				yield return new WaitForSeconds(2f);
				doAttack = false;
			}
			yield return new WaitForEndOfFrame();
		}
		attackRoutineStarted = false;
	}

	internal override void TriggerAbility()
	{
		foreach (var ability in phases[currentPhase].phaseAbilities)
		{
			if (Time.time < ability.abilityTimeUsed + ability.abilityBaseCooldown) continue;
			ability.QueRandomSound();
			ability.SetParams(gameObject);
			ability.SetTargetPos(player);
			ability.abilityTimeUsed = Time.time;
			ability.TriggerAbility();
			return;
		}
	}
	#endregion

	#region Intro
	public override void BossIntro()
	{
		StartCoroutine(IntroSequence());
	}

	private IEnumerator IntroSequence()
	{
		bossDialogue.InitiateDialogue();
		DialogueManager.instance.OnDialogueEnded += ToggleDiscussion;
		yield return new WaitUntil(() => discussionOver);
		DialogueManager.instance.OnDialogueEnded -= ToggleDiscussion;

		EndCurrentPhase();
		StartAttackRoutine();
		//if(healthUI != null) healthUI.AssignTarget(gameObject, Name);
	}

	public override void SetPositions(Transform[] tars)
	{
		foreach (var t in tars)
		{
			if (t.GetComponent<JumpTarget>().teleportTarget)
			{
				teleportTargets.Add(t);
			}
			else
			{
				jumpTargets.Add(t);
			}
		}
	}
	#endregion
}
