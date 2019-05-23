using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmouredRifleman : Boss
{
	[Header("Armoured Rifleman Mechanics")]

	public float minPlayerDistanceToJump; // The distance the player needs to be from rifleman to jump
	public float jumpWaitTime; // Time to wait before jumping
	public float jumpTime; // TIme it takes to reach destination
	public float jumpCooldown; // Time to wait before able to jump again

	public Transform masterJumpTarget;
	public List<Transform> jumpTargets;
	public Transform currentJumpTarget;
	[Space]
	public Transform eyes; // The point from which to check sight to target
	public LayerMask sightLayer;
	

	public bool playerClose; // If the player is closer than the min distance
	public bool playerInSight; // If the player is in sight
	public bool movingSoon; // Check to see if the boss should move
	public bool isJumping; // If the Rifleman is currently jumping
	public bool hasJumped; // Has jumped recently
	private float timeJumpWasUsed; // The time the jump was used

	[Header("Shooting")]
	public SpriteRenderer mainSprite;
	public SpriteRenderer[] bodySprites;
	public Animator armAnimator;
	private float armPosition;

	protected override void Update()
	{
		//base.Update();

		playerClose = GetDistanceToEnemy() < minPlayerDistanceToJump;
		playerInSight = CheckSightTarget(eyes.position, player.position, groundLayer);

		armPosition = ArmAngle();
		if (armAnimator.gameObject.activeInHierarchy)
		{
			armAnimator.SetFloat("Pos", armPosition * 0.4f);
		}

		if (playerClose || !playerInSight)
		{
			if (!movingSoon && !hasJumped)
			{
				StartCoroutine(MoveCountdown());
			}
		}

		if (hasJumped && Time.time > timeJumpWasUsed + jumpCooldown)
		{
			hasJumped = false;
		}

		if (isJumping)
		{

			if (Vector2.Distance(transform.position, currentJumpTarget.position) < 0.5f)
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

	private float ArmAngle()
	{
		// angle between 5 and 120
		Vector3 relative = transform.InverseTransformPoint(player.position);
		float angle = Mathf.Atan2(-relative.x, -relative.y);
		
		return -angle;
	}

	private IEnumerator MoveCountdown()
	{
		movingSoon = true;
		yield return new WaitForSeconds(jumpWaitTime);
		JumpToPosition();
		movingSoon = false;
		hasJumped = true;
		timeJumpWasUsed = Time.time;
	}

	private void JumpToPosition()
	{
		BodypartToggleTrigger(true);
		currentJumpTarget = FindSuitableJumpTarget();
		Velocity = ExtensionMethods.CalculateTrajectoryWithTime(transform.position, currentJumpTarget.position, jumpTime);
		isJumping = true;
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
			if (currentPhase == 0)
			{
				if (laserSight != null) laserSight.SetTarget(player, true);
				mainSprite.enabled = false;
				foreach (var b in bodySprites)
				{
					b.enabled = true;
				}
				yield return new WaitForSeconds(chargeUpTime);
				mainSprite.enabled = true;
				foreach (var b in bodySprites)
				{
					b.enabled = false;
				}
			}
			TriggerAbility();
			laserSight.HideLine();
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
			ability.QueRandomSound();
			ability.SetParams(gameObject);

			ability.SetTargetPos(player);
			ability.abilityTimeUsed = Time.time;
			ability.TriggerAbility();
			return;
		}
	}


	private Transform FindSuitableJumpTarget()
	{
		if (currentPhase == 1) return masterJumpTarget;

		foreach (var j in jumpTargets)
		{
			var hit = Physics2D.Linecast(j.position, player.position, sightLayer);
			if (!hit) continue;

			var tempPlayer = hit.transform.root.GetComponentInChildren<Player>();

			if (tempPlayer)
			{
				if (currentJumpTarget == null)
				{
					return j;
				}
				if (Vector2.Distance(j.position, currentJumpTarget.position) > 5)
				{
					return j; //currentJumpTarget = j;
				}
			}
		}
		return jumpTargets[0];
	}

	#region BossIntro
	public override void BossIntro()
	{
		StartCoroutine(IntroSequence(5000));
	}

	private IEnumerator IntroSequence(float _movementForce)
	{

		rb.AddForce(Vector2.down * _movementForce, ForceMode2D.Impulse);

		yield return new WaitForSeconds(1f);

		bossDialogue.InitiateDialogue();
		DialogueManager.instance.OnDialogueEnded += ToggleDiscussion;
		yield return new WaitUntil(() => discussionOver);
		DialogueManager.instance.OnDialogueEnded -= ToggleDiscussion;

		EndCurrentPhase();
		StartAttackRoutine();
		SetBossToUI(0);
	}
	#endregion

	public override void SetPositions(Transform[] tars)
	{
		foreach (var tar in tars)
		{
			if (tar.GetComponent<JumpTarget>().master)
			{
				masterJumpTarget = tar;
			}
			else
			{
				jumpTargets.Add(tar);
			}
		}
	}

}

