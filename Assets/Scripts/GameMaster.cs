using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
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
    public Transform pricesParent;
    public GameObject pricesPrefab;
    public Sprite emptyImage;
    public Sprite WoodImage;
    public Sprite FoodImage;
    public Sprite IronImage;
    public Sprite MedicineImage;
    public Sprite CoalImage;
    public Sprite AcceptedImage;
    private List<GameObject> contractGUI = new List<GameObject>();
    private List<GameObject> ownedShipsGUI = new List<GameObject>();
    private List<GameObject> ShipsGUI = new List<GameObject>();
    private List<GameObject> HarbourGUI = new List<GameObject>();
    private List<GameObject> availableShipsGUI = new List<GameObject>();
    private List<GameObject> pricesGUI = new List<GameObject>();
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
        updatePricesUI();
        availableShipPrefab.SetActive(false);
        ownedShipPrefab.SetActive(false);
        HarbourPrefab.SetActive(false);
        contractPrefab.SetActive(false);
        ShipPrefab.SetActive(false);
        pricesPrefab.SetActive(false);
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
                updatePricesUI();
            }
        }
        Company.Tick();
        if (HarboursToUnlock.Count > 0 && Company.curMoney > HarboursToUnlock[0].Item2) {
            Company.allHarbours.Add(HarboursToUnlock[0].Item1);
            Logger.addLog("Unlocked new Harbour " + HarboursToUnlock[0].Item1.name, Color.green);
            HarboursToUnlock.RemoveAt(0);
            regenerateHarbourUI();
            updatePricesUI();
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
        //check for addition
        foreach (Ship ship in Company.ownedShips) {
            if (!ownedShipsGUINames.Contains(ship.name)) {
                GameObject newGUI = GameObject.Instantiate(ownedShipPrefab);
                ownedShipsGUI.Add(newGUI);
                newGUI.SetActive(true);
                newGUI.transform.SetParent(ownedShipParent);
                newGUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ship.name;
                if (ship.targetDock != null) {
                    newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = "..." + ship.targetDock.name;
                }
                else {
                    newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = ship.dock.name;
                }
                newGUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = "-" + ship.runningCosts + "$";
                for (int i = 0; i < 4 - ship.inventorySize; i++) {
                    Destroy(newGUI.transform.GetChild(2).GetChild(3 - i).gameObject);
                }

            }
        }
        //update Status
        for (int i = 0; i < ownedShipsGUI.Count; i++) {
            Ship ship = Company.ownedShips[i];
            for (int x = 0; x < ship.inventorySize; x++) {
                if (x < ship.content.Count) {
                    switch (ship.content[x]) {
                        case TypeOfGoods.Wood:
                            ownedShipsGUI[i].transform.GetChild(2).GetChild(x).gameObject.GetComponent<Image>().sprite = WoodImage;
                            break;
                        case TypeOfGoods.Coal:
                            ownedShipsGUI[i].transform.GetChild(2).GetChild(x).gameObject.GetComponent<Image>().sprite = CoalImage;
                            break;
                        case TypeOfGoods.Food:
                            ownedShipsGUI[i].transform.GetChild(2).GetChild(x).gameObject.GetComponent<Image>().sprite = FoodImage;
                            break;
                        case TypeOfGoods.Iron:
                            ownedShipsGUI[i].transform.GetChild(2).GetChild(x).gameObject.GetComponent<Image>().sprite = IronImage;
                            break;
                        case TypeOfGoods.Medicine:
                            ownedShipsGUI[i].transform.GetChild(2).GetChild(x).gameObject.GetComponent<Image>().sprite = MedicineImage;
                            break;
                    }
                }
                else {
                    ownedShipsGUI[i].transform.GetChild(2).GetChild(x).gameObject.GetComponent<Image>().sprite = emptyImage;
                }
            }
            if (ship.targetDock != null) {
                ownedShipsGUI[i].transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = "..." + ship.targetDock.name;
            }
            else {
                ownedShipsGUI[i].transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = ship.dock.name;
            }
        }

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
            curHarbourUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = harbour.name;
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
            curShipUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = ship.name;
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
        //check for addition
        foreach (Ship ship in Company.availableShips) {
            if (!availableShipsGUINames.Contains(ship.name)) {
                GameObject newGUI = GameObject.Instantiate(availableShipPrefab);
                availableShipsGUI.Add(newGUI);
                newGUI.SetActive(true);
                newGUI.transform.SetParent(availableShipParent);
                newGUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ship.name;
                newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = ship.price + "$";
                newGUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = "-" + ship.runningCosts + "$";
                for (int i = 0; i < 4 - ship.inventorySize; i++) {
                    Destroy(newGUI.transform.GetChild(2).GetChild(3 - i).gameObject);
                }
            }
        }

    }
    private void updateContractUI() {
        List<String> ContractNames = new List<string>();
        foreach (Contract contract in Company.openContracts) {
            ContractNames.Add(contract.name);
        }
        foreach (Contract contract in Company.acceptedContracts) {
            ContractNames.Add(contract.name);
        }
        //check for removal
        List<String> contractUINames = new List<string>();
        for (int i = 0; i < contractGUI.Count; i++) {
            GameObject item = contractGUI[i];
            String name = item.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text;
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
                newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = contract.name;
                newGUI.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>().text = contract.targetHarbour.name;
                newGUI.transform.GetChild(6).gameObject.GetComponent<TextMeshProUGUI>().text = contract.reward + "$";
                newGUI.transform.GetChild(7).gameObject.GetComponent<TextMeshProUGUI>().text = contract.penalty + "$";
                newGUI.transform.GetChild(8).gameObject.GetComponent<TextMeshProUGUI>().text = contract.daysToComplete + "";
                contract.gui = newGUI;
                contract.acceptedImage = AcceptedImage;
                List<Sprite> ContainerSprites = new List<Sprite>();
                ContainerSprites.Add(emptyImage);
                ContainerSprites.Add(CoalImage);
                ContainerSprites.Add(FoodImage);
                ContainerSprites.Add(IronImage);
                ContainerSprites.Add(MedicineImage);
                ContainerSprites.Add(WoodImage);
                contract.images = ContainerSprites;
                contract.updateGUI();
            }
        }
        //check status
    }
    private void updatePricesUI() {
        List<String> HarbourNames = new List<string>();
        foreach (Harbour harbour in Company.allHarbours) {
            HarbourNames.Add(harbour.name);
        }
        //check for removal
        List<String> harbourUINames = new List<string>();
        for (int i = 0; i < pricesGUI.Count; i++) {
            GameObject item = pricesGUI[i];
            String name = item.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
            harbourUINames.Add(name);
        }
        //check for addition
        foreach (Harbour harbour in Company.allHarbours) {
            if (!harbourUINames.Contains(harbour.name)) {
                GameObject newGUI = GameObject.Instantiate(pricesPrefab);
                pricesGUI.Add(newGUI);
                newGUI.SetActive(true);
                newGUI.transform.SetParent(pricesParent);
                newGUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = harbour.name;
                newGUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "---";
                newGUI.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = "---";
                newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = "---";
                newGUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = "---";
                newGUI.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>().text = "---";
                foreach ((TypeOfGoods, int) item in harbour.prices) {
                    switch (item.Item1) {
                        case TypeOfGoods.Coal:
                            newGUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                            break;
                        case TypeOfGoods.Food:
                            newGUI.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                            break;
                        case TypeOfGoods.Iron:
                            newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                            break;
                        case TypeOfGoods.Medicine:
                            newGUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                            break;
                        case TypeOfGoods.Wood:
                            newGUI.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                            break;
                    }
                }

            }
            else {
                //find Objekt and set prices
                foreach (GameObject targetUI in pricesGUI) {
                    if (harbour.name == targetUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text) {
                        foreach ((TypeOfGoods, int) item in harbour.prices) {
                            switch (item.Item1) {
                                case TypeOfGoods.Coal:
                                    targetUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                                    break;
                                case TypeOfGoods.Food:
                                    targetUI.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                                    break;
                                case TypeOfGoods.Iron:
                                    targetUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                                    break;
                                case TypeOfGoods.Medicine:
                                    targetUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                                    break;
                                case TypeOfGoods.Wood:
                                    targetUI.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "$";
                                    break;
                            }
                        }
                    }
                }
            }

        }
    }
}
