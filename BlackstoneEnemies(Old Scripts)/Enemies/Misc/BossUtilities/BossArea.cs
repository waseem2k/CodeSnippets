using System.Collections;
using System.Globalization;
using DG.Tweening;
//using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))] // Required to initiante the boss fight
public class BossArea : MonoBehaviour
{
	public Transform bossToSpawn; // The position the boss spawns, set in inspector
	public float spawnDelay = 2f; // time to wait before spawning the boss
	[Tooltip("Ignores name check, forcing the boss to spawn regardless of being killed")]
	public bool overrideNameCheck; // If checked, it bypasses the name check

	[Header("Music")]
	public AudioClip bossTrack;
	
	[Header("Door")]
	public LockedDoor door;

	public AudioClip doorUnlockClip;

	[Header("Key")]
	public DoorKey key;

	public AudioClip keyPickupClip;


	//public BossDoorway[] doorTargets; // Door targets, to close when boss is engaged

	[Header("Movement Targets")]
	public Transform[] movementTargets; // Targets for the boss to use

	[Header("Boss Area")]
	public Vector2 areaSize; // Boss spawn area size;
	[Space]
	public Texture2D backgroundTexture; // The background texture for gizmo
	public BossTrigger bossTrigger;
	public GameObject bossSpawnEffect;
	public AudioSource audioSource;

	// Private variables
	private Health bossHealth; // To keep an eye on the bosses health
	private BoxCollider2D areaCollider;
	private enum SpawnState { None, Spawning, Attacking, Dead }
	private SpawnState spawnState; // Bosses current state
	private bool targetFound;
	private string bossName;

	[HideInInspector] public Transform target;

	private void Awake()
	{
		if (key != null) key.OnPickup += KeyPickup;
		if (door != null) door.OnDoorCheck += DoorCheck;
	}

	private void Start()
	{
		spawnState = SpawnState.None; // To make sure the boss is set to none at the start
		
		if (bossToSpawn == null) // Checks if a boss is attached
		{
			Debug.LogError("Boss missing from Boss Area");
			return;
		}

		bossName = bossToSpawn.GetComponent<Boss>().Name;
		bossHealth = bossToSpawn.GetComponent<Health>();
		bossToSpawn.gameObject.SetActive(false);
		DoorAndKeyCheck();
	}

	private void OnDisable()
	{
		if(key != null) key.OnPickup -= KeyPickup;
		if(door != null) door.OnDoorCheck -= DoorCheck;
	}

	private void Update()
	{
		BossHealthCheck(); // Checks if boss is alive

		if (target == null) return;
		if (bossTrigger == null) return;

		var bState = BossManager.instance.GetBossStatus(bossName);
		if (bState == BossStatus.Dead && !overrideNameCheck) return;

		if (spawnState != SpawnState.None) return;
		targetFound = bossTrigger.areaCollider.OverlapPoint(target.position);

		if (!targetFound) return;
		spawnState = SpawnState.Spawning;
		StartCoroutine(BeginSpawningBoss());
	}

	private void BossHealthCheck()
	{
		if (spawnState != SpawnState.Attacking) return;

		if (bossHealth.CurrentHealth > 0) return;
		OpenArenaDoors();
		spawnState = SpawnState.Dead; // sets to dead
		BossManager.instance.SetStatus(bossName, BossStatus.Dead);
		BossManager.instance.SetCombatState(false);
		CameraManager.instance.RemoveBoss(); // Removes the boss from camera
		BossHealthUI.instance.HideHealthBar();
		AudioManager.instance.UpdateMusicTrack();
		if(door != null) door.OpenDoor();
	}


	private IEnumerator BeginSpawningBoss() // Spawns the boss after a delay
	{
		CameraManager.instance.AssignBoss(bossToSpawn); // Sets the boss to camera for tracking
		BossManager.instance.SetCombatState(true);
		target.GetComponent<Player>().enabled = false;
		target.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		var go = Instantiate(bossSpawnEffect, bossToSpawn);
		var ps = go.GetComponent<ParticleSystem>();
		ps.Play();
		Destroy(go, ps.main.duration);

		yield return new WaitForSeconds(ps.main.duration * 0.5f);

		bossToSpawn.gameObject.SetActive(true);
		var boss = bossToSpawn.GetComponent<Boss>();
		boss.player = target;
		if (movementTargets.Length > 0) boss.SetPositions(movementTargets);

		yield return new WaitForSeconds(spawnDelay);

		boss.BossIntro();
		if(door != null) door.CloseDoor();

		yield return new WaitUntil(() => boss.attackRoutineStarted);
		AudioManager.instance.PlayTrack(bossTrack);
		target.GetComponent<Player>().enabled = true;
		CloseArenaDoors();
		spawnState = SpawnState.Attacking;
	}

	private void OnTriggerEnter2D(Collider2D other) // Starts the boss spawn sequence if player enters the trigger
	{
		if (bossToSpawn == null) return; // Doesn't do anything if the boss doesn't exist
		if (spawnState != SpawnState.None) return; // Only runs if set to the none state

		var tar = other.transform.root.GetComponentInChildren<Player>();

		if (tar == null) return;

		target = tar.transform;
	}

	#region DoorsAndKeys
	private void KeyPickup() // Tells the boss manager that this key has been picked up
	{
		
		key.OnPickup -= KeyPickup;
		string id = key.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture);
		ItemsArea.instance.AddItem("key", id, key.GetComponent<SpriteRenderer>().sprite, key.GetComponent<SpriteRenderer>().color);
		BossManager.instance.PickupKey(bossName, id);
		Destroy(key.gameObject);
		door.LockDoor(false);
		if(audioSource != null && keyPickupClip != null) audioSource.PlayOneShot(keyPickupClip);
	}

	private void DoorCheck()
	{
		door.OnDoorCheck -= DoorCheck;
		BossManager.instance.UnlockDoor(bossName);
		door.OpenDoor();
		audioSource.PlayOneShot(doorUnlockClip);
	}

	private void DoorAndKeyCheck() // Checks if boss key has been aquired and whether
	{
		BossInfo bossInfo = BossManager.instance.GetBossInfo(bossName); // Find the boss attached to this area in the Boss Manager
		if (bossInfo == null) return; // If the boss is not found, it does nothing
		if (bossInfo.keyAquired)
		{
			Destroy(key.gameObject);
			door.LockDoor(false);
		}

		if (bossInfo.doorUnlocked)
		{
			door.OnDoorCheck -= DoorCheck;
			door.LockDoor(false);
			door.transform.DOMoveY(transform.position.y + 4, 1f);
		}
		
		//if (door != null) door.LockDoor(bossInfo.doorUnlocked);
	}

	private void OpenArenaDoors() // Opens any attached doors
	{
		/*if (doorTargets.Length < 1) return; // Check if any doors are attached
		foreach (var tar in doorTargets)
			if (tar != null) tar.Open(); // Open doors	*/
	}

	private void CloseArenaDoors() // Closes any attached doors
	{
		/*if (doorTargets.Length < 1) return; // Check if any doors are attached
		foreach (var tar in doorTargets)
			if (tar != null) tar.Close(); // Close doors	*/
	}
	#endregion

	#region Set-Up
	private void OnDrawGizmos()
	{
		if (backgroundTexture == null) return;

		transform.position = new Vector3(transform.position.x.RoundToNearest(1), transform.position.y.RoundToNearest(1), 0);

		var gizmoSize = new Vector2(areaSize.x.RoundToNearest(2), areaSize.y.RoundToNearest(2));
		var texturePos = new Vector2(transform.position.x - gizmoSize.x / 2, transform.position.y - gizmoSize.y / 2);

		Gizmos.DrawGUITexture(new Rect(texturePos, gizmoSize), backgroundTexture);
	}


	private void OnValidate() // Updates the boss area based on the given dimensions 
	{
		if (areaCollider == null) areaCollider = GetComponent<BoxCollider2D>();
		areaCollider.size = new Vector2(areaSize.x.RoundToNearest(2), areaSize.y.RoundToNearest(2));
	}
	#endregion


}
