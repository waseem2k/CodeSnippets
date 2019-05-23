using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SwordAbility")]
public class SwordAbility : Ability
{
	public float swordDamage = 10f;
	public float slamForce = 500f;
	public bool isSlamAbility;

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.melee;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = swordDamage;
		abilityMachine.contactForce = slamForce;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTarget(targetPos);
	}

	public override void TriggerAbility()
	{
		if (isSlamAbility)
		{
			abilityMachine.GroundSlam();
		}
		else
		{
			abilityMachine.BasicSwing();
		}

		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
