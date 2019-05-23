using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barbarian : Boss
{
	[Header("Barbarian Mechanics")]
	public float minimumAttackRange;
	public float chargeTriggerRange;

	//private bool attackSequenceStarted;
	public bool needsToCharge;
	public float currentDistance;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		CheckDistance();
		CheckObstacles();
	}

	private void CheckObstacles()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * movementDirection, 3f, groundLayer);
		if (hit && playerDistance > maxAttackDistance)
		{
			DoJump();
		}
	}

	private void DoJump()
	{
		Debug.Log("Checking for ground");
		var grounded = Physics2D.Raycast(feet.transform.position, Vector2.down, 0.5f, groundLayer);
		if (grounded)
		{
			Debug.Log("Jumping");
			rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			animator.SetTrigger("Jump");
			//Velocity = Vector2.up * jumpForce;
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
			if (!isAttacking)
			{
				isAttacking = true;
				TriggerAbility();
				yield return new WaitForSeconds(attackRate);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	internal override void TriggerAbility()
	{
		if (phases.Length < 1) return;
		
		if (needsToCharge)
		{
			foreach (var ability in phases[currentPhase].phaseAbilities)
			{
				if (ability.abilityType == Ability.AbilityType.motion)
				{
					if (!(Time.time > ability.abilityTimeUsed + ability.abilityBaseCooldown)) continue;

					AbilityCast(ability);
					return;
				}
			}
		}
		else
		{
			foreach (var ability in phases[currentPhase].phaseAbilities)
			{
				if (ability.abilityType == Ability.AbilityType.motion) continue;

				if (!(Time.time > ability.abilityTimeUsed + ability.abilityBaseCooldown)) continue;
				
				AbilityCast(ability);
				return;
			}
		}
	}

	private void AbilityCast(Ability ability)
	{
		//abilitySource.clip = ability.aSound;
		//abilitySource.Play();
		ability.QueRandomSound();
		animator.SetBool("IsAttacking", true);
		ability.SetParams(gameObject);

		ability.SetTargetPos(player);

		ability.abilityTimeUsed = Time.time;
		ability.TriggerAbility();
	}

	public override void SetPositions(Transform[] tars)
	{
		return;
		/*foreach (var tar in tars)
		{
			if (!tar.GetComponent<JumpTarget>().master) continue;
			//rockThrowPosition = tar;
			return;
		}*/
	}


	private void CheckDistance()
	{
		if (player == null) return;

		playerDistance = Vector2.Distance(transform.position, player.position);

		needsToCharge = playerDistance > chargeTriggerRange;
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

		//yield return new WaitForSeconds(1f);

		EndCurrentPhase();
		StartAttackRoutine();
		SetBossToUI(0);
	}
	#endregion
}
