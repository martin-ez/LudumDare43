using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockBuilder
{
    public int xCoord;
    public int zCoord;
    public string block;
    public int player;
    public int variation;
    public int rotation;
    public string code;
    public string[] alias;

    public static MapBuilder Build(string json)
    {
        return JsonUtility.FromJson<MapBuilder>(json);
    }

    [Serializable]
    public class MapBuilder
    {
        public int chapter;
        public int level;
        public float time;
        public int members;
        public int money;
        public int fans;
        public string[] scrolls;
        public BlockBuilder[] blocks;
    }
}


