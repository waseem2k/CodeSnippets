using System.Collections.Generic;
using UnityEngine;

public class EnemyLootDrops : MonoBehaviour
{
	public List<Loot> lootTable;
	private Health health;

	private void OnEnable ()
	{
		health = GetComponent<Health>();
		health.OnDeath += DropItem;
	}

	private void OnDisable()
	{
		health.OnDeath -= DropItem;
	}

	private void DropItem()
	{
		foreach (var item in lootTable)
		{
			if (Random.value <= item.DropChance)
			{
				Instantiate(item.dropObj, transform.position, Quaternion.identity);
			}
			
		}
	}
}
