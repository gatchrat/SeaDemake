using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class Harbour {
    public String name;
    public Vector2Int pos;
    public List<(TypeOfGoods, int)> prices = new();
    public List<int> priceTargets = new();
    private readonly List<int> lastPrices = new();
    public Harbour(Vector2Int position, String s) {
        name = s;
        pos = position;
        prices.Add((TypeOfGoods.Medicine, 1000));
        InitPrices();
    }
    public int PriceOf(TypeOfGoods type) {
        foreach ((TypeOfGoods, int) item in prices) {
            if (item.Item1 == type) {
                return item.Item2;
            }
        }
        return 0;
    }
    private void InitPrices() {
        List<TypeOfGoods> allGoods = new() {
            TypeOfGoods.Food,
            TypeOfGoods.Medicine,
            TypeOfGoods.Wood,
            TypeOfGoods.Iron,
            TypeOfGoods.Coal
        };
        for (int i = 0; i < UnityEngine.Random.Range(2, 5); i++) {
            int index = UnityEngine.Random.Range(0, allGoods.Count);
            prices.Add((allGoods[index], UnityEngine.Random.Range(500, 2000)));
            allGoods.RemoveAt(index);
        }
        for (int i = 0; i < prices.Count; i++) {
            priceTargets.Add(UnityEngine.Random.Range(500, 2000));
            lastPrices.Add(prices.Last().Item2);
        }
    }
    public void UpdatePrices() {
        for (int i = 0; i < prices.Count; i++) {
            //adjust price 5% towards target, but atleast 20
            int newPrice;
            if (lastPrices[i] > priceTargets[i]) {
                newPrice = Math.Max(prices[i].Item2 + Math.Min((int)((float)(priceTargets[i] - lastPrices[i]) / UnityEngine.Random.Range(10, 20)), -3), priceTargets[i]);
            } else {
                newPrice = Math.Min(prices[i].Item2 + Math.Max((int)((float)(priceTargets[i] - lastPrices[i]) / UnityEngine.Random.Range(10, 20)), 3), priceTargets[i]);
            }
            if (prices[i].Item2 == newPrice) {
                Debug.Log(prices[i].Item2 + " " + lastPrices[i] + " " + priceTargets[i]);
            }
            prices[i] = (prices[i].Item1, newPrice);
            if (Math.Min(newPrice, priceTargets[i]) / Math.Max(newPrice, priceTargets[i]) >= 0.90) {
                priceTargets[i] = UnityEngine.Random.Range(500, 2000);
                lastPrices[i] = prices[i].Item2;
            }
        }
    }
}
