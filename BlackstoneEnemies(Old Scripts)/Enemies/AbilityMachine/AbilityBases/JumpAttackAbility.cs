using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/JumpAttackAbility")]
public class JumpAttackAbility : Ability
{
	public float contactForce;
	public float contactDamage;
	public float jumpForce;

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
		abilityMachine.initialForce = jumpForce;
	}
	 
	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTargetPosition(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.JumpAttack(rangeRequired);
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
