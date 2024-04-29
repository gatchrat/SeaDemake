using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
public class GameMaster : MonoBehaviour {
    // Start is called before the first frame update
    private int curDay = 0;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moneyText;
    public Transform availableShipParent;
    public Transform ownedShipParent;
    public GameObject availableShipPrefab;
    public GameObject ShipPrefab;
    public GameObject ownedShipPrefab;
    public GameObject HarbourPrefab;
    public Transform HarbourParent;
    public Transform contractParent;
    public GameObject contractPrefab;
    private List<GameObject> contractGUI = new List<GameObject>();
    private List<GameObject> ownedShipsGUI = new List<GameObject>();
    private List<GameObject> ShipsGUI = new List<GameObject>();
    private List<GameObject> HarbourGUI = new List<GameObject>();
    private List<GameObject> availableShipsGUI = new List<GameObject>();
    //Assumed to be in correct order
    public List<(Harbour, int)> HarboursToUnlock = new List<(Harbour, int)>();
    void Start() {
        Company.curMoney = 10000;
        Company.ownedShips = new List<Ship>();
        Company.acceptedContracts = new List<Contract>();
        Clock.Instance.tick += Tick;
        Company.companyUiUpdate += updateAllUI;
        Company.allHarbours = new List<Harbour>();
        Company.allHarbours.Add(new Harbour(new Vector2Int(164, 56), "Hamburg"));
        Company.allHarbours.Add(new Harbour(new Vector2Int(299, 146), "Sydney"));
        HarboursToUnlock.Add((new Harbour(new Vector2Int(158, 59), "Felixstowe"), 2000));
        Company.refreshAvailableShips();
        Company.refreshAvailableContracts();
        regenerateHarbourUI();
        availableShipPrefab.SetActive(false);
        ownedShipPrefab.SetActive(false);
        HarbourPrefab.SetActive(false);
        contractPrefab.SetActive(false);
        ShipPrefab.SetActive(false);
        Logger.addLog("Game init complete", Color.gray);
    }
    void Tick() {
        curDay++;
        if (curDay % 25 == 0) {
            Company.refreshAvailableShips();
        }
        if (curDay % 20 == 0) {
            Company.refreshAvailableContracts();
        }
        if (curDay % 10 == 0) {
            foreach (Harbour harbour in Company.allHarbours) {
                harbour.updatePrices();
            }
        }
        Company.Tick();
        if (HarboursToUnlock.Count > 0 && Company.curMoney > HarboursToUnlock[0].Item2) {
            Company.allHarbours.Add(HarboursToUnlock[0].Item1);
            Logger.addLog("Unlocked new Harbour " + HarboursToUnlock[0].Item1.name, Color.green);
            HarboursToUnlock.RemoveAt(0);
            regenerateHarbourUI();
        }
        updateAllUI();
    }
    private void updateAllUI() {
        moneyText.text = "Capital: " + Company.curMoney.ToString() + "$";
        timeText.text = "Day " + curDay.ToString();
        updateAvailableShips();
        updateOwnedShips();
        updateContractUI();
        regenerateShipUI();

    }
    private void updateOwnedShips() {
        List<String> ownedShips = new List<string>();
        foreach (Ship ship in Company.ownedShips) {
            ownedShips.Add(ship.name);
        }
        //check for removal
        List<String> ownedShipsGUINames = new List<string>();
        for (int i = 0; i < ownedShipsGUI.Count; i++) {
            GameObject item = ownedShipsGUI[i];
            String name = item.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
            ownedShipsGUINames.Add(name);
            if (!ownedShips.Contains(name)) {
                ownedShipsGUI.Remove(item);
                Destroy(item);
                i--;
            }
        }

        foreach (Ship ship in Company.ownedShips) {
            if (!ownedShipsGUINames.Contains(ship.name)) {
                GameObject newGUI = GameObject.Instantiate(ownedShipPrefab);
                ownedShipsGUI.Add(newGUI);
                newGUI.SetActive(true);
                newGUI.transform.SetParent(ownedShipParent);
                newGUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ship.name;
                if (ship.dock != null) {
                    newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = ship.dock.name;
                }
                else {
                    newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = "moving";
                }

            }
        }
        //check for addition
    }
    private void regenerateHarbourUI() {
        if (HarbourGUI != null) {
            for (int i = 0; i < HarbourGUI.Count; i++) {
                Destroy(HarbourGUI[i]);
            }
        }
        HarbourGUI = new List<GameObject>();
        foreach (Harbour harbour in Company.allHarbours) {
            GameObject curHarbourUI = GameObject.Instantiate(HarbourPrefab);
            curHarbourUI.transform.SetParent(HarbourParent, false);
            curHarbourUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(3 + 4 * harbour.pos.x, -2 - 4 * harbour.pos.y, 0);
            curHarbourUI.name = harbour.name;
            curHarbourUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = harbour.name;
            curHarbourUI.SetActive(true);
            HarbourGUI.Add(curHarbourUI);
        }
    }
    private void regenerateShipUI() {
        if (ShipsGUI != null) {
            for (int i = 0; i < ShipsGUI.Count; i++) {
                Destroy(ShipsGUI[i]);
            }
        }
        ShipsGUI = new List<GameObject>();
        foreach (Ship ship in Company.ownedShips) {
            GameObject curShipUI = GameObject.Instantiate(ShipPrefab);
            curShipUI.transform.SetParent(HarbourParent, false);
            curShipUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(3 + 4 * ship.pos.x, -2 - 4 * ship.pos.y, 0);
            curShipUI.name = ship.name;
            curShipUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ship.name;
            curShipUI.SetActive(true);
            ShipsGUI.Add(curShipUI);
        }
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
    private void updateContractUI() {
        List<String> ContractNames = new List<string>();
        foreach (Contract contract in Company.openContracts) {
            Debug.Log(contract.name);
            ContractNames.Add(contract.name);
        }
        foreach (Contract contract in Company.acceptedContracts) {
            ContractNames.Add(contract.name);
        }
        //check for removal
        List<String> contractUINames = new List<string>();
        for (int i = 0; i < contractGUI.Count; i++) {
            GameObject item = contractGUI[i];
            String name = item.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
            contractUINames.Add(name);
            if (!ContractNames.Contains(name)) {
                contractGUI.Remove(item);
                Destroy(item);
                i--;
            }
        }
        //check for addition
        foreach (Contract contract in Company.openContracts) {
            if (!contractUINames.Contains(contract.name)) {
                GameObject newGUI = GameObject.Instantiate(contractPrefab);
                contractGUI.Add(newGUI);
                newGUI.SetActive(true);
                newGUI.transform.SetParent(contractParent);
                newGUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = contract.name;
                newGUI.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = contract.targetHarbour.name;
                newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = contract.reward + "$";
                newGUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = contract.daysToComplete + "";
            }
        }
        //check status

    }
}
