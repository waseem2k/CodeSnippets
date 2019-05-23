using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class WingedBeast : Boss
{
	[Header("Winged Beast Mechanics")]
	public float heightFromPlayer; // The height from the player the beast will try and attain at all times
	public float distanceFromPlayer; // The distance from player the beast will try and attain at all times

	[Space]
	public float flightSpeed = 10f; // The force at which the beast moves
	public int maxFlightCount; // The number of times the beast will fly across

	//Private
	private Transform[] flightPosition; // Positions the beast flies between
	private Transform centerPosition; // The central position for the beast to return to
	private Vector3 currentTargetPosition; // The current position the beast is moving towards
 
	private bool targetReached; // Set to true once the target position has been reached
	private int flybyCount; // The number of times the boss has done a flyby

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!attackRoutineStarted) return;
		
		switch (currentPhase)
		{
			case 0:
				if (autoRotate == false) autoRotate = true;
				flybyCount = 0;
				MoveToPlayer(player);
				Movement();
				break;
			case 1:
				if (autoRotate) autoRotate = false;
				FlyToTargetPosition(flightSpeed);
				CheckDistanceToTargetPosition();
				ChangeTargetPosition();
				CheckForPhaseChange();
				break;
			default:
				Debug.LogError("Current Phase is out of range");
				break;
		}
	}

	private void Movement() // Phase 1 movement
	{
		if (player == null) return; // Check to make sure a target exists

		var calculatedHeight = Mathf.Abs(player.position.y - transform.position.y); // The current height difference
		var dir = 1; // The direction to move the player, 1 is up, -1 is down

		if (calculatedHeight > heightFromPlayer) // Change the direction if height is too high
		{
			dir = -1;
		}

		rb.AddForce(Vector2.up * dir * movementSpeed, ForceMode2D.Force); // Add force in the required direction
	}

	#region FlightPhase
	private void FlyToTargetPosition(float _movementForce) // Moves the beast in the direction of it's target
	{
		var dir = (currentTargetPosition - transform.position).normalized;
		rb.AddForce(dir * _movementForce, ForceMode2D.Force);

		var faceRight = !(currentTargetPosition.x < transform.position.x); // Checks the direction of its target
		RotateMe(faceRight); // Rotates in the direction it's target is
	}

	private void CheckDistanceToTargetPosition() // Checks the distance to it's flight position and stops the beasts motion once a position has been reached, ready to turn around
	{
		if (!CheckDistance(currentTargetPosition)) return;

		rb.velocity = Vector2.zero;
		targetReached = true;
		flybyCount++;
	}
	private void ChangeTargetPosition() // Requests a new position once the current one has been reached
	{
		if (!targetReached) return;
		targetReached = false;
		currentTargetPosition = RequestNextTarget(maxFlightCount, currentTargetPosition);
		if (attackRoutineStarted && flybyCount > 0 && flybyCount < maxFlightCount + 1) TriggerAbility();
	}

	private void CheckForPhaseChange() // Ends the flight phase once the beast has flown past enough times
	{
		if (flybyCount < maxFlightCount + 1) return;
		currentTargetPosition = flightPosition[0].position;
		EndCurrentPhase();
	}

	private bool CheckDistance(Vector2 _target) // Checks the distance between the beast and it's target
	{
		var dist = Vector2.Distance(transform.position, _target);
		return dist < 5f; // Distance of 5 units hardcoded in
	}

	private Vector3 RequestNextTarget(int maxCount, Vector3 _currentTarget) // Returns a new target position that is not the same as it's current position
	{
		if (flybyCount < maxCount) // Checks if the max flight count has been reached
		{
			foreach (var t in flightPosition)
			{
				if (t.position != _currentTarget)
				{
					return t.position; // Returns a position on the map that is not central
				}
			}
		}
		return centerPosition.position; // Returns the central position when max count has been reached
	}
	#endregion

	#region BossIntro
	public override void BossIntro()
	{
		StartCoroutine(IntroSequence(5000, 3));
	}

	private IEnumerator IntroSequence(float _movementForce, int maxFlights)
	{
		flybyCount = 0;
		autoRotate = false;
		targetReached = true;
		var nextTarget = Vector3.zero; //RequestNextTarget(maxFlights, Vector3.zero);

		while (flybyCount < maxFlights)
		{
			if (targetReached)
			{
				nextTarget = RequestNextTarget(2, nextTarget);
				targetReached = false;
				var dir = (nextTarget - transform.position).normalized;

				rb.AddForce(dir * _movementForce, ForceMode2D.Impulse);

				var faceRight = !(nextTarget.x < transform.position.x);
				RotateMe(faceRight);

				flybyCount++;
			}

			if (CheckDistance(nextTarget))
			{
				rb.velocity = Vector2.zero;
				targetReached = true;
			}

			yield return new WaitForEndOfFrame();
		}

		var returning = false;
		while (!returning)
		{
			if (CheckDistance(nextTarget))
			{
				rb.velocity = Vector2.zero;
				returning = true;
			}

			yield return new WaitForEndOfFrame();
		}

		autoRotate = true;

		bossDialogue.InitiateDialogue();
		DialogueManager.instance.OnDialogueEnded += ToggleDiscussion;
		yield return new WaitUntil(() => discussionOver);
		DialogueManager.instance.OnDialogueEnded -= ToggleDiscussion;

		//yield return new WaitForSeconds(1f);

		EndCurrentPhase();
		StartAttackRoutine();
		SetBossToUI(0);
		currentTargetPosition = flightPosition[0].position;
	}
	#endregion



	#region Abilities
	internal override void StartAttackRoutine() // Starts the attack routine for the boss
	{
		StartCoroutine(AttackSequence());
	}

	internal IEnumerator AttackSequence() // Triggers an ability after enough time has passed
	{
		attackRoutineStarted = true;

		while (true)
		{
			if(currentPhase == 0)
				TriggerAbility();

			yield return new WaitForSeconds(attackRate);
		}
	}

	internal override void TriggerAbility() // Generic trigger for abilities, checks the phase and triggers abilities accordingly
	{
		if (phases.Length < 1) return;

		foreach (var ability in phases[currentPhase].phaseAbilities)
		{
			switch (currentPhase)
			{
				case 0:
					if (!(Time.time > ability.abilityTimeUsed + ability.abilityBaseCooldown)) continue;
					UseAbility(ability);
					return;
				case 1:
					Debug.Log("Flight Phase Trigger");
					animator.SetBool("AbilityHold", true);
					UseAbility(ability);
					return;
			}
		}
	}

	private void UseAbility(Ability ability) // Actually triggers the ability
	{
		//abilitySource.clip = ability.aSound;
		//abilitySource.Play();
		ability.QueRandomSound();
		ability.SetParams(gameObject);

		ability.SetTargetPos(player);

		ability.abilityTimeUsed = Time.time;
		ability.TriggerAbility();
	}
	#endregion

	// Init
	public override void SetPositions(Transform[] tars) // Sets the bosses target positions as required
	{
		var flightTargets = new List<Transform>();
		foreach (var t in tars)
		{
			var jumpTar = t.GetComponent<JumpTarget>();
			if (jumpTar.master)
			{
				centerPosition = t;
			}
			else
			{
				flightTargets.Add(t);
			}
		}
		flightPosition = flightTargets.ToArray();
	}
}
