using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void Start()
    {
        FindObjectOfType<AudioManager>().TurnOnAll();
    }

    public void Play()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void Quit()
    {
        Session.QuitGame();
    }
}
