using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Enemy
{
	[Header("BOSS")]
	public string bossName;

	[Header("Boss Phases")]
	public BossPhase[] phases;

	protected int currentPhase; // The current phase the boss is in

	// Dialogue
	protected BossDialogue bossDialogue;
	protected bool discussionOver;
	protected event Action<int> OnPhaseChange;

	protected virtual void Start()
	{
		player = Player.instance.transform;
	}

	protected virtual void OnEnable()
	{
		InitializeCharacter();
		InitializeBossAbilities();
		health.OnDamaged += OnDamage;
		health.OnDeath += OnDeathEvent;
		bossDialogue = GetComponent<BossDialogue>();
		
	}

	protected virtual void OnDisable()
	{
		health.OnDamaged -= OnDamage;
		health.OnDeath -= OnDeathEvent;
	}

	private void OnDeathEvent()
	{
		Debug.Log("I am being called twice");
		StopAllCoroutines();
		Velocity = Vector2.zero;
		animator.SetBool("Dead", true);
		animator.SetTrigger("Killed");
		attackRoutineStarted = false;
		abilityMachine.StopCurrentOrders();

		bossDialogue.InitiateDefeatDialogue();
		//		StartCoroutine(DeathSpeech());
	}

/*	private IEnumerator DeathSpeech()
	{
		

	}*/

	private void Killed()
	{
		animator.SetTrigger("Killed");

	}

	protected virtual void Update()
	{
		if(isAttacking) isAttacking = animator.GetBool("IsAttacking"); // Updates is attacking when it is set to true by ability trigger
	}

	protected virtual void FixedUpdate()
	{

		if (attackRoutineStarted)
		{
			CheckTargetDirection(player.position);
			UpdateAnimationController();
			UpdatePhases();

			run = GetDirectionToEnemy().x < 0 ? -1 : 1;
			if (isAttacking)
			{
				run = 0;
			}
			Run(run, movementSpeed, movementMaxSpeed, false);
		}
	}

	protected void InitializeBossAbilities()
	{
		foreach (var phase in phases)
		{
			phase.Initialize(gameObject);
		}
	}

	#region BossPhases
	protected void StartNextPhase()
	{
		if(phases.Length > 0) phases[currentPhase].StartPhase();
	}

	protected void EndCurrentPhase()
	{
		if (phases.Length < 1) return;
		if (attackRoutineStarted)
		{
			phases[currentPhase].EndPhase();
			currentPhase++;
			if (currentPhase >= phases.Length) currentPhase = 0;
		}
		StartNextPhase();
		if (OnPhaseChange != null) OnPhaseChange(currentPhase);
	}

	private void UpdatePhases()
	{
		if (health.CurrentHealth <= 0) return;
		if (phases.Length < 1) return;
		if (!phases[currentPhase].phaseStarted) return;

		phases[currentPhase].UpdatePhase();
		if (phases[currentPhase].phaseStarted == false)
		{
			EndCurrentPhase();
		}
	}
	#endregion

	protected override void OnValidate()
	{
		base.OnValidate();
		if (phases.Length < 1) return; // Checks if there is at least 1 element in the list

		for (var i = 0; i < phases.Length; i++)
		{
			phases[i].phaseName = (i + 1).ToString(); // Changes the name of the phase, so it shows up as a number in the inspector
		}
	}

	internal void ToggleDiscussion()
	{
		discussionOver = true;
	}

	internal virtual void TriggerAbility()
	{
		throw new NotImplementedException();
	}

	public virtual void BossIntro()
	{
		throw new NotImplementedException();
	}

	internal virtual void StartAttackRoutine()
	{
		throw new NotImplementedException();
	}

	public virtual void SetPositions(Transform[] tars)
	{
		throw new NotImplementedException();
	}

	protected void SetBossToUI(int frameTarget, int bossTarget = 0, bool fadeName = false)
	{
		BossHealthUI.instance.AssignTarget(gameObject, bossName, frameTarget, bossTarget, fadeName);
	}

	public virtual void ForceStartFight()
	{
		throw new NotImplementedException();
	}

	[Serializable]
	public class BossPhase
	{
		[HideInInspector] public string phaseName;
		[HideInInspector] public float phaseStartTime;
		[HideInInspector] public bool phaseStarted;
		//[HideInInspector]
		public float phaseTimer;

		public enum PhaseType { Duration, HealthGate, Custom }
		[Header("Phase Info")]
		public PhaseType phaseType;
		public float phaseDuration;
		[Range(0f, 1f)] public float healthGate;

		[Header("Phase Abilities")]
		public Ability[] phaseAbilities;
		private Health health;

		

		public void StartPhase()
		{
			phaseTimer = 0;
			phaseStarted = true;
			phaseStartTime = Time.time;
		}

		public void EndPhase()
		{
			if (!phaseStarted) return;
			phaseStarted = false;
		}

		public void UpdatePhase()
		{
			switch (phaseType)
			{
				case PhaseType.Duration:
					phaseTimer += Time.fixedDeltaTime;

					if (phaseTimer > phaseDuration)
					{
						phaseStarted = false;
					}
					break;
				case PhaseType.HealthGate:
					phaseTimer += Time.fixedDeltaTime;

					if (health.CurrentHealth <= health.MaxHealth * healthGate)
					{
						phaseStarted = false;
					}
					break;
				case PhaseType.Custom:
					// Do nothing
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Initialize(GameObject thisObject)
		{
			for (var i = 0; i < phaseAbilities.Length; i++)
			{
				if (phaseAbilities[i] == null)
				{
					Debug.LogError("Ability missing!");
					continue;
				}
				var abilityClone = Instantiate(phaseAbilities[i]);
				abilityClone.Initialize(thisObject);
				phaseAbilities[i] = abilityClone;
			}
			health = thisObject.GetComponent<Health>();
		}
	}
}


