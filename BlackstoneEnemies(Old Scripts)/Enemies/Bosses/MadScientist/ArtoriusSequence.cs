using UnityEngine;

public class ArtoriusSequence : MonoBehaviour
{
	public static ArtoriusSequence instance;

	[SerializeField] private BossCollapsingFloor[] collapsingPlatform;
	private int platformIndex;

	private void Awake()
	{
		instance = this;
	}

	public void DropPlatforms()
	{
		if (platformIndex >= collapsingPlatform.Length) return;

		collapsingPlatform[platformIndex].CollapseFloors();
		platformIndex++;
	}

}
