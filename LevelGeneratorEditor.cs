using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using UnityEditor.SceneManagement;

/// <summary>
/// The level generator editor, This is where all the magic happens
/// Along with drawing the UI for the generator, it also handles the logic for each action
/// </summary>
[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
	private LevelGenerator lg;

	private bool resetEverything;
	private int selGridInt; // Index for handleSelected object in manual mode grid
 	private Texture[] contents; // Textures for each spawnable object
 	private string[] contentNames; // The GameObject names of each prefab in contents

	// Textures and style for buttons
	private Texture2D greenTex;
	private Texture2D redTex;
	private Texture2D whiteTex;
	private GUIStyle style;

	//
	private int handleIndex; // Count the number of handles
	private bool handleSelected; // Toggle used for selecting a handle

	private int selectedHandleIndex; // The index of the handleSelected handle
	private bool objectSelected; // If an object is handleSelected in the scene
	private Texture selectedTexture; // The texture of the handleSelected object
	private Vector3 startPos; // The start position before starting to drag

	// Makes a 1px texture for specified colour since we can't set the background colour of a button directly
	private static Texture2D MakeTex(Color col)
	{
		Color[] pix = new Color[1];

		for (int i = 0; i < pix.Length; i++)
			pix[i] = col;

		Texture2D result = new Texture2D(1, 1);
		result.SetPixels(pix);
		result.Apply();

		return result;
	}

	// Find texture from Texture array using the names array
	private Texture FindTextureFromContents(string cName)
	{
		for (int i = 0; i < contentNames.Length; i++)
		{
			if (contentNames[i].Contains(cName))
				return contents[i];
		}
		return contents[0];
	}

	private void Awake()
	{
		lg = target as LevelGenerator; // Init

		// Make button textures
		greenTex = MakeTex(Color.green);
		redTex = MakeTex(Color.red);
		whiteTex = MakeTex(Color.white);

		// Create style
		style = new GUIStyle
		{
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter,
			fontSize = 18,
			normal = { textColor = Color.white }
		};

		// Set up texture names and generate textures from the asset preview
		if (lg != null)
		{
			int obstaclesLength = lg.obstacleList.Obstacles.Length;

			contents = new Texture[obstaclesLength + 3];
			contentNames = new string[obstaclesLength + 3];
			for (int i = 0; i < contents.Length; i++)
			{
				GameObject ob = null;
				if (i < obstaclesLength)
				{
					ob = lg.obstacleList.Obstacles[i];
				}
				else
				{
					if (i == obstaclesLength)
					{
						ob = lg.obstacleList.Fire;
					}
					if (i == obstaclesLength + 1)
					{
						ob = lg.obstacleList.Dog;
					}
					if (i == obstaclesLength + 2)
					{
						ob = lg.obstacleList.Exit;
					}
				}

				if (ob != null)
				{
					contents[i] = AssetPreview.GetAssetPreview(ob);
					contentNames[i] = ob.name;
				}
			}
		}
	}

	public override void OnInspectorGUI()
	{
		if(lg == null) lg = target as LevelGenerator;

		base.OnInspectorGUI();
		GUILayout.Space(10f);

		lg.manualMode = GUILayout.Toggle(lg.manualMode, "Enable Manual Mode");

		GUILayout.Space(10f);

		if (!lg.manualMode)
		{
			if (!lg.routineStarted) // Draw generate buttons if not currently generating
				DrawLevelGenerationButtons();
			else // Draw stop button if generating to exit the loop
				DrawStopGeneratingButton();
		}
		else // Draw the manual mode grid
		{
			DrawManualModeGrid();
		}


		if (objectSelected) // Draw selected object so we can remove objects from scene
		{
			DrawSelectedObject();
		}

		GUILayout.Space(10f);

		if (lg.levelCache != null && !lg.routineStarted) // Enable loading and saving if a cache object is in the slot
		{
			lg.saveLoad = GUILayout.Toggle(lg.saveLoad, "Enable Saving and Loading");
			if(lg.saveLoad) DrawLevelCache(); // Draw Cache saving and loading buttons if saving and loading is enabled
		}

		// Emergency reset if things break, two step process for confirmation
		resetEverything = GUILayout.Toggle(resetEverything, "Reset Everything");

		if (resetEverything) // Draw reset button if resetEverything is enabled
		{
			DrawResetButton();
		}

		// Show internal stats to check if the correct amount of objects are in the scene
		lg.showStats = GUILayout.Toggle(lg.showStats, "Show Stats");

		if (lg.showStats)
		{
			DrawStats();
		}
	}

	private void OnSceneGUI()
	{
		if (lg == null) lg = target as LevelGenerator;

		// Draw handles around each spawned object to enable dragging
		if (lg != null && lg.SpawnedObjectsCount > 0)
		{
			handleIndex = 1;
			for (int i = 0; i < lg.obstacleGridList.Count; i++)
			{
				DrawHandle(lg.obstacleGridList[i], lg.data.Obstacles[i], Color.gray, handleIndex);
				handleIndex++;
			}

			for (int i = 0; i < lg.fireGridList.Count; i++)
			{
				DrawHandle(lg.fireGridList[i], lg.data.Fires[i], Color.blue, handleIndex);
				handleIndex++;
			}

			if (lg.dogGrid != null)
			{
				DrawHandle(lg.dogGrid, lg.data.Dog, Color.green, handleIndex);

			}
			handleIndex++;

			if (lg.exitGrid != null)
			{
				DrawHandle(lg.exitGrid, lg.data.Exit, Color.cyan, handleIndex);
			}
			handleIndex++;

			DetectClicks();
		}
	}

	// Manual Mode Grid
	private void DrawManualModeGrid()
	{
		GUILayout.BeginVertical("Box");
		style.normal.background = whiteTex;
		style.onNormal.background = redTex;
		style.hover.background = greenTex;
		selGridInt = GUILayout.SelectionGrid(selGridInt, contents, 4, style, GUILayout.Width(600f), GUILayout.Height(600f));
		style.normal.background = greenTex;

		// Button to spawn selected object because drawing a grid of buttons is a pain
		if (GUILayout.Button("Spawn: " + contentNames[selGridInt], style, GUILayout.Height(50f)))
		{
			SpawnObstacleType(selGridInt);
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		style.hover.background = redTex;
		style.normal.background = redTex;

		// Clear Grid button, just with a fancy texture
		if (GUILayout.Button("Clear Entire Grid", style, GUILayout.Height(50f)))
		{
			ClearEntireGrid();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		GUILayout.EndVertical();
	}

	// Selected Object
	private void DrawSelectedObject()
	{
		// Horrible horrible way to layout a simple texture with label
		GUILayout.Space(10f);
		GUILayout.Label("Selected Object");
		GUILayout.BeginVertical("Box");
		GameObject go = GetSelectedObject(selectedHandleIndex);

		style.normal.background = redTex;
		style.normal.textColor = Color.black;

		GUILayout.BeginHorizontal(); // Because a simple option to center everything is hard to do...
		GUILayout.FlexibleSpace();
		GUILayout.Label(go.name, style);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();


		style.normal.textColor = Color.white;

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(FindTextureFromContents(go.name));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// Button to remove the selected object
		if (GUILayout.Button("Remove Selected", style, GUILayout.Height(50f)))
		{
			ClearSelectedObject(selectedHandleIndex);
			objectSelected = false;
		}

		GUILayout.EndVertical();
	}

	// Auto Generate
	private void DrawLevelGenerationButtons()
	{
		// Have to mark the scene dirty after each action. THe coroutines do it at the end of the routine
		if (lg.SpawnedObjectsCount > 0) lg.clearExisting = GUILayout.Toggle(lg.clearExisting, "Clear Existing Prefabs");

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Generate Obstacles")) // Start an editor coroutine to generate objects over time rather then all at once
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(GenerateObstacles());
		}

		if (GUILayout.Button("Clear Obstacles")) // Clear all spawned obstacles
		{
			ClearObstacles();
			objectSelected = false;
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Generate Fire")) // Same as the Generate Obstacle button but for Fires
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(GenerateFires());
		}

		if (GUILayout.Button("Clear Fires")) // Clear all spawned Fires
		{
			ClearFires();
			objectSelected = false;
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Generate Dog")) // Generate dog in a random position
		{
			SpawnDog();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		if (GUILayout.Button("Clear Dog")) // Just in case we need to get rid of the dog, also to keep it uniform
		{
			ClearDog();
			objectSelected = false;
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Generate Exit")) // Generate Exit in a random position
		{
			SpawnExit();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		if (GUILayout.Button("Clear Exit")) // Just in case we need to get rid of the exit
		{
			ClearExit();
			objectSelected = false;
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(10f);

		if (GUILayout.Button("Clear Entire Grid")) // Destroy everything, even the puppy
		{
			ClearEntireGrid();
			objectSelected = false;
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
	}

	// Button to stop the coroutines
	private void DrawStopGeneratingButton()
	{
		if (GUILayout.Button("Stop Generating"))
		{
			lg.routineStarted = false;
		}
	}

	// Saving and Loading buttons, similar to generate buttons but no coroutines
	// because obstacles already have a set position
	private void DrawLevelCache()
	{
		GUILayout.Space(10f);
		GUILayout.Label("Save / Load Data");

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Load Fires")) // Load all fires from the cache file
		{
			LoadFiresFromCache();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		if (GUILayout.Button("Load Obstacles")) // Load all obstacles from the cache file
		{
			LoadObstaclesFromCache();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Load Dog")) // Load the puppy from the cache file
		{
			LoadDogFromCache();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		if (GUILayout.Button("Load Exit")) // Load the exit from the cache file
		{
			LoadExitFromCache();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(10f);
		if (GUILayout.Button("Load All Cache")) // Load everything from the cache file
		{
			LoadEntireCache();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		if (lg.SpawnedObjectsCount > 0)
		{
			GUILayout.Space(10f);
			if (GUILayout.Button("Save to Cache")) // Save everything to the cache file and mark it dirty so the collab can pick it up as changed like wtf?
			{
				SaveLevelToCache();
				EditorUtility.SetDirty(lg.levelCache);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}
	}

	// Emergency Reset Button
	private void DrawResetButton()
	{
		GUILayout.Label("WARNING THIS WILL RESET EVERYTHING BACK TO DEFAULT VALUES");

		if (GUILayout.Button("Reset Everything"))
		{
			resetEverything = false;
			lg.showStats = false;
			lg.ClearEverything();
		}
	}

	// Draw stats so we can track hidden variables without designers breaking them
	private void DrawStats()
	{
		GUILayout.Space(10f);
		GUILayout.Label("Hidden Stats");
		GUILayout.Label("Spawned Object: " + lg.SpawnedObjectsCount);
		GUILayout.Label("Objects in Obstacle Grid List: " + lg.obstacleGridList.Count + " | " + lg.data.Obstacles.Count);
		GUILayout.Label("Objects in Fire Grid List: " + lg.fireGridList.Count + " | " + lg.data.Fires.Count);
		GUILayout.Label("Dog is Set: " + (lg.dogGrid != null) + " | " + lg.data.Dog.set);
		GUILayout.Label("Exit is Set: " + (lg.exitGrid != null) + " | " + lg.data.Exit.set);
		GUILayout.Label("Routine Started: " + lg.routineStarted);
	}

	// Draws a dragable handle in the scene view and detect if it has moved
	private void DrawHandle(GridCheck g, GeneratedObject ge, Color color, int hIndex)
	{

		EditorGUI.BeginChangeCheck();

		// Draw a separate handle to visualize that this object has been selected
		if (objectSelected && hIndex == selectedHandleIndex + 1)
		{
			Handles.color = Color.red;
			Handles.FreeMoveHandle(9999, g.transform.position, Quaternion.identity, 0.55f, Vector3.one, Handles.CircleHandleCap);
		}

		Handles.color = color; // Se the colour of the actual handle
		startPos = g.transform.position; // Set the starting position before the handle starts moving

		// Draw the actual handle and save it's position in a variable
		//Vector3 dPos = Handles.FreeMoveHandle(hIndex, g.transform.position, Quaternion.identity, 0.45f, Vector3.one, Handles.CircleHandleCap);
		Vector3 dPos = Handles.FreeMoveHandle(hIndex, g.transform.position, Quaternion.identity, 0.225f, Vector3.one, Handles.DotHandleCap);

		if (dPos != startPos) // Check if this handle has moved and set the scene to dirty if it has,
		{ // Because even though it's moving a gameobject the scene can't detect a chance so it doesn't save...
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		g.transform.position = dPos; // Set the new position have to set this twice because of the way obstacles are set up
		dPos = g.GetPosition(lg.grid); // Get the nearest grid position of the set position
		g.transform.position = dPos; // Set the new position on the grid
		ge.position = dPos; // Set the position for the generator to track and save

		EditorGUI.EndChangeCheck();
	}

	// Detect if a handle has been clicked
	private void DetectClicks()
	{
		if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != 9999 && !handleSelected)
		{
			Debug.Log(GUIUtility.hotControl);
			if (HandleUtility.nearestControl == GUIUtility.hotControl) // The same
			{
				selectedHandleIndex = HandleUtility.nearestControl - 1;
				objectSelected = true;
				Repaint();
			}
			handleSelected = true;
		}

		if (GUIUtility.hotControl == 0 && handleSelected)
		{
			handleSelected = false;
		}
	}

	// Spawn selected obstacle from manual selection grid
	private void SpawnObstacleType(int index)
	{
		int obLength = lg.obstacleList.Obstacles.Length;
		// Some funky checks to determine what kind of object is selected
		// so we can do it all in one grid
		if (index < obLength)
		{
			SpawnObstacle(index);
		}
		else
		{
			if (index == obLength)
			{
				SpawnFire();
			}
			if (index == obLength + 1)
			{
				SpawnDog();
			}
			if (index == obLength + 2)
			{
				SpawnExit();
			}
		}
	}

	// Coroutine for generating obstacles
	private IEnumerator GenerateObstacles()
	{
		int tempObstacleIndex = lg.NumObstaclesToSpawn;
		lg.routineStarted = true;
		if (lg.clearExisting) ClearObstacles();
		while (lg.NumObstaclesToSpawn > 0)
		{
			if (lg.routineStarted == false)
			{
				lg.NumObstaclesToSpawn = tempObstacleIndex;
				yield break;
			}

			SpawnRandomObstacle();
			yield return new EditorWaitForSeconds(0.5f);
		}
		lg.routineStarted = false;
		lg.NumObstaclesToSpawn = tempObstacleIndex;
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
	}

	// Coroutine for generating fires
	private IEnumerator GenerateFires()
	{
		int tempFireIndex = lg.NumFiresToSpawn;
		lg.routineStarted = true;
		if(lg.clearExisting) ClearFires();
		while (lg.NumFiresToSpawn > 0)
		{
			if (lg.routineStarted == false)
			{
				lg.NumFiresToSpawn = tempFireIndex;
				yield break;
			}

			SpawnFire();
			yield return new EditorWaitForSeconds(0.5f);
		}
		lg.routineStarted = false;
		lg.NumFiresToSpawn = tempFireIndex;
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
	}

	#region CreateObstacles


	public bool SpawnRandomObstacle() // Spawns a random obstacle
	{
		int i = UnityEngine.Random.Range(0, lg.obstacleList.Obstacles.Length);

		return SpawnObstacle(i);
	}

	public bool SpawnObstacle(int i) // Spawns specified obstacle
	{
		GridCheck gridComp = SpawnObject(lg.obstacleList.Obstacles[i], lg.obstacleParent);

		if (gridComp != null)
		{
			lg.obstacleGridList.Add(gridComp);
			lg.data.Obstacles.Add(new GeneratedObject(i, gridComp.transform.position));
			lg.NumObstaclesToSpawn--;
			return true;
		}
		return false;
	}

	public bool SpawnFire() // Spawns a fire
	{
		GridCheck gridComp = SpawnObject(lg.obstacleList.Fire, lg.fireParent);

		if (gridComp != null)
		{
			lg.fireGridList.Add(gridComp);
			lg.data.Fires.Add(new GeneratedObject(0, gridComp.transform.position));
			lg.NumFiresToSpawn--;
			return true;
		}
		return false;
	}

	public bool SpawnDog() // Spawns the dog, deletes any existing dogs in the scene
	{
		GridCheck gc = SpawnObject(lg.obstacleList.Dog, lg.genericParent);

		if (gc != null)
		{
			if (lg.dogGrid != null)
			{
				DestroyImmediate(lg.dogGrid.gameObject);
			}

			lg.dogGrid = gc;
			lg.data.Dog = new GeneratedObject(0, gc.transform.position);
			return true;
		}
		return false;
	}

	public bool SpawnExit() // Spawns the exit, deletes any existing exits in the scene
	{
		GridCheck gc = SpawnObject(lg.obstacleList.Exit, lg.genericParent);

		if (gc != null)
		{
			if (lg.exitGrid != null)
			{
				DestroyImmediate(lg.exitGrid.gameObject);
			}

			lg.exitGrid = gc;
			lg.data.Exit = new GeneratedObject(0, gc.transform.position);
			return true;
		}
		return false;
	}

	// Spawns selected object in a random position and parents it accordingly
	private GridCheck SpawnObject(GameObject objToSpawn, Transform parent)
	{
		GridCheck gridCheck = null;
		int attemptCount = 10;

		while (attemptCount > 0) // Limit the number of attempts so we don't hang
		{
			GameObject go = PrefabUtility.InstantiatePrefab(objToSpawn) as GameObject;
			if (go != null)
			{
				go.transform.position = lg.RandomPosition;
				go.transform.rotation = Quaternion.identity;

				GridCheck gc = go.GetComponent<GridCheck>();

				go.transform.position = gc.GetPosition(GridManager.Grid);


				if (!gc.CanPlace()) // Checks if the object can be placed in the specified location, destroys if it can't
				{
					DestroyImmediate(go);
					attemptCount--; // Minus attempt count so we can break out if the object can't find a position
					continue;
				}

				gridCheck = gc;
				go.transform.SetParent(parent);
				go.name = objToSpawn.name;
				lg.SpawnedObjectsCount++;
			}
			break;
		}

		return gridCheck;
	}

	#endregion

	#region ClearGridObjects

	public void ClearSelectedObject(int index) // More funky math to determine which object is selected
	{
		int newIndex = index;
		int fireIndex = lg.obstacleGridList.Count + lg.fireGridList.Count;
		if (newIndex < lg.obstacleGridList.Count) // Is Obstacle
		{
			lg.data.Obstacles.RemoveAt(newIndex);
			DestroyImmediate(lg.obstacleGridList[newIndex].gameObject);
			lg.obstacleGridList.RemoveAt(newIndex);
			MinusSpawnedObjectCount();
		}

		else if (newIndex < fireIndex)
		{
			newIndex -= lg.obstacleGridList.Count;
			lg.data.Fires.RemoveAt(newIndex);
			DestroyImmediate(lg.fireGridList[newIndex].gameObject);
			lg.fireGridList.RemoveAt(newIndex);
			MinusSpawnedObjectCount();
		}

		else if (newIndex == fireIndex) // Tis Dog
		{
			ClearDog();
		}

		else if (newIndex == fireIndex + 1) // Tis Exit
		{
			ClearExit();
		}
	}

	public GameObject GetSelectedObject(int index) // Get a GameObject from selected grid
	{
		int newIndex = index;
		int fireIndex = lg.obstacleGridList.Count + lg.fireGridList.Count;
		if (newIndex < lg.obstacleGridList.Count) // Is Obstacle
		{
			return lg.obstacleGridList[newIndex].gameObject;
		}
		if (newIndex < fireIndex)
		{
			newIndex -= lg.obstacleGridList.Count;
			return lg.fireGridList[newIndex].gameObject;
		}
		if (newIndex == fireIndex) // Tis Dog
		{
			return lg.dogGrid.gameObject;
		}

		if (newIndex == fireIndex + 1) // Tis Exit
		{
			return lg.exitGrid.gameObject;
		}

		Debug.LogWarning("Nothing found");
		return null;
	}

	public void ClearObstacles() // Clear all Obstacles
	{
		lg.data.Obstacles.Clear();
		foreach (GridCheck g in lg.obstacleGridList)
		{
			DestroyImmediate(g.gameObject);
			MinusSpawnedObjectCount();
		}
		lg.obstacleGridList.Clear();

	}

	public void ClearFires() // Clear all fires
	{
		lg.data.Fires.Clear();
		foreach (GridCheck g in lg.fireGridList)
		{
			DestroyImmediate(g.gameObject);
			MinusSpawnedObjectCount();
		}
		lg.fireGridList.Clear();

	}

	public void ClearDog() // Clear the dog
	{
		lg.data.Dog = new GeneratedObject(0, Vector3.zero);
		if (lg.dogGrid != null)
		{
			DestroyImmediate(lg.dogGrid.gameObject);
			lg.dogGrid = null;
			MinusSpawnedObjectCount();
		}
	}

	public void ClearExit() // Clear the exit
	{
		lg.data.Exit = new GeneratedObject(0, Vector3.zero);
		if (lg.exitGrid != null)
		{
			DestroyImmediate(lg.exitGrid.gameObject);
			lg.exitGrid = null;
			MinusSpawnedObjectCount();
		}
	}

	public void ClearEntireGrid() // Clear everything
	{
		lg.data = new GeneratedData();
		ClearObstacles();
		ClearFires();
		ClearDog();
		ClearExit();
	}

	private void MinusSpawnedObjectCount() // Checks to make sure Spawned objects never go under 0
	{
		lg.SpawnedObjectsCount--;
		if (lg.SpawnedObjectsCount < 0) lg.SpawnedObjectsCount = 0;
	}
	#endregion

	#region Caching
	public void SaveLevelToCache()
	{
		lg.levelCache.Data = new GeneratedData();

		GeneratedData newData = new GeneratedData
		{
			Obstacles = lg.data.Obstacles,
			Fires = lg.data.Fires,
			Dog = lg.data.Dog,
			Exit = lg.data.Exit
		};
		lg.levelCache.Data = newData;
	}

	public void LoadEntireCache()
	{
		lg.data = new GeneratedData();
		LoadObstaclesFromCache();
		LoadFiresFromCache();
		LoadDogFromCache();
		LoadExitFromCache();
	}

	public void LoadObstaclesFromCache()
	{
		if (lg.levelCache.Data.Obstacles.Count > 0 && lg.levelCache.Data.Obstacles[0].set)
		{
			ClearObstacles();
			lg.data.Obstacles = new List<GeneratedObject>();
			foreach (GeneratedObject o in lg.levelCache.Data.Obstacles)
			{
				GameObject go = PrefabUtility.InstantiatePrefab(lg.obstacleList.Obstacles[o.prefabIndex]) as GameObject;

				Vector3 pos = o.position;
				pos.z = 0;
				if (go != null)
				{
					go.transform.position = pos;
					go.transform.rotation = Quaternion.identity;
					go.transform.SetParent(lg.obstacleParent);
					go.name = lg.obstacleList.Obstacles[o.prefabIndex].name;

					lg.obstacleGridList.Add(go.GetComponent<GridCheck>());

					GeneratedObject ge = new GeneratedObject(o.prefabIndex, o.position);
					lg.data.Obstacles.Add(ge);
					lg.SpawnedObjectsCount++;
				}

			}
		}
	}

	public void LoadFiresFromCache()
	{
		if (lg.levelCache.Data.Fires.Count > 0 && lg.levelCache.Data.Fires[0].set)
		{
			ClearFires();
			lg.data.Fires = new List<GeneratedObject>();
			foreach (GeneratedObject o in lg.levelCache.Data.Fires)
			{
				GameObject go = PrefabUtility.InstantiatePrefab(lg.obstacleList.Fire) as GameObject;

				Vector3 pos = o.position;
				pos.z = 0;
				if (go != null)
				{
					go.transform.position = pos;
					go.transform.rotation = Quaternion.identity;
					go.transform.SetParent(lg.fireParent);
					go.name = lg.obstacleList.Fire.name;

					lg.fireGridList.Add(go.GetComponent<GridCheck>());

					GeneratedObject ge = new GeneratedObject(o.prefabIndex, o.position);
					lg.data.Fires.Add(ge);
					lg.SpawnedObjectsCount++;
				}

			}
		}
	}

	public void LoadDogFromCache()
	{
		if (lg.levelCache.Data.Dog.set)
		{
			ClearDog();
			lg.data.Dog = new GeneratedObject(0, lg.levelCache.Data.Dog.position);
			GameObject go = PrefabUtility.InstantiatePrefab(lg.obstacleList.Dog) as GameObject;

			Vector3 pos = lg.data.Dog.position;
			pos.z = 0;
			if (go != null)
			{
				go.transform.position = pos;
				go.transform.rotation = Quaternion.identity;
				go.transform.SetParent(lg.genericParent);
				go.name = lg.obstacleList.Dog.name;
				lg.dogGrid = go.GetComponent<GridCheck>();
				lg.SpawnedObjectsCount++;
			}
		}
	}

	public void LoadExitFromCache()
	{
		if (lg.levelCache.Data.Exit.set)
		{
			ClearExit();
			lg.data.Exit = new GeneratedObject(0, lg.levelCache.Data.Exit.position);
			GameObject go = PrefabUtility.InstantiatePrefab(lg.obstacleList.Exit) as GameObject;

			Vector3 pos = lg.data.Exit.position;
			pos.z = 0;
			if (go != null)
			{
				go.transform.position = pos;
				go.transform.rotation = Quaternion.identity;
				go.transform.SetParent(lg.genericParent);
				go.name = lg.obstacleList.Exit.name;

				lg.exitGrid = go.GetComponent<GridCheck>();
				lg.SpawnedObjectsCount++;
			}
		}
	}
	#endregion
}
