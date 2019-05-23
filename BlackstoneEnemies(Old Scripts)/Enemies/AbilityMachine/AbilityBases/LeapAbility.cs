using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/LeapAbility")]
public class LeapAbility : Ability
{
	public LeapDirection leapDirection;
	
	public float leapMaxDistance;
	public float leapForce;
	public float contactDamage;
	public float contactForce;
	
	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.motion;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = contactDamage;
		abilityMachine.contactForce = contactForce;
		abilityMachine.initialForce = leapForce;
		abilityMachine.motionDistance = leapMaxDistance;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTarget(targetPos);
		abilityMachine.SetTargetPosition(targetPos);
	}

	public override void TriggerAbility()
	{
		switch (leapDirection)
		{
			case LeapDirection.Away:
				abilityMachine.LeapAwayFromTarget();
				break;
			case LeapDirection.Towards:
				abilityMachine.LeapToTarget();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new NotImplementedException();
	}
}
