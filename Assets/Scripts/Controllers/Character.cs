using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float movementTime = 0.25f;

    private bool playable = false;
    private MapBlock currentBlock = null;
    private int player;

    private bool ready;
    public Action OnReady;

    public void Init(int player, MapBlock block)
    {
        this.player = player;
        ready = false;
        currentBlock = block;
        currentBlock.OnDestroy += OnSpaceDestory;
        currentBlock.OnReady += Appear;
        transform.position = new Vector3(block.transform.position.x, 100, block.transform.position.z);
    }

    public void Appear()
    {
        currentBlock.OnReady -= Appear;
        StartCoroutine(InitAnimation());
    }

    public void Move(Vector3 direction, MapBlock block)
    {
        if (playable)
        {
            if (currentBlock != null) currentBlock.OnDestroy -= OnSpaceDestory;
            currentBlock.Leave();
            currentBlock = block;
            currentBlock.OnDestroy += OnSpaceDestory;
            Rotate(direction);
            Vector3 newPos = new Vector3(transform.position.x + direction.x * 10, 0, transform.position.z + direction.z * 10);
            StartCoroutine(MovementAnimation(newPos));
        }
    }

    public void Rotate(Vector3 direction)
    {
        Transform model = transform.Find("Model");
        int angle = 0;
        int current = (int)model.eulerAngles.y;
        if (direction == Vector3.back)
        {
            angle = 180;
        }
        else if (direction == Vector3.right)
        {
            angle = 90;
        }
        else if (direction == Vector3.left)
        {
            angle = 270;
        }
        if (angle != current) StartCoroutine(RotateAnimation(angle, model));
    }

    public bool IsPlayable()
    {
        return playable;
    }

    public int GetCoord(char axis)
    {
        if (axis == 'x')
        {
            return (int)transform.position.x / 10;
        }
        else
        {
            return (int)transform.position.z / 10;
        }
    }

    private void OnSpaceDestory()
    {
        transform.Find("Model").gameObject.SetActive(false);
        FindObjectOfType<LevelController>().OnCharacterDeath(player);
    }

    public bool IsReady()
    {
        return ready;
    }

    IEnumerator MovementAnimation(Vector3 newPos)
    {
        playable = false;
        Vector3 start = transform.position;

        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / movementTime;
            transform.position = Vector3.Lerp(start, newPos, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        transform.position = newPos;
        playable = true;
    }

    IEnumerator RotateAnimation(float angle, Transform model)
    {
        float start = model.eulerAngles.y;
        if (start > 180) start -= 360;
        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / movementTime;
            model.eulerAngles = Vector3.up * Mathf.Lerp(start, angle, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        model.eulerAngles = Vector3.up * angle;
    }

    IEnumerator InitAnimation()
    {
        playable = false;
        Vector3 start = transform.position;

        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / 1.5f;
            transform.position = Vector3.Lerp(start, currentBlock.transform.position, Easing.Ease(i, Easing.Functions.CubicEaseInOut));
            yield return null;
        }
        transform.position = currentBlock.transform.position;
        playable = true;
        if (OnReady != null) OnReady();
        ready = true;
    }
}
