using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Contract {
    public Harbour targetHarbour;
    public int reward;
    public int penalty; //multiple of the reward, maybe half?
    public int daysToComplete;
    public List<TypeOfGoods> toDeliverGoods = new();
    public List<TypeOfGoods> deliverdGoods = new();
    public String name = "uninitialized";
    public Sprite acceptedImage;
    public GameObject gui;
    public List<Sprite> images;
    public void SetAccepted() {
        gui.transform.GetChild(9).gameObject.GetComponent<Image>().sprite = acceptedImage;
    }
    public void Deliver(TypeOfGoods good) {
        deliverdGoods.Add(good);
        toDeliverGoods.Remove(good);
        UpdateGUI();

    }
    public void UpdateGUI() {
        Transform Inventory = gui.transform.GetChild(4);
        for (int i = 0; i < toDeliverGoods.Count; i++) {
            switch (toDeliverGoods[i]) {

                case TypeOfGoods.Coal:
                    Inventory.GetChild(i).gameObject.GetComponent<Image>().sprite = images[1];
                    break;
                case TypeOfGoods.Food:
                    Inventory.GetChild(i).gameObject.GetComponent<Image>().sprite = images[2];
                    break;
                case TypeOfGoods.Iron:
                    Inventory.GetChild(i).gameObject.GetComponent<Image>().sprite = images[3];
                    break;
                case TypeOfGoods.Medicine:
                    Inventory.GetChild(i).gameObject.GetComponent<Image>().sprite = images[4];
                    break;
                case TypeOfGoods.Wood:
                    Inventory.GetChild(i).gameObject.GetComponent<Image>().sprite = images[5];
                    break;
            }
        }
        gui.transform.GetChild(8).gameObject.GetComponent<TextMeshProUGUI>().text = daysToComplete + "";
    }
    //possibly keep deliverd goods
}
