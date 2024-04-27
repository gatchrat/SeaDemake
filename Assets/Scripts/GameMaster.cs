using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using JetBrains.Annotations;
public class GameMaster : MonoBehaviour {
    // Start is called before the first frame update
    private int curDay = 0;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moneyText;
    public Transform availableShipParent;
    public Transform ownedShipParent;
    public GameObject availableShipPrefab;
    private List<GameObject> availableShipsGUI = new List<GameObject>();
    void Start() {
        Company.curMoney = 10000;
        Company.ownedShips = new List<Ship>();
        Company.acceptedContracts = new List<Contract>();
        Clock.Instance.tick += Tick;
        Company.companyUiUpdate += updateAllUI;
        Company.refreshAvailableShips();
        Company.refreshAvailableContracts();
        Company.allHarbours = new List<Harbour>();
        Company.allHarbours.Add(new Harbour(new Vector2Int(0, 0)));
        Company.allHarbours.Add(new Harbour(new Vector2Int(0, 0)));
        Company.allHarbours.Add(new Harbour(new Vector2Int(0, 0)));
        availableShipPrefab.SetActive(false);
    }
    void Tick() {
        curDay++;
        if (curDay % 25 == 0) {
            Company.refreshAvailableShips();
        }
        if (curDay % 20 == 0) {
            Company.refreshAvailableContracts();
        }
        Company.Tick();
        updateAllUI();
    }
    private void updateAllUI() {
        moneyText.text = "Capital: " + Company.curMoney.ToString() + "$";
        timeText.text = "Day " + curDay.ToString();
        updateAvailableShips();

    }
    private void updateAvailableShips() {
        List<String> availableShips = new List<string>();
        foreach (Ship ship in Company.availableShips) {
            availableShips.Add(ship.name);
        }
        //check for removal
        List<String> availableShipsGUINames = new List<string>();
        for (int i = 0; i < availableShipsGUI.Count; i++) {
            GameObject item = availableShipsGUI[i];
            String name = item.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
            availableShipsGUINames.Add(name);
            if (!availableShips.Contains(name)) {
                availableShipsGUI.Remove(item);
                Destroy(item);
                i--;
            }
        }

        foreach (Ship ship in Company.availableShips) {
            if (!availableShipsGUINames.Contains(ship.name)) {
                GameObject newGUI = GameObject.Instantiate(availableShipPrefab);
                availableShipsGUI.Add(newGUI);
                newGUI.SetActive(true);
                newGUI.transform.SetParent(availableShipParent);
                newGUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ship.name;
                newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = ship.price + "$";
            }
        }
        //check for addition
    }
}
