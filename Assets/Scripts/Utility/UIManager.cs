using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Color failColor;
    public Color successColor;

    public RectTransform gameOverPanel;
    public RectTransform pausePanel;
    public RectTransform scrollPanel;
    public RectTransform resumePanel;
    public RectTransform metricsPanel;
    public RectTransform scorePanel;

    private RectTransform[] playersPanel;

    private bool OnScrollAnimation = false;

    public void GameOver(bool success, int score, int minScore, int fans, int minFans)
    {
        gameOverPanel.Find("Panel/Yes").gameObject.SetActive(success);
        gameOverPanel.Find("Panel/No").gameObject.SetActive(!success);

        StartCoroutine(MovePanel(gameOverPanel, 0f, 0.5f, 0f));
        StartCoroutine(GameOverStats(score, minScore, fans, minFans, success, 0.5f));
    }

    public void Pause(bool on)
    {
        StartCoroutine(MovePanel(pausePanel, on ? 0f : -2000f, 0.3f, 0f));
    }

    public void ShowScroll(string text)
    {
        Text textComponent = scrollPanel.Find("Panel/Text").GetComponent<Text>();
        StartCoroutine(MovePanel(scrollPanel, 0f, 0.5f, 0f));
        StartCoroutine(ScrollTextAnimation(textComponent, text, 0.5f));
    }

    public void SkipScroll(string text)
    {
        StopAllCoroutines();
        scrollPanel.Find("Panel/Text").GetComponent<Text>().text = text;
        OnScrollAnimation = false;
    }

    public void HideScroll()
    {
        StartCoroutine(MovePanel(scrollPanel, 2000f, 0.5f, 0f));
    }

    public bool OnScroll()
    {
        return OnScrollAnimation;
    }

    public void LevelResume(int chapter, int level, int time, int score, int money, int fans)
    {
        resumePanel.Find("Panel/Title").GetComponent<Text>().text = "Level " + (chapter + 1) + "-" + (level + 1);
        resumePanel.Find("Panel/Goals").GetComponent<Text>().text = "<b>Score:</b> " + score + "\n"
            + (time > 0 ? "<b>Time limit:</b> " + (int)time / 60 + ":" + (time % 60 < 10 ? "0" : "") + (int)time % 60 + "\n" : "")
            + (money > 0 ? (chapter == 2 ? "<b>Show Price:</b>" : "<b>Rehearsal Price:</b> ") + money + "\n" : "")
            + (fans > 0 ? "<b>Fans to reach:</b> " + fans : "");

        StartCoroutine(MovePanel(resumePanel, 0f, 1f, 0f));
    }

    public void HideResume()
    {
        StartCoroutine(MovePanel(resumePanel, 2000f, 0.5f, 0f));
    }

    public void SetupMetrics(int players, bool instrument, int time, int money, int fans, string place)
    {
        playersPanel = new RectTransform[] { metricsPanel.Find("Drummy").GetComponent<RectTransform>(), metricsPanel.Find("Bassy").GetComponent<RectTransform>(), metricsPanel.Find("Keysy").GetComponent<RectTransform>() };
        if (time > 0)
        {
            metricsPanel.Find("Time").gameObject.SetActive(true);
            metricsPanel.Find("Time/Text").GetComponent<Text>().text = (int)time / 60 + ":" + (time % 60 < 10 ? "0" : "") + (int)time % 60;
            metricsPanel.Find("Time/Label").GetComponent<Text>().text = place;
            metricsPanel.Find("Fans/Image").localScale = Vector3.one;
        }
        else
        {
            metricsPanel.Find("Time").gameObject.SetActive(false);
        }
        if (fans > 0)
        {
            metricsPanel.Find("Fans").gameObject.SetActive(true);
            metricsPanel.Find("Fans/Text").GetComponent<Text>().text = "00/" + fans;
        }
        else
        {
            metricsPanel.Find("Fans").gameObject.SetActive(false);
        }
        for (int i = 0; i < players; i++)
        {
            playersPanel[i].gameObject.SetActive(true);
            if (instrument)
            {
                playersPanel[i].Find("Instrument").gameObject.SetActive(true);
            }
            else
            {
                playersPanel[i].Find("Instrument").gameObject.SetActive(false);
            }
            if (money > 0)
            {
                playersPanel[i].Find("Money").gameObject.SetActive(true);
                playersPanel[i].Find("Money/Text").GetComponent<Text>().text = "00/" + money;
            }
            else
            {
                playersPanel[i].Find("Money").gameObject.SetActive(false);
            }
        }
        if (players == 1)
        {
            playersPanel[0].anchoredPosition = Vector3.right * -110;
        }
        else if (players == 2)
        {
            playersPanel[0].anchoredPosition = Vector3.right * -320;
            playersPanel[1].anchoredPosition = Vector3.right * -110;
        }
        else if (players == 3)
        {
            playersPanel[0].anchoredPosition = Vector3.right * -530;
            playersPanel[1].anchoredPosition = Vector3.right * -320;
            playersPanel[2].anchoredPosition = Vector3.right * -110;
        }
        scorePanel.gameObject.SetActive(true);
        scorePanel.Find("Text").GetComponent<Text>().text = "000";
        SetActiveCharacter(0);
    }

    public void SetActiveCharacter(int character)
    {
        if (playersPanel != null)
        {
            for (int i = 0; i < 3; i++)
            {
                playersPanel[i].Find("Active").gameObject.SetActive(character == i);
            }
        }
    }

    public void AddMoney(int character, float total, float min)
    {
        playersPanel[character].Find("Money/Text").GetComponent<Text>().text = total + "/" + min;
        playersPanel[character].Find("Money/Image").localScale = new Vector3(Mathf.Clamp01(total / min), 1, 1);
    }

    public void AddInstrument(int character)
    {
        playersPanel[character].Find("Instrument/Image").localScale = new Vector3(1, 1, 1);
    }

    public void AddFans(float total, float min)
    {
        metricsPanel.Find("Fans/Text").GetComponent<Text>().text = total + "/" + min;
        metricsPanel.Find("Fans/Image").localScale = new Vector3(Mathf.Clamp01((float)total / (float)min), 1, 1);
    }

    public void ChangeTimer(int left, int total)
    {
        metricsPanel.Find("Time/Text").GetComponent<Text>().text = (int)left / 60 + ":" + (left % 60 < 10 ? "0" : "") + (int)left % 60;
        metricsPanel.Find("Time/Image").localScale = Vector3.one - Vector3.right * (1 - Mathf.Clamp01((float)left / (float)total));
    }

    public void ChangeScore(int score)
    {
        scorePanel.Find("Text").GetComponent<Text>().text = (score < 10 ? "00" : score < 100 ? "0" : "") + score;
    }

    public void KillCharacter(int character)
    {
        playersPanel[character].Find("Dead").gameObject.SetActive(true);
    }

    IEnumerator MovePanel(RectTransform panel, float end, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        float start = panel.anchoredPosition.y;
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / duration;
            panel.anchoredPosition = Vector3.up * Mathf.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        panel.anchoredPosition = Vector3.up * end;
    }

    IEnumerator GameOverStats(int score, int minScore, int fans, int minFans, bool success, float delay)
    {
        Text statText = gameOverPanel.Find("Panel/Stats").GetComponent<Text>();
        statText.text = "<b>Score:</b> " + "000 / " + minScore + "\n"
            + (fans > 0 ? "<b>Fans:</b> " + "000 / " + minFans + "\n" : "");
        yield return new WaitForSeconds(delay);

        float statDuration = score * 0.01f;
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / statDuration;
            int currScore = (int)Mathf.Lerp(0, score, Easing.Ease(i, Easing.Functions.CubicEaseOut));
            int currFans = (int)Mathf.Lerp(0, fans, Easing.Ease(i, Easing.Functions.CubicEaseOut));
            statText.text = "<b>Score:</b> " + (currScore < 10 ? "00" : currScore < 100 ? "0" : "") + currScore + " / " + minScore + "\n"
            + (fans > 0 ? "<b>Fans:</b> " + (currFans < 10 ? "00" : currFans < 100 ? "0" : "") + currFans + " / " + minFans + "\n" : "");
            yield return null;
        }
        statText.text = "<b>Score:</b> " + (score < 10 ? "00" : score < 100 ? "0" : "") + score + " / " + minScore + "\n"
            + (fans > 0 ? "<b>Fans:</b> " + (fans < 10 ? "00" : fans < 100 ? "0" : "") + fans + " / " + minFans + "\n" : "");
        if (success)
        {
            statText.color = successColor;
        }
        else
        {
            statText.color = failColor;
        }
    }

    IEnumerator ScrollTextAnimation(Text component, string text, float delay)
    {
        OnScrollAnimation = true;
        component.text = "";
        yield return new WaitForSeconds(delay);
        float duration = text.Length * 0.025f;
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / duration;
            int currentLength = (int)Mathf.Lerp(0, text.Length, i);
            component.text = text.Substring(0, currentLength);
            yield return null;
        }
        component.text = text;
        OnScrollAnimation = false;
    }
}
