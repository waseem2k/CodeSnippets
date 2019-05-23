using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FredDoor : MonoBehaviour
{
	public string bossTarget;

	private void OnCollisionEnter2D(Collision2D other)
	{
		Player player = other.transform.root.GetComponentInChildren<Player>();
		if (player)
		{
			CheckBossStatus();
		}
	}

	private void CheckBossStatus()
	{
		BossStatus status = BossManager.instance.GetBossStatus(bossTarget);
		if (status == BossStatus.Dead)
		{
			transform.DOMoveY(transform.position.y + 4f, 2f);
		}
	}
}
