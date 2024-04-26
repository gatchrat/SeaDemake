using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship {
    public String name;
    public int inventorySize = 3;
    public List<Goods> content;
    private int standardPrice = 10000;
    public int runningCosts = 5;
    public int price = 0;
    public Vector2Int pos;
    public Harbour dock;
    public bool isMoving() {
        return dock == null;
    }
    public Ship() {
        name = UnityEngine.Random.Range(0, 100) + "";
        price = standardPrice * ((UnityEngine.Random.Range(0, 100) - 50) / 50);
        Debug.Log("ship" + name);
    }
}
