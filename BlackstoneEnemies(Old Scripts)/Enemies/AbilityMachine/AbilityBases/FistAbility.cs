using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/FistAbility")]
public class FistAbility : Ability
{
	public float fistDamage = 10f;
	public float fistForce = 500f;

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.melee;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = fistDamage;
		abilityMachine.contactForce = fistForce;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTarget(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.Punch();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
