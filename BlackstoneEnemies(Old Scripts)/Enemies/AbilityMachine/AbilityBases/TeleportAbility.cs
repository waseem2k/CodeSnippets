using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/TeleportAbility")]
public class TeleportAbility : Ability
{
	public float damage;
	public float attackRadius;
	public float attackDelay;
	public float teleportBackDelay;
	public float contactForce;


	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.motion;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = damage;
		abilityMachine.teleportDelay = teleportBackDelay;
		abilityMachine.abilityDelay = attackDelay;
		abilityMachine.abilityRadius = attackRadius;
		abilityMachine.contactForce = contactForce;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTargetPosition(targetPos);
	}

	public override void TriggerAbility()
	{
		
		abilityMachine.TeleportAttack();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		//throw new System.NotImplementedException();
	}
}
