using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public SceneManager SceneManager;
    public void playButton()
    {
        SceneManager.LoadScene("Nathans spooky arena");
    }

    public void quitGame()
    {
        print("Quit");
        Application.Quit();
    }

}
