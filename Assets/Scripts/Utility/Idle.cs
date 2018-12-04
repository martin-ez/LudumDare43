using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : MonoBehaviour
{
    private bool up;
    private float time;
    private float current;
    private Vector3 upPos;
    private Vector3 downPos;

    private void Start()
    {
        time = 2f;
        upPos = new Vector3(transform.position.x, -1.25f, transform.position.z);
        downPos = new Vector3(transform.position.x, -1.75f, transform.position.z);
        Restart();
    }

    void Update()
    {
        current += Time.deltaTime;
        float i = current / time;
        if (up)
        {     
            transform.position = Vector3.Lerp(downPos, upPos, Easing.Ease(i, Easing.Functions.SineEaseInOut));
        }
        else
        {
            transform.position = Vector3.Lerp(upPos, downPos, Easing.Ease(i, Easing.Functions.SineEaseInOut));
        }
        if (i > 1f)
        {
            Restart();
        }
    }

    void Restart()
    {
        up = !up;
        current = 0;
        time = Random.value * 0.25f + 3f;
    }
}
