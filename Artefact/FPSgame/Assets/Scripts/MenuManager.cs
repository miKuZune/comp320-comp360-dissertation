using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    // Loads the scene which belongs to the name given.
	public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    // Closes the game.
    public void QuitGame()
    {
        Application.Quit();
    }
}
