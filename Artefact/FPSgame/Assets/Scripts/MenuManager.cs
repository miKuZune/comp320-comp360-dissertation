using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    Image loadingImg;

    public void Start()
    {
        loadingImg = GameObject.Find("LoadingImg").GetComponent<Image>();
        loadingImg.enabled = false;
    }

    // Loads the scene which belongs to the name given.
    public void LoadScene(string sceneName)
    {
        // Randomly choose if the ML should be enabled or not.
        System.Random rand = new System.Random((int)(Time.time * 1000));

        int enableML = rand.Next(0, 2);
        PlayerPrefs.SetInt("enableML", enableML);
        Debug.Log("Player pref set to: " + PlayerPrefs.GetInt("enableML"));
        loadingImg.enabled = true;

        SceneManager.LoadScene(sceneName);
    }
    public void NextSession()
    {
        loadingImg.enabled = true;

        Debug.Log("before " + PlayerPrefs.GetInt("enableML"));
        int enableML = 0;
        if (PlayerPrefs.GetInt("enableML") == 0) { enableML = 1; Debug.Log("Set enableML to " + enableML); }
        
        PlayerPrefs.SetInt("enableML", enableML);
        Debug.Log("after " + PlayerPrefs.GetInt("enableML"));

        SceneManager.LoadScene("SampleScene");
    }
    // Closes the game.
    public void QuitGame()
    {
        Application.Quit();
    }
}
