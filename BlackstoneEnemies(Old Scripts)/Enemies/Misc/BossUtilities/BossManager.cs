using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
	public static BossManager instance;

	public bool BossCombatState { get; private set; }
	public event Action<bool> EnteringBossCombat;

	private void Awake()
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

	public List<BossInfo> bossInfo;

	public BossInfo GetBossInfo(string _name)
	{
		return bossInfo.FirstOrDefault(b => b.bossName == _name);
	}

	public BossStatus GetBossStatus(string _name)
	{
		foreach (BossInfo b in bossInfo)
		{
			if (b.bossName == _name) return b.status;
		}
		return BossStatus.Dead;
	}

	public void SetStatus(string _name, BossStatus _status)
	{
		foreach (BossInfo b in bossInfo)
		{
			if (b.bossName != _name) continue;
			b.status = _status;
			return;
		}
	}

	public int GetBossesKilled()
	{
		return bossInfo.Count(b => b.status == BossStatus.Dead);
	}

	public void SetCombatState(bool _inCombat)
	{
		BossCombatState = _inCombat;
		if (EnteringBossCombat != null) EnteringBossCombat(_inCombat);
	}

	public void PickupKey(string _name, string _keyId) // Call when the key is being picked up
	{
		foreach (var b in bossInfo)
		{
			if (b.bossName != _name) continue;

			b.keyAquired = true;
			b.keyId = _keyId;
			SaveManager.instance.SaveGame(); //TODO: Replace with quick save
			return;
		}
	}

	public void UnlockDoor(string _name)
	{
		foreach (var b in bossInfo)
		{
			if(b.bossName != _name) continue;
			if (b.doorUnlocked) return;

			b.doorUnlocked = true;
			ItemsArea.instance.RemoveItem("key", b.keyId);
			SaveManager.instance.SaveGame(); // TODO: Replace with quick save
			return;
		}
	}
}

[Serializable]
public class BossInfo
{
	public string bossName;
	public BossStatus status;
	public Dimension activeDimension;
	public Sprite keySprite;
	public bool keyAquired; // Set to true when the key is picked up
	public string keyId;
	public Sprite doorSprite;
	public bool doorUnlocked; // Set to true when the door is unlocked
}

public enum BossStatus { Alive, Dead }
