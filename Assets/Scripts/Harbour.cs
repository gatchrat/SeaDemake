using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Harbour {
    public String name;
    public Vector2Int pos;
    public List<(TypeOfGoods, int)> prices = new List<(TypeOfGoods, int)>();
    public List<int> priceTargets = new List<int>();
    public Harbour(Vector2Int position, String s) {
        name = s;
        pos = position;
        prices.Add((TypeOfGoods.Medicine, 1000));
        initPrices();
    }
    public int priceOf(TypeOfGoods type) {
        foreach ((TypeOfGoods, int) item in prices) {
            if (item.Item1 == type) {
                return item.Item2;
            }
        }
        return 0;
    }
    private void initPrices() {
        List<TypeOfGoods> allGoods = new List<TypeOfGoods>();
        allGoods.Add(TypeOfGoods.Food);
        allGoods.Add(TypeOfGoods.Medicine);
        allGoods.Add(TypeOfGoods.Wood);
        allGoods.Add(TypeOfGoods.Steel);
        allGoods.Add(TypeOfGoods.Coal);
        for (int i = 0; i < UnityEngine.Random.Range(2, 5); i++) {
            int index = UnityEngine.Random.Range(0, allGoods.Count);
            prices.Add((allGoods[index], UnityEngine.Random.Range(500, 2000)));
            allGoods.RemoveAt(index);
        }
        for (int i = 0; i < prices.Count; i++) {
            priceTargets.Add(UnityEngine.Random.Range(500, 2000));
        }
    }
    public void updatePrices() {
        for (int i = 0; i < prices.Count; i++) {
            //adjust price 5% towards target, but atleast 20
            int newPrice = prices[i].Item2 + (int)Math.Max((float)(priceTargets[i] - prices[i].Item2) * 0.05f, (priceTargets[i] - prices[i].Item2) / Math.Abs((priceTargets[i] - prices[i].Item2)) * 20);
            prices[i] = (prices[i].Item1, newPrice);
            if (newPrice / priceTargets[i] >= 0.95) {
                priceTargets[i] = UnityEngine.Random.Range(500, 2000);
            }
        }
    }
}
