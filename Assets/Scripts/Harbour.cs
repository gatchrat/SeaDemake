using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Harbour {
    public String name;
    public Vector2Int pos;
    public (Goods, int) prices;
    public Harbour(Vector2Int position) {
        name = UnityEngine.Random.Range(0, 100) + "";
        pos = position;
        Debug.Log("harbour" + name);
    }
}
