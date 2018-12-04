using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Session
{
    public static bool newGame = true;
    public static int CurrentChapter = 0;
    public static int CurrentLevel = 0;
    public static int ChapterToLoad = 0;
    public static int LevelToLoad = 0;
    public static bool LevelCompleted = false;

    public static void QuitGame()
    {
        Debug.Log("Quiting app...");
        Application.Quit();
    }
}
