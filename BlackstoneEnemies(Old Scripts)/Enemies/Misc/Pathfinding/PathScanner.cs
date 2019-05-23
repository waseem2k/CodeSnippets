using UnityEngine;

public class PathScanner : MonoBehaviour
{
	/// <summary>
	/// This scripts only purpose is to rescan the grid map when the dimension changes
	/// </summary>
	private AstarPath pathGraph;

	private void Start()
	{
		pathGraph = GetComponent<AstarPath>();
		WorldManager.instance.OnDimensionSwap += ScanMap;
	}

	private void OnDisable()
	{
		WorldManager.instance.OnDimensionSwap -= ScanMap;
	}

	private void ScanMap(Dimension dim)
	{
		pathGraph.Scan();
	}
}
