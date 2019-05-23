using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ChargeAbility")]
public class ChargeAbility : Ability
{
	public float chargeDamage;
	public float chargeForce;
	public float contactForce;

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.motion;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = chargeDamage;
		abilityMachine.initialForce = chargeForce;
		abilityMachine.contactForce = contactForce;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTargetPosition(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.ChargeTarget();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
