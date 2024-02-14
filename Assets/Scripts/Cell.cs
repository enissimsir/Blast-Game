using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string Type { get; set; }
    public List<Cell> Neighbors { get; set; }

    public Cell(int row, int col, String type)
    {
        Row = row;
        Col = col;
        Type = type;
        Neighbors = new List<Cell>();
    }
}
