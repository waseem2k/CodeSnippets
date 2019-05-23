/// <summary>
/// Simple method for toggling between several objects
/// Here I'm using it to toggle between different menu's in a UI
/// SetActive(); Is very efficient as it checks the state first before executing
///     - This way it's not toggling panels for no reason and makes for cleaner code
/// </summary>
private void TogglePanels(GameObject panel)
{
	PauseMenu.TogglePauseMenu(panel == gamePanel);
	gamePanel.SetActive(panel == gamePanel);
	mainMenuPanel.SetActive(panel == mainMenuPanel);
	winScreenPanel.SetActive(panel == winScreenPanel);
	levelSelectPanel.SetActive(panel == levelSelectPanel);
	creditsPanel.SetActive(panel == creditsPanel);
}

/// <summary>
/// Gets the level number value from scene name
/// Eg. We check "Level36" with the value "Level", it returns 36
/// </summary>
public static int SetCurrentScene(Scene scene, string val) // Sets the current scene index
{
    if (scene.name.Contains(val))
    {
        string n = scene.name.Remove(0, val.length);
        int i;
        int.TryParse(n, out i);
        return i;
    }
    return 0; // Defaults to zero if a number hasn't been found
}
