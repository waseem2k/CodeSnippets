using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeastScientist : Boss
{
	[Header("Beast Scientist Mechanics")]
	public float jumpTime = 2f; // Time it takes to reach destination

	public bool finalFight;

	//public LayerMask groundLayer;

	[Header("Initialization")]
	public List<Transform> jumpTargets;

	public Transform currentJumpTarget;

	public bool inMeleeRange;
	
	//public bool needsToJump;
	public bool isJumping;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!attackRoutineStarted) return;
		
		CheckDistance();
		CheckIfNearEdge();
		CheckForJumpReq();

		if (!inMeleeRange) BeastMovement();// Melee phase
										   //else rb.velocity = Vector2.zero;

		if (isJumping)
		{
			if (Vector2.Distance(transform.position, currentJumpTarget.position) < 2f)
			{
				Velocity = Vector2.zero;
				isJumping = false;
			}
		}
		if (Velocity.y < 0.1f) // if the distance to the target is less than amount and currently falling down
		{
			BodypartToggleTrigger(false);
		}
	}

	private void BeastMovement()
	{
		if (Mathf.Abs(player.position.x - transform.position.x) > followDistance && frontHit)
		{
			rb.AddForce(Vector2.right * movementDirection * movementSpeed, ForceMode2D.Force);
		}

		if (Mathf.Abs(player.position.x - transform.position.x) < followDistance && rearHit)
		{
			rb.AddForce(Vector2.right * -movementDirection * movementSpeed, ForceMode2D.Force);
		}
	}

	private void CheckDistance()
	{
		float targetDistance = Vector2.Distance(Position, player.position);
		inMeleeRange = targetDistance < minAttackDistance;
	}

	private void CheckForJumpReq()
	{
		if (inMeleeRange || frontHit) return;
		if (!isJumping) JumpToPosition();
	}

	#region AttackRoutine
	internal override void StartAttackRoutine() // Starts the attack routine for the boss
	{
		StartCoroutine(AttackSequence());
	}

	private IEnumerator AttackSequence()
	{
		attackRoutineStarted = true;
		while (health.CurrentHealth > 0)
		{
			TriggerAbility();
			yield return new WaitForSeconds(attackRate);
		}
	}

	internal override void TriggerAbility()
	{
		if (phases.Length < 1) return;

		foreach (var ability in phases[currentPhase].phaseAbilities)
		{
			if (!(Time.time > ability.abilityTimeUsed + ability.abilityBaseCooldown)) continue;
			//abilitySource.clip = ability.aSound;
			//abilitySource.Play();
			if (ability.rangeRequired == Ability.RangeRequirement.Melee && !inMeleeRange) continue;

			ability.QueRandomSound();
			ability.SetParams(gameObject);
			ability.SetTargetPos(player);
			ability.abilityTimeUsed = Time.time;
			ability.TriggerAbility();
			return;
		}
	}
	#endregion

	#region BossIntro
	public override void BossIntro()
	{
		StartCoroutine(IntroSequence());
	}

	private IEnumerator IntroSequence()
	{
		yield return new WaitForSeconds(2f);

		bossDialogue.InitiateDialogue();
		DialogueManager.instance.OnDialogueEnded += ToggleDiscussion;
		yield return new WaitUntil(() => discussionOver);
		DialogueManager.instance.OnDialogueEnded -= ToggleDiscussion;

		EndCurrentPhase();
		StartAttackRoutine();
		if (finalFight)
		{
			SetBossToUI(2, 1, true);
		}
		else
		{
			SetBossToUI(1, 1, true);
		}
		
	}

	public override void ForceStartFight()
	{
		EndCurrentPhase();
		StartAttackRoutine();
	}
	#endregion

	#region Jumping
	private void JumpToPosition()
	{
		BodypartToggleTrigger(true);
		currentJumpTarget = FindSuitableJumpTarget();
		Velocity = ExtensionMethods.CalculateTrajectoryWithTime(transform.position, currentJumpTarget.position, jumpTime);
		isJumping = true;
		Invoke("ResetPosition", 5f);
	}

	private void ResetPosition()
	{
		if (isJumping)
		{
			transform.position = currentJumpTarget.position;
		}
	}

	private Transform FindSuitableJumpTarget()
	{
		float minDist = Mathf.Infinity;
		Transform jumpTar = null;
		foreach (var j in jumpTargets)
		{
			float dist = Vector2.Distance(j.position, player.position);
			if (dist > minDist) continue;
			
			minDist = dist;
			jumpTar = j;
		}
		return jumpTar;
	}
	#endregion


	private void OnDrawGizmos()
	{
		var leftRay = new Vector2(transform.position.x - rearEdgeDistance * movementDirection, transform.position.y);
		var rightRay = new Vector2(transform.position.x + frontEdgeDistance * movementDirection, transform.position.y);

		Gizmos.color = Color.magenta;
		Gizmos.DrawRay(leftRay, Vector2.down*5);
		Gizmos.DrawRay(rightRay, Vector2.down * 5);
	}

	public override void SetPositions(Transform[] tars)
	{
		jumpTargets = tars.ToList();
	}
}
