using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/JumpAbility")]
public class JumpAbility : Ability
{
	public float initialForce; // Force the object is propelled at
	public float contactForce; // Force applied when arriving at destination
	public float contactDamage; // Damage applied to nearby targets when arriving at destination
	public float nodeDistance; // The distance to a jump node

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.motion;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = contactDamage;
		abilityMachine.initialForce = initialForce;
		abilityMachine.contactForce = contactForce;
		abilityMachine.nodeDistance = nodeDistance;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTargetPosition(targetPos);
	}
	public override void TriggerAbility()
	{
		abilityMachine.MightyJump();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
