using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space : MapBlock
{
    public GameObject movingBlockPrefab;
    public GameObject instrumentPrefab;
    public GameObject moneyPrefab;
    public GameObject fansPrefab;

    private bool spawnMoney;
    private bool spawnFans;
    private float nextAttempt;

    public void Update()
    {
        if (interactible == null && Time.time > nextAttempt && (spawnMoney || spawnFans))
        {
            nextAttempt = Time.time + 2f;
            float chance = Random.value;
            if (chance < 0.15)
            {
                if (spawnMoney && !spawnFans)
                {
                    SpawnMoney();
                }
                else if (!spawnMoney && spawnFans)
                {
                    SpawnFans();
                }
                else
                {
                    chance = Random.value;
                    if (chance < 0.5)
                    {
                        SpawnMoney();
                    }
                    else
                    {
                        SpawnFans();
                    }
                }
            }
        }
    }

    private void SpawnMoney()
    {
        GameObject created = Instantiate(moneyPrefab);
        Interactible interact = created.GetComponent<Interactible>();
        interact.Init(this, player);
    }

    private void SpawnFans()
    {
        GameObject created = Instantiate(fansPrefab);
        Interactible interact = created.GetComponent<Interactible>();
        interact.Init(this, player);
    }

    public override bool Action(int player, bool isInteractible)
    {
        if (!destroy && !character)
        {
            if (interactible != null)
            {
                if (interactible.Action(player))
                {
                    character = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (!isInteractible) character = true;
                return true;
            }
        }
        else
        {
            FindObjectOfType<AudioManager>().PlaySound(AudioManager.Sound.No);
            return false;
        }
    }

    public void SetUpVariation(int variation, bool spawnMoney, bool spawnFans)
    {
        this.spawnMoney = spawnMoney;
        this.spawnFans = spawnFans;
        GameObject toCreate = null;
        switch (variation)
        {
            case 0:
                return;
            case 1:
                toCreate = movingBlockPrefab;
                break;
            case 2:
                toCreate = instrumentPrefab;
                break;
            case 3:
                character = true;
                FindObjectOfType<LevelController>().AddCharacter(player, this);
                return;
        }
        GameObject created = Instantiate(toCreate);
        Interactible interact = created.GetComponent<Interactible>();
        interact.Init(this, player);
        nextAttempt = Time.time + 2f;
    }

    public override void Leave()
    {
        if (character) character = false;
    }
}
