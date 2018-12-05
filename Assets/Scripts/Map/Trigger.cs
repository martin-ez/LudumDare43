using System.Collections;
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
        base.Init();
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
            StartCoroutine(ActionAnimation());
            nextInput = Time.time + inputCooldown;
        }
        return (type == TriggerType.Button && this.player == player);
    }

    public override void Leave()
    {
        if (type == TriggerType.Button) obstacle.ChangeState();
        StartCoroutine(ActionAnimation());
    }

    private void ObstacleDestroy()
    {
        base.Destroy(0);
    }

    private void Update()
    {
        if (obstacle.GetState() && type == TriggerType.Switch)
        {
            transform.Find("Model/Paint/Actioner").Rotate(Vector3.up * Time.deltaTime * 100);
        }
    }

    IEnumerator ActionAnimation()
    {
        Transform actioner = transform.Find("Model/Paint/Actioner");
        float i = 0;
        float time = 0;
        Vector3 start = actioner.localPosition;
        if (type == TriggerType.Button)
        {         
            Vector3 end = new Vector3(0, 1, 0);
            if (obstacle.GetState()) end = new Vector3(0, -0.75f, 0);
            while (i < 1)
            {
                time += Time.deltaTime;
                i = time / 0.75f;
                actioner.localPosition = Vector3.Lerp(start, end, Easing.Ease(i, Easing.Functions.CubicEaseOut));
                yield return null;
            }
        }        
    }
}
