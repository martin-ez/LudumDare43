using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    public Transform model;
    public RectTransform label;
    public Text text;
    public bool onMap;
    private bool revealed;

    public void Init(int chapter, int level)
    {
        text.text = (chapter + 1) + " - " + (level + 1);
        model.gameObject.SetActive(false);
        label.gameObject.SetActive(false);
        onMap = false;
        revealed = false;
    }

    public void Reveal()
    {
        if (onMap && !revealed)
        {
            revealed = true;
            Vector3 end = new Vector3(model.localPosition.x, 0f, model.localPosition.z);
            StartCoroutine(ModelAnimation(model.localPosition, end, 0.5f));
        }
    }

    public void Hide()
    {
        if (onMap && revealed)
        {
            revealed = false;
            Vector3 end = new Vector3(model.localPosition.x, -5f, model.localPosition.z);
            StartCoroutine(ModelAnimation(model.localPosition, end, 0.3f));
        }
    }

    public void Appear()
    {
        onMap = true;
        model.gameObject.SetActive(true);
        label.gameObject.SetActive(true);
        Vector3 end = new Vector3(model.localPosition.x, -5f, model.localPosition.z);
        StartCoroutine(ModelAnimation(model.localPosition, end, 0.3f));
    }

    IEnumerator ModelAnimation(Vector3 start, Vector3 end, float duration)
    {
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / duration;
            model.localPosition = Vector3.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseOut));
            yield return null;
        }
        model.localPosition = end;
    }
}
