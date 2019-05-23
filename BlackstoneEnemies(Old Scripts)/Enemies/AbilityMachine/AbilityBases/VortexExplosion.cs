using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/VortexExplosion")]
public class VortexExplosion : Ability
{
	public float damage;
	public float attackRadius;
	public float pullForce;
	public float knockbackForce;
	[Space]
	public float camShakeForce;
	[Space]
	public GameObject implosionPrefab;
	public GameObject explosionPrefab;
	private bool secondPosition;


	public override void Initialize(GameObject obj)
	{
		abilityMachine = obj.GetComponent<AbilityMachine>();
		abilityType = AbilityType.motion;
		abilityTimeUsed = 0;
	}

	public override void SetParams(GameObject obj)
	{
		abilityMachine.damage = damage;
		abilityMachine.abilityRadius = attackRadius;
		abilityMachine.contactForce = knockbackForce;
		abilityMachine.particlePrefab = explosionPrefab;
	}

	public override void SetTargetPos(Transform targetPos)
	{
		if (!secondPosition)
		{
			abilityMachine.SetTargetPosition(targetPos);
			secondPosition = true;
		}
		else
		{
			abilityMachine.SetTarget(targetPos);
		}
		
	}

	public override void TriggerAbility()
	{

		abilityMachine.PlanarVortex();
		abilityMachine.PlayAnimation(animationToPlay);
	}

	public override void BeginCasting()
	{
		abilityMachine.abilityDuration = castTime;
		abilityMachine.camShakeForce = camShakeForce;
		abilityMachine.particlePrefab = implosionPrefab;
		abilityMachine.VortexImplosion(pullForce);
		secondPosition = false;
	}
}
