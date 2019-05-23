using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaperManager : MonoBehaviour
{
	public GameObject reaper; // The reaper prefab to spawn
	public float timeToSpawn; // The time to wait before spawning a boss
	public float spawnDistance; // The distance at which to spawn
	public int deadBosses; // The required number of dead bosses
	[Space]
	public Dialogue reaperSense; // Dialogue to let the players know that they are being followed

	private GameObject reaperHot; // The instantiated object of reaper

	private bool timerStarted; // If the timer is counting down
	private bool reaperHasSpawned; // The reaper is spawned and active

	public float TimeLeft { get; private set; }

	public static ReaperManager instance;

	public void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
		transform.parent = null;
		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		WorldManager.instance.OnDimensionSwap += CheckReaperDim;
		
	}

	private void OnDisable()
	{
		WorldManager.instance.OnDimensionSwap -= CheckReaperDim;
		
	}
	
	private void Update ()
	{
		if (!timerStarted) return;
		
		// Update time left

		TimeLeft -= Time.deltaTime;
		if (TimeLeft < 0)
		{
			SpawnReaper();
		}
	}

	private void SpawnReaper()
	{
		// Spawn the reaper if enough time has passed and one doesn't already exist in the scene
		reaperHot = Instantiate(reaper);
		Vector2 reaperPos = Random.onUnitSphere * spawnDistance;
		reaperHot.transform.position = reaperPos;

		reaperHot.GetComponent<Reaper>().target = Player.instance.transform;
		reaperHasSpawned = true;
		timerStarted = false;
	}

	private void CheckReaperDim(Dimension dim)
	{
		if (BossManager.instance.GetBossesKilled() < deadBosses) return;
		switch (dim)
		{
			case Dimension.Darkworld:
				DialogueManager.instance.OnDialogueEnded += StartTimer;
				DialogueManager.instance.DisplayDialogue(reaperSense);
				break;
			case Dimension.Overworld:
				timerStarted = false;

				if (reaperHasSpawned) // If we exit the dark world, the spawned reaper will dissappear
				{
					Destroy(reaperHot);
					reaperHasSpawned = false;
				}
				break;
		}
	}

	private void StartTimer()
	{
/*		if (BossManager.instance.GetBossesKilled() < deadBosses) return;
		if (WorldManager.instance.currentDimension != Dimension.Darkworld) return;*/
		TimeLeft = timeToSpawn;
		timerStarted = true;
		DialogueManager.instance.OnDialogueEnded -= StartTimer;
	}
}
