using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public RectTransform gameOverPanel;
    public RectTransform pausePanel;
    public RectTransform resumePanel;
    public RectTransform metricsPanel;

    private RectTransform[] playersPanel;

    public void GameOver(bool success)
    {
        gameOverPanel.Find("Panel/Yes").gameObject.SetActive(success);
        gameOverPanel.Find("Panel/No").gameObject.SetActive(!success);

        StartCoroutine(MovePanel(gameOverPanel, 0f, 0.5f, 0f));
    }

    public void Pause(bool on)
    {
        StartCoroutine(MovePanel(pausePanel, on ? 0f : -2000f, 0.3f, 0f));
    }

    public void LevelResume(int chapter, int level, int time, int members, int money, int fans)
    {
        resumePanel.Find("Panel/Title").GetComponent<Text>().text = "Level " + (chapter + 1) + "-" + (level + 1);
        resumePanel.Find("Panel/Goals").GetComponent<Text>().text = (time > 0 ? "<b>Time limit:</b> " + (int)time / 60 + ":" + (time % 60 < 10 ? "0" : "") + (int)time % 60 + "\n" : "")
            + "<b>Min.Members:</b> " + members + "\n"
            + (money > 0 ? (chapter == 2 ? "<b>Show Price:</b>" : "<b>Rehearsal Price:</b> ") + members + "\n" : "")
            + (fans > 0 ? "<b>Fans to reach:</b> " + members : "");

        StartCoroutine(MovePanel(resumePanel, 0f, 1f, 0f));
    }

    public void HideResume()
    {
        StartCoroutine(MovePanel(resumePanel, 2000f, 0.5f, 0f));
    }

    public void SetupMetrics(int players, bool instrument, int money, int fans)
    {
        playersPanel = new RectTransform[] { metricsPanel.Find("Drummy").GetComponent<RectTransform>(), metricsPanel.Find("Bassy").GetComponent<RectTransform>(), metricsPanel.Find("Keysy").GetComponent<RectTransform>() };
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
        SetActiveCharacter(0);
    }

    public void SetActiveCharacter(int character)
    {
        for (int i = 0; i < 3; i++)
        {
            playersPanel[i].Find("Active").gameObject.SetActive(character == i);
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
        metricsPanel.Find("Fans/Image").localScale = new Vector3(Mathf.Clamp01(total / min), 1, 1);
    }

    public void KillCharacter(int character)
    {
        playersPanel[character].Find("Dead").gameObject.SetActive(true);
    }

    IEnumerator MovePanel(RectTransform panel, float end, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        float start = panel.localPosition.y;
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / duration;
            panel.localPosition = Vector3.up * Mathf.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        panel.localPosition = Vector3.up * end;
    }
}
