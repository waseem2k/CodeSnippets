using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/PotionAbility")]
public class PotionAbility : Ability
{
	public GameObject potionProjectile;
	public float potionDamage;
	public float throwForce;
	public float contactForce;
	public float delay;

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.ranged;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.projectile = potionProjectile;
		abilityMachine.damage = potionDamage;
		abilityMachine.initialForce = throwForce;
		abilityMachine.contactForce = contactForce;
		abilityMachine.abilityDelay = delay;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTarget(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.PotionThrow();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
