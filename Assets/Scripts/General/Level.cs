using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeReference
{
    Empty, BasicBlue, BasicGreen, BasicRed, Box, Bomb, Rocket, BlueDisco, GreenDisco, RedDisco
}

[System.Serializable]
public class Level
{
    public int level;
    public int row;
    public int col;
    public int moves;
    public int[] shapesArray;
    public Goal[] goalsArray;
}