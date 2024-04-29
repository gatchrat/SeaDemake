using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Contract {
    public Harbour targetHarbour;
    public int reward;
    public int penalty; //multiple of the reward, maybe half?
    public int daysToComplete;
    public List<TypeOfGoods> toDeliverGoods = new List<TypeOfGoods>();
    public List<TypeOfGoods> deliverdGoods = new List<TypeOfGoods>();
    public String name = "uninitialized";
    public Sprite acceptedImage;
    public GameObject gui;
    public void setAccepted() {
        gui.transform.GetChild(9).gameObject.GetComponent<Image>().sprite = acceptedImage;
    }
}
