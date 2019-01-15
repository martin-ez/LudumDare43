using UnityEngine;

public class Space : MapBlock
{
    public GameObject movingBlockPrefab;
    public GameObject[] instrumentPrefabs;
    public GameObject moneyPrefab;
    public GameObject fansPrefab;
    public GameObject grassPrefab;

    private bool spawnMoney;
    private bool spawnFans;
    private float nextAttempt;
    private float attemptCooldown = 5f;

    public void Update()
    {
        if (!destroy && !character && interactible == null && Time.time > nextAttempt && (spawnMoney || spawnFans))
        {
            nextAttempt = Time.time + attemptCooldown;
            attemptCooldown -= 0.1f;
            attemptCooldown = Mathf.Clamp(attemptCooldown, 2f, 5f);
            float chance = Random.value;
            if (chance < 0.0075)
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
        created.transform.SetParent(transform);
        created.transform.localPosition = Vector3.zero;
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
                if (interactible.Action(player, isInteractible))
                {
                    if (!isInteractible) character = true;
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
        if (variation == 0)
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject grass = Instantiate(grassPrefab);
                grass.transform.SetParent(transform);
                grass.transform.localScale = Random.insideUnitSphere;
                grass.transform.localPosition = new Vector3(Random.Range(-4, 4), -0.2f, Random.Range(-4, 4));
                grass.transform.eulerAngles = Random.insideUnitSphere * 180;
            }
        }
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
                toCreate = instrumentPrefabs[player];
                break;
            case 3:
                character = true;
                FindObjectOfType<LevelController>().AddCharacter(player, this);
                return;
        }
        GameObject created = Instantiate(toCreate);
        Interactible interact = created.GetComponent<Interactible>();
        interact.Init(this, player);
        nextAttempt = Time.time + attemptCooldown;
    }

    public override void Leave()
    {
        if (character) character = false;
    }
}
