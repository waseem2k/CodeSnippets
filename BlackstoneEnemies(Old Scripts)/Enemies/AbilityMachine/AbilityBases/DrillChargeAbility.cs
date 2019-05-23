using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/DrillChargeAbility")]
public class DrillChargeAbility : Ability
{
	public float contactDamage;
	public float contactForce;
	public float chargeForce;
	public float chargeDistance;
	public int numberOfCharges;

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.ranged;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = contactDamage;
		abilityMachine.contactForce = contactForce;
		abilityMachine.initialForce = chargeForce;
		abilityMachine.motionDistance = chargeDistance;
		abilityMachine.motionCount = numberOfCharges;

	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTargetPosition(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.DrillCharge();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
