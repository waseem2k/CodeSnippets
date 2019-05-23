using UnityEngine;

public abstract class Ability : ScriptableObject
{
	[Header("Animations")]
	public string animationToPlay;
	[Header("Audio")]
	public AudioClip[] abilitySound;
	public bool looping;

	[Header("Requirements")]
	public float abilityBaseCooldown = 1f;
	public RangeRequirement rangeRequired;
	public float requiredRange;

	[HideInInspector] public float abilityTimeUsed;

	public enum AbilityType { ranged, melee, motion}
	public enum LeapDirection { Away, Towards }
	public enum RangeRequirement { Neither, Melee, Ranged }

	[HideInInspector] public AbilityType abilityType;

	[Header("Casting Ability")]
	public float castTime; // Used by Enemy
	public float abilityDuration; // Duration the ability goes for
	[Space]
	[HideInInspector] public AbilityMachine abilityMachine;

	public abstract void Initialize(GameObject obj);
	public abstract void SetParams(GameObject obj);
	public abstract void SetTargetPos(Transform targetPos);
	public abstract void TriggerAbility();
	public abstract void BeginCasting();

	public void QueRandomSound() // Ques up a sound ready to play
	{
		if (abilitySound.Length < 1) return;
		int random = Random.Range(0, abilitySound.Length - 1);
		abilityMachine.QueueSound(abilitySound[random], looping);
	}
}
