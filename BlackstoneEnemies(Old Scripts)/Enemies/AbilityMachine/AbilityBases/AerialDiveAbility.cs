using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/AerialDiveAbility")]
public class AerialDiveAbility : Ability
{
	public float damage;
	public float diveForce;
	public float contactForce;
	public float knockbackDelay;
	public float returnDelay;
	public float contactRadius;
	public float timeToContact;

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.motion;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = damage;
		//abilityMachine.initialForce = diveForce;
		abilityMachine.projectileTravelTime = timeToContact;
		abilityMachine.contactForce = contactForce;
		abilityMachine.abilityDelay = knockbackDelay;
		abilityMachine.aftermathDelay = returnDelay;
		abilityMachine.abilityRadius = contactRadius;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTargetPosition(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.DiveAttack();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
