using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEasing : MonoBehaviour {
    float i = 0;
    bool reversed = false;
    float currentTime = 0;

    public float time;
    public float distance;
    public Easing.Functions type;

    void Update () {
        currentTime += Time.deltaTime;
        i = Mathf.Clamp01(currentTime / time);
        if (reversed)
        {
            transform.position = new Vector3(Mathf.Lerp(-distance, distance, Easing.Ease(i, type)), 5, 0);
        }
        else
        {
            transform.position = new Vector3(Mathf.Lerp(distance, -distance, Easing.Ease(i, type)), 5, 0);
        }
        if (i >= 1)
        {
            reversed = !reversed;
            currentTime = 0;
            i = 0;
        }
	}
}
