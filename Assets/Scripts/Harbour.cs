using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Harbour {
    public String name;
    public Vector2Int pos;
    public List<(TypeOfGoods, int)> prices = new List<(TypeOfGoods, int)>();
    public Harbour(Vector2Int position) {
        name = UnityEngine.Random.Range(0, 100) + "";
        pos = position;
        Debug.Log("harbour" + name);
    }
    public Harbour(Vector2Int position, String s) {
        name = s;
        pos = position;
        prices.Add((TypeOfGoods.Medicine, 1000));
    }
    public int priceOf(TypeOfGoods type) {
        foreach ((TypeOfGoods, int) item in prices) {
            if (item.Item1 == type) {
                return item.Item2;
            }
        }
        return 0;
    }
}
