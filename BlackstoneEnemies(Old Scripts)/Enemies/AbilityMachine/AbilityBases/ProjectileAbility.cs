using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ProjectileAbility")]
public class ProjectileAbility : Ability
{
	public GameObject projectile; // The projectile to spawn
	[Space]
	public float projectileDamage = 10f; // Damage per projectile
	public float projectileForce = 500f; // Force per projectile
	[Range(1, 25)] public int projectileCount = 1; // The number of projectiles to sapwn
	[Range(0, 3)] public int projectileDistanceApart = 2; // Distance between projectiles
	public bool useHighAngle; // To use the high or low angle in calculations
	public bool useTime; // To use time instead of force
	public float projectileTravelTime = 2f; // Time for projectile to reach target


	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.ranged;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = projectileDamage;
		abilityMachine.projectile = projectile;
		abilityMachine.initialForce = projectileForce;
		abilityMachine.projectile = projectile;
		abilityMachine.projectileCount = projectileCount;
		abilityMachine.projectileDistanceApart = projectileDistanceApart;
		abilityMachine.useHighAngle = useHighAngle;
		abilityMachine.useTime = useTime;
		abilityMachine.projectileTravelTime = projectileTravelTime;

	}

	public override void SetTargetPos(Transform targetPos)
	{
		abilityMachine.SetTarget(targetPos);
	}

	public override void TriggerAbility()
	{
		abilityMachine.LaunchProjectile();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		abilityMachine.projectile = projectile;
		abilityMachine.abilityDuration = castTime;
		abilityMachine.SpawnGrowingProjectile();
	}
}
