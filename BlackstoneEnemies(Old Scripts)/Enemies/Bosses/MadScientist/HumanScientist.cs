using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanScientist : Boss
{
	[Header("Human Scientist Mechanics")]
	public float maxLeapDistance = 10f;

	[Header("Air Phase")]
	public float heightFromTarget = 5f;

	[Header("Misc")]
	public Transform switchPosition;

	public GameObject PhaseTwoTransitionEffect;
	public GameObject PhaseThreeTransitionEffect;

	public List<Transform> jumpTargets;

	private bool phaseTwoStarted;
	private  bool switchPositionReached;
	public bool abilityComplete;

	public bool firstFight;

	protected override void Start()
	{
		base.Start();
		OnPhaseChange += PhaseChange;
	}

	protected override void Update()
	{
		if (attackRoutineStarted && Player.instance.health.CurrentHealth <= 0)
		{
			BossHealthUI.instance.HideHealthBar();
			attackRoutineStarted = false;
		}
		base.Update();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!attackRoutineStarted) return;
		//CheckDistance();
		if(abilityComplete && currentPhase == 0) MoveToPlayer(player); // Melee phase
		if(currentPhase == 1 && phaseTwoStarted) Hover(); // Air Phase


	}

	#region Phases

	private void PhaseChange(int phase)
	{
		switch (phase)
		{
			case 0:
				break;
			case 1:
				StartCoroutine(PhaseTwoTransition());
				break;
				
			case 2:
				StartCoroutine(PhaseThreeTransition());
				break;
		}
	}

	private IEnumerator PhaseTwoTransition()
	{
		var newPos = new Vector2(transform.position.x, transform.position.y + heightFromTarget);
		DoKnockback();
		rb.gravityScale = 0;
		rb.MovePosition(newPos);
		rb.velocity = Vector2.zero;
		var effect = Instantiate(PhaseTwoTransitionEffect, transform);
		Destroy(effect, 2f);
		yield return new WaitForSeconds(2f);
		phaseTwoStarted = true;
	}

	private IEnumerator PhaseThreeTransition()
	{
		phaseTwoStarted = false;
		DoKnockback();
		rb.gravityScale = 1;
		rb.velocity = Vector2.zero;
		var effect = Instantiate(PhaseThreeTransitionEffect, transform);
		Destroy(effect, 2f);
		GetComponent<Health>().invulnerable = true;
		StartCoroutine(MoveToSwitch());
		
		yield return new WaitUntil(() => switchPositionReached);
		Velocity = Vector2.zero;
		animator.SetTrigger("UseDevice");
		CameraManager.instance.ShakeCamera(5, 0.5f);
		yield return new WaitForSeconds(2f);		
		WorldManager.instance.SwapDimension();
		ArtoriusSequence.instance.DropPlatforms();
		Destroy(gameObject, 2f);
	}

	private IEnumerator MoveToSwitch()
	{
		autoRotate = false;
		RotateMe(true);
		var dist = Vector2.Distance(transform.position, switchPosition.position);

		while (dist > 1f)
		{
			dist = Vector2.Distance(transform.position, switchPosition.position);
			rb.AddForce(Vector2.right * movementSpeed, ForceMode2D.Force);
			yield return new WaitForEndOfFrame();
		}
		Velocity = Vector2.zero;
		switchPositionReached = true;
		BossHealthUI.instance.HideHealthBar();
	}

	private void Hover()
	{
		if (player == null) return; // Check to make sure a target exists

		var calculatedHeight = Mathf.Abs(player.position.y - transform.position.y); // The current height difference
		var dir = 1; // The direction to move the player, 1 is up, -1 is down

		if (calculatedHeight > heightFromTarget) // Change the direction if height is too high
		{
			dir = -1;
		}

		rb.AddForce(Vector2.up * dir * movementSpeed, ForceMode2D.Force); // Add force in the required direction
	}

	#endregion

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

			if (currentPhase > 1)
			{
				attackRoutineStarted = false;
				yield break;
			}
			CheckAbility();
			abilityComplete = false;

			yield return new WaitUntil(() => abilityComplete && animator.GetBool("AbilityHold") == false);

			yield return new WaitForSeconds(attackRate);
		}
	}

	private void CheckAbility()
	{
		
		if (phases.Length < 1) return;
		if (currentPhase == 1 && !phaseTwoStarted) return;
		if (currentPhase > 1) return;

		foreach (var ability in phases[currentPhase].phaseAbilities)
		{
			if (!(Time.time > ability.abilityTimeUsed + ability.abilityBaseCooldown)) continue;

			if (currentPhase == 0 && ability.rangeRequired == Ability.RangeRequirement.Ranged)
			{
				if(Vector2.Distance(transform.position, player.position) < minAttackDistance) LeapAway();
			}

			if(Vector2.Distance(transform.position, player.position) > maxAttackDistance) LeapTowards();

			StartCoroutine(UseAbility(ability));
			return;
		}
	}

	private IEnumerator UseAbility(Ability ability)
	{
		animator.SetBool("DrawComplete", false);
		animator.SetTrigger(ability.animationToPlay);

		yield return new WaitUntil(() => animator.GetBool("DrawComplete"));
		ability.SetParams(gameObject);
		ability.SetTargetPos(player);
		ability.abilityTimeUsed = Time.time;
		ability.TriggerAbility();
		ability.QueRandomSound();
		yield return new WaitForSeconds(ability.abilityDuration); // Using cast time as duration
		animator.SetTrigger("AbilityComplete");
		abilityComplete = true;
	}

	private void DoKnockback()
	{
		abilityMachine.abilityRadius = 20f;
		abilityMachine.contactForce = 5000;
		abilityMachine.damage = 10f;
		abilityMachine.Knockback();
	}

	private void LeapAway()
	{
		abilityMachine.damage = 10f;
		abilityMachine.contactForce = 1;
		abilityMachine.initialForce = 10;
		abilityMachine.motionDistance = 15;
		abilityMachine.SetTarget(player);
		abilityMachine.SetTargetPosition(player);
		abilityMachine.LeapAwayFromTarget();
		animator.SetTrigger("Jump");
	}

	private void LeapTowards()
	{
		abilityMachine.damage = 10f;
		abilityMachine.contactForce = 1;
		abilityMachine.initialForce = 10;
		abilityMachine.motionDistance = 15;
		abilityMachine.SetTarget(player);
		abilityMachine.SetTargetPosition(player);
		abilityMachine.LeapToTarget();
		animator.SetTrigger("Jump");
	}
	#endregion

	#region BossIntro
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
		discussionOver = false;

		//yield return new WaitForSeconds(1f);

		EndCurrentPhase();
		StartAttackRoutine();
		SetBossToUI(1, 0, true);
	}

	public override void ForceStartFight()
	{
		EndCurrentPhase();
		StartAttackRoutine();
	}
	#endregion

	public override void SetPositions(Transform[] tars)
	{
		jumpTargets = tars.ToList();
	}
}
