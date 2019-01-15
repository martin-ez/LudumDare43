using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public Transform[] chapters;
    public Transform cam;
    public GameObject levelPrefab;
    public RectTransform finishLabel;
    public RectTransform instructions;

    private Level[] levels;
    private int currentChapter;
    private int currentLevel;

    private int chapterSelect;
    private int levelSelect;

    private float nextInput;

    private bool endGame = false;

    private void Start()
    {
        bool wasNewGame = Session.newGame;
        if (Session.newGame)
        {
            Session.newGame = false;
            Session.CurrentChapter = 0;
            Session.CurrentLevel = 0;

            for (int i = 0; i < chapters.Length; i++)
            {
                chapters[i].eulerAngles = Vector3.forward * 180;
            }
            StartCoroutine(RevealChapter(0, 2f, 0.5f));
            StartCoroutine(CenterCamera(0, 0, 0.75f, 2.75f, true));
            nextInput = Time.time + 4f;
        }
        else
        {
            for (int i = 0; i < chapters.Length; i++)
            {
                if (Session.CurrentChapter >= i)
                {
                    chapters[i].eulerAngles = Vector3.zero;
                }
                else
                {
                    chapters[i].eulerAngles = Vector3.forward * 180;
                }
            }
            cam.position = chapters[Session.ChapterToLoad].position + Vector3.forward * 30 + Vector3.right * (-80 + 40 * Session.LevelToLoad);
            nextInput = Time.time + 0.5f;
        }
        currentChapter = Session.CurrentChapter;
        currentLevel = Session.CurrentLevel;
        chapterSelect = Session.ChapterToLoad;
        levelSelect = Session.LevelToLoad;
        CreateMap();
        bool needsReveal = false;
        bool chapterReveal = false;
        if (chapterSelect == currentChapter && levelSelect == currentLevel && Session.LevelCompleted)
        {
            currentLevel++;
            //TODO Remove this
            if (currentLevel == 2 && currentChapter == 1)
            {
                StartCoroutine(CenterCamera(1, 2, 1.5f, 1f, false));
                StartCoroutine(FinishGame(0f, 2.5f));
                endGame = true;
            }
            else if (currentLevel == 5)
            {
                currentLevel = 0;
                currentChapter++;
                if (currentChapter == 3)
                {
                    currentChapter = 2;
                    currentLevel = 4;
                    if (!Session.EndGame)
                    {
                        chapterSelect = 1;
                        levelSelect = 2;
                        StartCoroutine(CenterCamera(1, 2, 1.5f, 0.5f, false));
                        StartCoroutine(FinishGame(0f, 1.5f));
                        endGame = true;
                        nextInput = Time.time + 3f;
                        Session.EndGame = true;
                    }
                }
                else
                {
                    StartCoroutine(CenterCamera(currentChapter, 2, 1f, 0.5f, false));
                    StartCoroutine(RevealChapter(currentChapter, 2, 2f));
                    StartCoroutine(CenterCamera(currentChapter, 0, 1f, 4.5f, true));
                    nextInput = Time.time + 6f;
                    FindObjectOfType<AudioManager>().ChangeSong(currentChapter);
                    chapterReveal = true;
                }
            }
            else
            {
                needsReveal = true;
            }
            Session.CurrentChapter = currentChapter;
            Session.CurrentLevel = currentLevel;
            if (!endGame)
            {
                levelSelect = currentLevel;
                chapterSelect = currentChapter;
            }
        }
        if (!wasNewGame)
        {
            for (int i = 0; i < (currentChapter * 5 + currentLevel) + 1; i++)
            {
                if (!(chapterReveal && i == (currentChapter * 5 + currentLevel)))
                {
                    levels[i].Appear();
                }
            }
            if (needsReveal)
            {
                StartCoroutine(CenterCamera(currentChapter, currentLevel, 1f, 0.5f, true));
                nextInput = Time.time + 2f;
            }
            else if (!chapterReveal && !endGame)
            {
                levels[chapterSelect * 5 + levelSelect].Reveal();
            }
        }
        if (!chapterReveal) FindObjectOfType<AudioManager>().TurnOnAll();
    }

    private void LateUpdate()
    {
        if (Time.time > nextInput)
        {
            if (Input.GetKey(KeyCode.D) && !endGame)
            {
                NextLevel();
            }
            else if (Input.GetKey(KeyCode.A) && !endGame)
            {
                PreviousLevel();
            }
            else if (Input.GetKey(KeyCode.W) && !endGame)
            {
                PreviousChapter();
            }
            else if (Input.GetKey(KeyCode.S) && !endGame)
            {
                NextChapter();
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                if (endGame)
                {
                    StartCoroutine(FinishGame(2000, 0f));
                    StartCoroutine(CenterCamera(1, 2, 0.1f, 0f, true));
                    nextInput = Time.time + 0.25f;
                    endGame = false;
                }
                else
                {
                    Session.ChapterToLoad = chapterSelect;
                    Session.LevelToLoad = levelSelect;
                    Session.LevelRestarted = !(chapterSelect == currentChapter && levelSelect == currentLevel);
                    SceneManager.LoadScene("Level");
                }

            }
            if (Input.GetKey(KeyCode.Escape))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private void CreateMap()
    {
        levels = new Level[15];
        GameObject level = null;
        for (int i = 0; i < 15; i++)
        {
            level = Instantiate(levelPrefab);
            level.transform.SetParent(chapters[i / 5]);
            level.transform.localEulerAngles = Vector3.zero;
            level.transform.localPosition = Vector3.right * (-80 + i % 5 * 40);
            levels[i] = level.GetComponent<Level>();
            levels[i].Init(i / 5, i % 5);
        }
    }

    private void NextLevel()
    {
        int lastChapter = chapterSelect;
        int lastLevel = levelSelect;
        if (currentChapter > chapterSelect || (currentChapter == chapterSelect && currentLevel > levelSelect))
        {
            levelSelect++;
            if (levelSelect == 5 && chapterSelect != 2 && currentChapter > chapterSelect)
            {
                levelSelect = 0;
                chapterSelect++;
                PerformChange(lastChapter, lastLevel);
            }
            else if (levelSelect == 5)
            {
                levelSelect = 4;
            }
            else
            {
                PerformChange(lastChapter, lastLevel);
            }
        }
    }

    private void PreviousLevel()
    {
        int lastChapter = chapterSelect;
        int lastLevel = levelSelect;
        levelSelect--;
        if (levelSelect == -1 && chapterSelect != 0)
        {
            levelSelect = 4;
            chapterSelect--;
            PerformChange(lastChapter, lastLevel);
        }
        else if (levelSelect == -1)
        {
            levelSelect = 0;
        }
        else
        {
            PerformChange(lastChapter, lastLevel);
        }
    }

    private void NextChapter()
    {
        int lastChapter = chapterSelect;
        int lastLevel = levelSelect;
        if (chapterSelect != 2 && currentChapter > chapterSelect && currentLevel >= levelSelect)
        {
            chapterSelect++;
            PerformChange(lastChapter, lastLevel);
        }
    }

    private void PreviousChapter()
    {
        int lastChapter = chapterSelect;
        int lastLevel = levelSelect;
        if (chapterSelect != 0)
        {
            chapterSelect--;
            PerformChange(lastChapter, lastLevel);
        }
    }

    void PerformChange(int lastChapter, int lastLevel)
    {
        levels[lastChapter * 5 + lastLevel].Hide();
        StartCoroutine(CenterCamera(chapterSelect, levelSelect, 0.5f, 0f, true));
        nextInput = Time.time + 0.75f;
    }

    IEnumerator RevealChapter(int chapter, float animationTime, float delay)
    {
        yield return new WaitForSeconds(delay);
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / animationTime;
            chapters[chapter].eulerAngles = Vector3.right * Mathf.Lerp(180, 0, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        chapters[chapter].eulerAngles = Vector3.zero;
        levels[5 * chapter].Appear();
    }

    IEnumerator CenterCamera(int chapter, int level, float animationTime, float delay, bool withReveal)
    {
        yield return new WaitForSeconds(delay);
        Vector3 start = cam.position;
        Vector3 end = chapters[chapter].position + Vector3.forward * 30 + Vector3.right * (-80 + 40 * level);
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / animationTime;
            cam.position = Vector3.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseOut));
            yield return null;
        }
        cam.position = end;
        if (withReveal)
        {
            levels[5 * chapter + level].Reveal();
            instructions.gameObject.SetActive(true);
        }
    }

    IEnumerator FinishGame(float end, float delay)
    {
        yield return new WaitForSeconds(delay);
        float start = finishLabel.localPosition.y;
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / 1f;
            finishLabel.localPosition = Vector3.up * Mathf.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        finishLabel.localPosition = Vector3.up * end;
    }
}
