using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    public Material[] playerMaterials;

    public enum InteractibleType
    {
        MovingBlock,
        Instrument,
        Money,
        Fans
    }
    public InteractibleType type;

    private LevelController level;
    private MapController map;
    private MapBlock currentBlock;

    private bool destroy = false;
    private int player;
    private int value;
    private float movementTime;

    private float nextInput;
    private readonly float inputCooldown = 0.5f;

    public virtual void Init(MapBlock block, int player)
    {
        if (transform.Find("Model/Paint") != null)
        {
            Renderer[] rend = transform.Find("Model/Paint").GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rend.Length; i++)
            {
                rend[i].material = playerMaterials[player];
            }
        }
        nextInput = Time.time;
        level = FindObjectOfType<LevelController>();
        map = FindObjectOfType<MapController>();
        currentBlock = block;
        currentBlock.OnDestroy += Destroy;
        currentBlock.OnReady += Appear;
        transform.position = new Vector3(block.transform.position.x, 100, block.transform.position.z);
        block.AddInteractible(this);
        this.player = player;
        if (type == InteractibleType.Money)
        {
            value = Random.Range(2, 7);
        }
        else if (type == InteractibleType.Fans)
        {
            value = Random.Range(3, 20);
        }
    }

    public virtual bool Action(int player, bool isInteractible)
    {
        if (!destroy)
        {
            if((type == InteractibleType.Money || type == InteractibleType.Fans) && isInteractible)
            {
                Destroy();
                return true;
            }
            if (type == InteractibleType.Money)
            {
                level.CollectMoney(player, value);
                Destroy();
                return true;
            }
            else if (type == InteractibleType.Fans)
            {
                level.CollectFans(value);
                Destroy();
                return true;
            }
            else if (this.player == player)
            {
                if (type == InteractibleType.Instrument)
                {
                    level.CollectInstrument(player);
                    Destroy();
                    return true;
                }
                else if (type == InteractibleType.MovingBlock && Time.time > nextInput)
                {
                    return HandleMovingBlock();
                }
            }
        }
        return false;
    }

    private bool HandleMovingBlock()
    {
        Character current = level.GetCurrentCharacter();
        movementTime = current.movementTime;
        Vector3 diff = Vector3.Normalize(transform.position - current.transform.position);

        MapBlock block = map.GetBlock((int)Mathf.Round(transform.position.x) / 10 + (int)Mathf.Round(diff.x), (int)Mathf.Round(transform.position.z) / 10 + (int)Mathf.Round(diff.z));
        if (block != null)
        {
            if (block.Action(player, true))
            {
                if (currentBlock != null)
                {
                    currentBlock.OnDestroy -= Destroy;
                    currentBlock.RemoveInteractible();
                    currentBlock.Leave();
                }
                block.AddInteractible(this);
                block.OnDestroy += Destroy;
                currentBlock = block;
                Vector3 newPos = block.transform.position;
                StartCoroutine(MoveAnimation(newPos));
                nextInput = Time.time + inputCooldown;
                return true;
            }
        }
        FindObjectOfType<AudioManager>().PlaySound(AudioManager.Sound.No);
        return false;
    }

    public virtual void Destroy()
    {
        currentBlock.OnDestroy -= Destroy;
        destroy = true;
        currentBlock.RemoveInteractible();
        transform.Find("Model").gameObject.SetActive(false);
    }

    public virtual void Appear()
    {
        currentBlock.OnReady -= Appear;
        StartCoroutine(InitAnimation());
    }

    private void Update()
    {
        if (!destroy && (type == InteractibleType.Instrument || type == InteractibleType.Money))
        {
            if (type == InteractibleType.Instrument || type == InteractibleType.Money) transform.Rotate(Vector3.up * Time.deltaTime * 15);
        }
    }

    IEnumerator InitAnimation()
    {
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
    }

    IEnumerator MoveAnimation(Vector3 newPos)
    {
        Vector3 start = transform.position;

        float i = 0;
        float time = 0;
        while (i < 1)
        {
            time += Time.deltaTime;
            i = time / (movementTime + 0.5f);
            transform.position = Vector3.Lerp(start, newPos, Easing.Ease(i, Easing.Functions.CubicEaseOut));
            yield return null;
        }
        transform.position = newPos;
    }
}
