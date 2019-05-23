using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Level generator was made to quickly generate and edit levels without having to go through the hierarchy
/// It has two modes, Auto and Manual.
/// Auto Mode: Generate a set number of objects randomly inside the grid area
///		- There are checks in place to make sure obstacles don't overlap while spawning
///		- This limits the amount of obstacles that can be spawned overall
/// Manual Mode: Select which object to spawn.
///		- Currently it spawns the selected object in a random position
///		- We can also select spawned objects and destroy them
/// Note: Only contains data that needs to persist, most of the logic is in the editor script
/// </summary>
public class LevelGenerator : MonoBehaviour
{
	public Grid grid; // Reference to the grid object

	[Header("Obstacle Prefab List")]
	public ObstacleList obstacleList; // List of spawnable objects

	[Header("Parent Objects")] // Set the parent for each type of spawned object
	public Transform genericParent;
	public Transform obstacleParent;
	public Transform fireParent;

	[Header("Level Cache Target")]
	public GeneratedLevel levelCache; // For saving a backup of the data

/*	[Header("GridSize")]
	public int width = 10;
	public int height = 10;*/

	// Auto Generation
	[Header("The number of prefabs to spawn")]
	public int NumObstaclesToSpawn = 10;
	public int NumFiresToSpawn = 5;

	// These are hidden because the Editor needs access but shouldn't be changed by anyone else
	[HideInInspector] public List<GridCheck> obstacleGridList = new List<GridCheck>();
	[HideInInspector] public List<GridCheck> fireGridList = new List<GridCheck>();
	[HideInInspector] public GridCheck dogGrid;
	[HideInInspector] public GridCheck exitGrid;

	[HideInInspector] public GeneratedData data = new GeneratedData();

	[HideInInspector] public int SpawnedObjectsCount; // The number of objects currently in the lists, represents the number of spawned obstacles
	[HideInInspector] public bool routineStarted; // Set to false to stop auto generating obstacles

	[HideInInspector] public bool manualMode; // Enable Manual mode
	[HideInInspector] public bool clearExisting = true;
	[HideInInspector] public bool saveLoad; // Enable Saving and Loading to Cache
	[HideInInspector] public bool showStats; // Show stats for debugging

	// Emergency reset
	public void ClearEverything()
	{
		obstacleGridList = new List<GridCheck>();
		fireGridList = new List<GridCheck>();
		dogGrid = null;
		exitGrid = null;

		SpawnedObjectsCount = 0;
		routineStarted = false;

		NumObstaclesToSpawn = 10;
		NumFiresToSpawn = 5;

		data = new GeneratedData();
	}

	// Get a random position on the grid
	public Vector3 RandomPosition
	{
		get
		{
			int X = UnityEngine.Random.Range(0, 10);
			int Y = UnityEngine.Random.Range(0, 10);

			return grid.GetCellCenterLocal(new Vector3Int(X, Y, 0));
		}
	}
}

// Store data for everything being spawned by the generator
[Serializable]
public class GeneratedData
{
	// Dog
	public GeneratedObject Dog;
	// Exit
	public GeneratedObject Exit;

	// Obstacles
	public List<GeneratedObject> Obstacles = new List<GeneratedObject>();

	// Fires
	public List<GeneratedObject> Fires = new List<GeneratedObject>();
}

// Data for spawned objects
[Serializable]
public class GeneratedObject
{
	public Vector3 position;
	public int prefabIndex;
	public bool set;

	public GeneratedObject(int index, Vector3 pos)
	{
		prefabIndex = index;
		position = pos;
		set = true;
	}
}
