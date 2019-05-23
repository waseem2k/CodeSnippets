using UnityEngine;

public class EnemyDimensionSwapper : MonoBehaviour
{
	/// <summary>
	/// Class for swapping enemies, will add more to this to make sure enemies reset correctly
	/// </summary>
	public GameObject lightWorldEnemies;
	public GameObject darkWorldEnemies;
	[Space]
	public float distanceFromPlayer = 60f; // The distance from the player the enemies need to be within to become enabled
	public float timeBetweenChecks = 2f; // The time to wait before checking
	
	private Enemy[] lightEnemies;
	private Enemy[] darkEnemies;
	private Transform player; // The player to check distance
	private bool disableToggle; // Disables the toggling of enemies
	private float lastChecked; // The last time it was checked

	private void Start()
	{
		player = Player.instance.transform;
		FindEnemyTransforms();
	}

	private void FindEnemyTransforms()
	{
		lightEnemies = lightWorldEnemies.transform.GetComponentsInChildren<Enemy>();
		darkEnemies = darkWorldEnemies.transform.GetComponentsInChildren<Enemy>();
	}

	private void OnEnable ()
	{
		WorldManager.instance.OnDimensionSwap += ToggleEnemies;
		BossManager.instance.EnteringBossCombat += CheckForBossCombat;
	}

	private void OnDisable()
	{
		WorldManager.instance.OnDimensionSwap -= ToggleEnemies;
		BossManager.instance.EnteringBossCombat -= CheckForBossCombat;
	}
	
	private void ToggleEnemies(Dimension dim)
	{
		if (disableToggle) return;

		lightWorldEnemies.SetActive(dim == Dimension.Overworld);
		darkWorldEnemies.SetActive(dim == Dimension.Darkworld);
	}

	private void CheckForBossCombat(bool engaged)
	{
		disableToggle = engaged;

		if (engaged)
		{
			lightWorldEnemies.SetActive(false);
			darkWorldEnemies.SetActive(false);
			return;
		}

		lightWorldEnemies.SetActive(WorldManager.instance.currentDimension == Dimension.Overworld);
		darkWorldEnemies.SetActive(WorldManager.instance.currentDimension == Dimension.Darkworld);
	}

	private void Update()
	{
		if (disableToggle) return;
		if (Time.time < lastChecked + timeBetweenChecks) return;
		lastChecked = Time.time;
		switch (WorldManager.instance.currentDimension)
		{
			case Dimension.Overworld:
				foreach (var enemy in lightEnemies)
				{
					if (enemy != null)
					{
						enemy.gameObject.SetActive(Vector2.Distance(enemy.transform.position, player.position) < distanceFromPlayer);
					}
				}
				break;
			case Dimension.Darkworld:
				foreach (var enemy in darkEnemies)
				{
					if (enemy != null)
					{
						enemy.gameObject.SetActive(Vector2.Distance(enemy.transform.position, player.position) < distanceFromPlayer);
					}
					
				}
				break;
		}
	}
}
