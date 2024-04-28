using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contract {
    public Harbour targetHarbour;
    public int reward;
    public int penalty;
    public int daysToComplete;
    public List<Goods> toDeliverGoods = new List<Goods>();
    public List<Goods> deliverdGoods = new List<Goods>();
    public String name;
}
