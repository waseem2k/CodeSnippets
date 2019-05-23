using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/BioAbility")]
public class BioAbility : Ability
{
	public GameObject projectile; // The projectile to spawn
	[Space]
	public float damage = 10f; // Damage per projectile
	public float force = 100f; // Force per projectile
	public float bioArc = 30f; // Max arc for projectiles
	[Range(1, 50)] public int projectileCount = 5; // The number of projectiles to spawn
	public float attackDuration; // The duration to spawn projectiles
	

	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.ranged;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.projectile = projectile;
		abilityMachine.damage = damage;
		abilityMachine.initialForce = force;
		abilityMachine.bioArc = bioArc;
		abilityMachine.projectileCount = projectileCount;
		abilityMachine.abilityDuration = attackDuration;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTarget(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.BioAttack();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		throw new System.NotImplementedException();
	}
}
