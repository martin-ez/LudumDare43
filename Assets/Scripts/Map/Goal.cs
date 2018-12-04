using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MapBlock
{
    private float nextInput;
    private readonly float inputCooldown = 0.25f;

    private LevelController level;

    public override void Init()
    {
        nextInput = Time.time;
        level = FindObjectOfType<LevelController>();
    }

    public override bool Action(int player, bool isInteractible)
    {
        if (isInteractible) return false;
        if (Time.time > nextInput)
        {
            nextInput = Time.time + inputCooldown;
            return level.CharacterFinish(player);
        }
        else
        {
            return false;
        }
    }
}
