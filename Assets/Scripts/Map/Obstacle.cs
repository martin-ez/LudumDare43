using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MapBlock
{

    public enum ObstacleType
    {
        Gate,
        Bridge
    }

    public ObstacleType type;
    private bool state;

    public Action OnChangeState;

    public override void Init()
    {
        Renderer[] rend = transform.Find("Model").GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rend.Length; i++)
        {
            rend[i].material = playerMaterials[player];
        }
        state = false;
    }

    public override bool Action(int player, bool isInteractible)
    {
        if (isInteractible) return false;
        if (!destroy && state)
        {
            return true;
        }
        FindObjectOfType<AudioManager>().PlaySound(AudioManager.Sound.No);
        return false;
    }

    public void ChangeState()
    {
        state = !state;
        if (OnChangeState != null) OnChangeState();
        StopAllCoroutines();
        StartCoroutine(StateAnimation());
    }

    IEnumerator StateAnimation()
    {
        Transform toChange = transform.Find("Model");
        Vector3 start = toChange.localPosition;
        Vector3 end = Vector3.zero;
        if (!state)
        {
            end = Vector3.up * 10;
        }

        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / 1f;
            Easing.Functions func = Easing.Functions.BounceEaseOut;
            if (!state) func = Easing.Functions.CubicEaseOut;
            toChange.localPosition = Vector3.Lerp(start, end, Easing.Ease(i, func));
            yield return null;
        }
        toChange.localPosition = end;
    }
}
