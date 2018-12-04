using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public GameObject SpacePrefab;

    public GameObject[] BridgeObstaclePrefabs;
    public GameObject[] GateObstaclePrefabs;

    public GameObject SwitchTriggerPrefabs;
    public GameObject ButtonTriggerPrefabs;

    public GameObject[] ConnectorPrefabs;

    private Dictionary<string, MapBlock> map;
    private Dictionary<string, string> alias;
    private Dictionary<string, Obstacle> obstacles;

    void Awake()
    {
        map = new Dictionary<string, MapBlock>();
        alias = new Dictionary<string, string>();
        obstacles = new Dictionary<string, Obstacle>();
    }

    public float CreateLevel(BlockBuilder[] blocks, bool money, bool fans)
    {
        float maxDistance = 0f;
        BlockBuilder builder = null;
        for (int i = 0; i < blocks.Length; i++)
        {
            builder = blocks[i];
            GameObject toCreate = null;
            switch (builder.block)
            {
                case "ES":
                    toCreate = SpacePrefab;
                    break;
                case "BO":
                    toCreate = BridgeObstaclePrefabs[builder.variation];
                    break;
                case "GO":
                    toCreate = GateObstaclePrefabs[builder.variation];
                    break;
                case "BT":
                    toCreate = ButtonTriggerPrefabs;
                    break;
                case "ST":
                    toCreate = SwitchTriggerPrefabs;
                    break;
                case "CO":
                    toCreate = ConnectorPrefabs[builder.variation];
                    break;
            }
            GameObject created = Instantiate(toCreate);
            Vector3 pos = new Vector3(builder.xCoord * 10, 0, builder.zCoord * 10);
            if (pos.sqrMagnitude > maxDistance)
            {
                maxDistance = pos.sqrMagnitude;
            }
            created.transform.position = pos + Vector3.up * 100;
            created.transform.Find("Model").eulerAngles = Vector3.up * builder.rotation * 90f;
            created.transform.SetParent(transform);
            MapBlock block = created.GetComponent<MapBlock>();
            block.Setup(builder.player, builder.code);
            string coordCode = builder.xCoord + ":" + builder.zCoord;
            alias.Add(coordCode, coordCode);
            map.Add(coordCode, block);

            if (builder.block == "ES")
            {
                block.GetComponent<Space>().SetUpVariation(builder.variation, money, fans);
            }
            if (builder.block == "BO" || builder.block == "GO")
            {
                obstacles.Add(builder.code, created.GetComponent<Obstacle>());
            }
            if (builder.alias.Length > 0)
            {
                for (int j = 0; j < builder.alias.Length; j++)
                {
                    alias.Add(builder.alias[j], coordCode);
                }
            }
        }

        Goal goal = new GameObject().AddComponent<Goal>();
        map.Add("0:0", goal);
        alias.Add("-1:1", "0:0");
        alias.Add("0:1", "0:0");
        alias.Add("1:1", "0:0");
        alias.Add("-1:0", "0:0");
        alias.Add("1:0", "0:0");
        alias.Add("-1:-1", "0:0");
        alias.Add("0:-1", "0:0");
        alias.Add("1:-1", "0:0");

        return maxDistance;
    }

    public void InitMap()
    {
        foreach (var block in map)
        {
            block.Value.Init();
        }
    }

    public void AppearMap()
    {
        foreach (var block in map)
        {
            block.Value.Appear();
        }
    }

    public void Destroy(float distance)
    {
        foreach (var block in map)
        {
            block.Value.Destroy(distance);
        }
    }

    public MapBlock GetBlock(int x, int z)
    {
        try
        {
            string coord = alias[x + ":" + z];
            return map[coord];
        }
        catch
        {
            return null;
        }
    }

    public Obstacle GetObstacle(string code)
    {
        return obstacles[code];
    }
}
