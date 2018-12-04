using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelBuilder
{
    public int chapter;
    public int level;
    public int time;
    public int score;
    public int money;
    public int fans;
    public Scroll[] scrolls;
    public BlockBuilder[] blocks;

    public static LevelBuilder Build(string json)
    {
        return JsonUtility.FromJson<LevelBuilder>(json);
    }

    [Serializable]
    public class Scroll
    {
        public string action;
        public string text;
    }

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
    }
}


