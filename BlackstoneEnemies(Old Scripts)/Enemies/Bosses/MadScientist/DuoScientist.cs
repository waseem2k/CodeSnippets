using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuoScientist : MonoBehaviour
{
	public int activePhase;
	public SequencedPhase[] phase;

	private BossArea bossArea;

	private float BossHealth
	{
		get { return phase[activePhase].health.CurrentHealth; }
		set { phase[activePhase].health.CurrentHealth = value; }
	}

	private float MaxHealth { get { return phase[activePhase].health.MaxHealth; } }

	private Vector2 BossPosition
	{
		get { return phase[activePhase].boss.transform.position; }
		set { phase[activePhase].boss.transform.position = value; }
	}

	private GameObject ActiveBoss { get { return phase[activePhase].boss.gameObject; } }

	private float HealthGate { get { return phase[activePhase].healthGate; } }
	private int HealthIndex { get { return phase[activePhase].uiHealthIndex; } }

	private void Start()
	{
		bossArea = GetComponent<BossArea>();
		foreach (var p in phase)
		{
			p.boss.SetPositions(bossArea.movementTargets);
		}
	}

	private void Update()
	{
		if (activePhase >= phase.Length) return;
		if (BossHealth < MaxHealth * HealthGate)
		{
			EnterNextPhase();
		}
	}

	private void EnterNextPhase()
	{
		Debug.Log("Entering next phase");
		float tempHealth = BossHealth;
		Vector2 tempPos = BossPosition;
		ActiveBoss.SetActive(false);
		activePhase++;
		WorldManager.instance.SwapDimension(); // Swap dimensions
		ActiveBoss.SetActive(true);
		BossHealth = tempHealth;
		BossPosition = tempPos;
		BossHealthUI.instance.UpdateTarget(phase[activePhase].boss.health, HealthIndex);
		phase[activePhase].boss.player = Player.instance.transform;
		//phase[activePhase].boss.ForceStartFight();
	}
	
	[Serializable]
	public class SequencedPhase
	{
		public Boss boss;
		public Health health;
		[Range(0,1)] public float healthGate;
		public int uiHealthIndex;
	}
}
