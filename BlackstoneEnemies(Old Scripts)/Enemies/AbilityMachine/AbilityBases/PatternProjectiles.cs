using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/PatternProjectile")]
public class PatternProjectiles : Ability
{
	public GameObject projectile;

	public float projectileDamage;
	public float projectileForce;
	public int projectileCount;

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.ranged;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.projectile = projectile;
		abilityMachine.damage = projectileDamage;
		abilityMachine.initialForce = projectileForce;
		abilityMachine.projectileCount = projectileCount;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTarget(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.LaunchCircleProjectiles();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		abilityMachine.projectile = projectile;
		abilityMachine.damage = projectileDamage;
		abilityMachine.initialForce = projectileForce;
		abilityMachine.projectileCount = projectileCount;
		abilityMachine.abilityDuration = castTime;
		abilityMachine.SpawnProjectilesCircle();
	}
}
