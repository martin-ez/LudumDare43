using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MapBlock
{
    public enum TriggerType
    {
        Switch,
        Button
    }

    public TriggerType type;

    private Obstacle obstacle;

    private float nextInput;
    private readonly float inputCooldown = 1.25f;

    public override void Init()
    {
        nextInput = Time.time;
        obstacle = map.GetObstacle(code);
        obstacle.OnDestroy += ObstacleDestroy;
    }

    public override void Destroy(float threshold)
    {
        return;
    }

    public override bool Action(int player, bool isInteractible)
    {
        if (interactible != null) return interactible.Action(player);
        if (!destroy && this.player == player && (type == TriggerType.Button || Time.time > nextInput))
        {
            obstacle.ChangeState();
            FindObjectOfType<AudioManager>().PlaySound(AudioManager.Sound.Action);
            nextInput = Time.time + inputCooldown;
        }
        return (type == TriggerType.Button && this.player == player);
    }

    public override void Leave()
    {
        if (type == TriggerType.Button) obstacle.ChangeState();
    }

    private void ObstacleDestroy()
    {
        base.Destroy(0);
    }
}
