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
    public UILineRenderer LR;
    public Transform pricesParent;
    public GameObject pricesPrefab;
    public Sprite emptyImage;
    public Sprite WoodImage;
    public Sprite FoodImage;
    public Sprite IronImage;
    public Sprite MedicineImage;
    public Sprite CoalImage;
    public Sprite AcceptedImage;
    public List<AudioClip> audioClips;
    private readonly List<GameObject> contractGUI = new();
    private readonly List<GameObject> ownedShipsGUI = new();
    private List<GameObject> ShipsGUI = new();
    private List<GameObject> HarbourGUI = new();
    private readonly List<GameObject> availableShipsGUI = new();
    private readonly List<GameObject> pricesGUI = new();
    private readonly List<int> moneyHistory = new();
    private int maxMoney;
    public static GameMaster Instance;
    //Assumed to be in correct order
    public List<(Harbour, int)> HarboursToUnlock;
    void Start() {
        Instance = this;
        Company.curMoney = 10000;
        moneyHistory.Add(Company.curMoney);
        maxMoney = Company.curMoney;
        Company.ownedShips = new List<Ship>();
        Company.acceptedContracts = new List<Contract>();
        Clock.Instance.Tick += Tick;
        Company.CompanyUiUpdate += UpdateAllUI;
        Company.allHarbours = new List<Harbour> {
            new(new Vector2Int(164, 56), "Hamburg"),
            new(new Vector2Int(156, 73), "Valencia")
        };
        HarboursToUnlock = new List<(Harbour, int)> {
            (new Harbour(new Vector2Int(299, 146), "Sydney"), 12000),
            (new Harbour(new Vector2Int(98, 67), "Halifax"), 15000),
            (new Harbour(new Vector2Int(82, 105), "Balboa"), 20000),
            (new Harbour(new Vector2Int(39, 65), "Vancouver"), 25000),
            (new Harbour(new Vector2Int(321, 148), "Auckland"), 35000),
            (new Harbour(new Vector2Int(271, 83), "Shanghai"), 50000),
            (new Harbour(new Vector2Int(231, 106), "Colombo"), 75000),
            (new Harbour(new Vector2Int(157, 107), "Lagos"), 100000),
            (new Harbour(new Vector2Int(193, 93), "Jeddah"), 150000),
            (new Harbour(new Vector2Int(111, 137), "Santos"), 200000),
            (new Harbour(new Vector2Int(187, 138), "Durban"), 250000),
            (new Harbour(new Vector2Int(106, 40), "Nuuk"), 500000)
        };

        foreach ((Harbour, int) harbour in HarboursToUnlock) {
            Company.lockedHarbours.Add(harbour.Item1);
        }


        //add "tutorial" ship and contracts
        Contract tutorialContract1 = new() {
            daysToComplete = 100,
            name = "0",
            reward = 5000,
            penalty = 10,
            targetHarbour = Company.allHarbours[0]
        };
        tutorialContract1.toDeliverGoods.Add(TypeOfGoods.Food);
        Company.openContracts.Add(tutorialContract1);

        Contract tutorialContract2 = new() {
            daysToComplete = 100,
            name = "1",
            reward = 5000,
            penalty = 10,
            targetHarbour = Company.allHarbours[1]
        };
        tutorialContract2.toDeliverGoods.Add(TypeOfGoods.Medicine);
        Company.openContracts.Add(tutorialContract2);

        Ship tutorialShip = new() {
            inventorySize = 1,
            name = "Starter",
            price = 6500,
            runningCosts = 1
        };
        Company.availableShips.Add(tutorialShip);

        Company.RefreshAvailableShips();
        Company.RefreshAvailableContracts();
        RegenerateHarbourUI();
        UpdatePricesUI();
        availableShipPrefab.SetActive(false);
        ownedShipPrefab.SetActive(false);
        HarbourPrefab.SetActive(false);
        contractPrefab.SetActive(false);
        ShipPrefab.SetActive(false);
        pricesPrefab.SetActive(false);
        Logger.AddLog("Game init complete", Color.gray);
    }
    void Tick() {
        curDay++;
        if (curDay % 25 == 0) {
            Company.RefreshAvailableShips();
        }
        if (curDay % 20 == 0) {
            Company.RefreshAvailableContracts();
        }
        if (curDay % 10 == 0) {
            foreach (Harbour harbour in Company.allHarbours) {
                harbour.UpdatePrices();
                UpdatePricesUI();
            }
        }
        Company.Tick();
        if (HarboursToUnlock.Count > 0 && Company.curMoney > HarboursToUnlock[0].Item2) {
            Company.allHarbours.Add(HarboursToUnlock[0].Item1);
            Logger.AddLog("Unlocked new Harbour " + HarboursToUnlock[0].Item1.name, Color.green);
            GameMaster.Instance.PlayAudio(AudioClipType.harbour);
            HarboursToUnlock.RemoveAt(0);
            RegenerateHarbourUI();
            UpdatePricesUI();
        }
        UpdateAllUI();
    }
    private void UpdateAllUI() {

        timeText.text = curDay.ToString();
        UpdateAvailableShips();
        UpdateOwnedShips();
        UpdateContractUI();
        RegenerateShipUI();
        UpdateMoneyUI();
    }
    private void UpdateMoneyUI() {
        moneyHistory.Add(Company.curMoney);
        if (Company.curMoney > maxMoney) {
            maxMoney = Company.curMoney;
        }
        List<Vector2> points = new();
        float maxX = 1344;
        float maxY = 100;
        float width = maxX / (moneyHistory.Count - 1);
        float curX = 0;
        for (int i = 0; i < moneyHistory.Count; i++) {
            points.Add(new Vector2(curX, -8 - maxY + (float)moneyHistory[i] / (float)maxMoney * maxY));
            curX += width;
        }
        LR.points = points;
        LR.SetAllDirty();
        moneyText.text = Company.curMoney.ToString() + "$";
        moneyText.transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(LR.points[LR.points.Count - 1].x - 45, LR.points[LR.points.Count - 1].y, 0);
    }
    private void UpdateOwnedShips() {
        List<String> ownedShips = new();
        foreach (Ship ship in Company.ownedShips) {
            ownedShips.Add(ship.name);
        }
        //check for removal
        List<String> ownedShipsGUINames = new();
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
                } else {
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
                } else {
                    ownedShipsGUI[i].transform.GetChild(2).GetChild(x).gameObject.GetComponent<Image>().sprite = emptyImage;
                }
            }
            if (ship.targetDock != null) {
                ownedShipsGUI[i].transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = "..." + ship.targetDock.name;
            } else {
                ownedShipsGUI[i].transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = ship.dock.name;
            }
        }

    }
    private void RegenerateHarbourUI() {
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
    private void RegenerateShipUI() {
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
    private void UpdateAvailableShips() {
        List<String> availableShips = new();
        foreach (Ship ship in Company.availableShips) {
            availableShips.Add(ship.name);
        }
        //check for removal
        List<String> availableShipsGUINames = new();
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
    private void UpdateContractUI() {
        List<String> ContractNames = new();
        foreach (Contract contract in Company.openContracts) {
            ContractNames.Add(contract.name);
        }
        foreach (Contract contract in Company.acceptedContracts) {
            ContractNames.Add(contract.name);
        }
        //check for removal
        List<String> contractUINames = new();
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
                List<Sprite> ContainerSprites = new() {
                    emptyImage,
                    CoalImage,
                    FoodImage,
                    IronImage,
                    MedicineImage,
                    WoodImage
                };
                contract.images = ContainerSprites;
                contract.UpdateGUI();
            }
        }
        //check status
    }
    private void UpdatePricesUI() {
        List<String> HarbourNames = new();
        foreach (Harbour harbour in Company.allHarbours) {
            HarbourNames.Add(harbour.name);
        }
        //check for removal
        List<String> harbourUINames = new();
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
                            newGUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                            break;
                        case TypeOfGoods.Food:
                            newGUI.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                            break;
                        case TypeOfGoods.Iron:
                            newGUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                            break;
                        case TypeOfGoods.Medicine:
                            newGUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                            break;
                        case TypeOfGoods.Wood:
                            newGUI.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                            break;
                    }
                }

            } else {
                //find Objekt and set prices
                foreach (GameObject targetUI in pricesGUI) {
                    if (harbour.name == targetUI.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text) {
                        foreach ((TypeOfGoods, int) item in harbour.prices) {
                            switch (item.Item1) {
                                case TypeOfGoods.Coal:
                                    targetUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                                    break;
                                case TypeOfGoods.Food:
                                    targetUI.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                                    break;
                                case TypeOfGoods.Iron:
                                    targetUI.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                                    break;
                                case TypeOfGoods.Medicine:
                                    targetUI.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                                    break;
                                case TypeOfGoods.Wood:
                                    targetUI.transform.GetChild(5).gameObject.GetComponent<TextMeshProUGUI>().text = item.Item2 + "";
                                    break;
                            }
                        }
                    }
                }
            }

        }
    }
    public void PlayAudio(AudioClipType adc) {
        switch (adc) {
            case AudioClipType.scribble:
                GetComponent<AudioSource>().clip = audioClips[0];
                GetComponent<AudioSource>().volume = 1f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.buzzer:
                GetComponent<AudioSource>().clip = audioClips[1];
                GetComponent<AudioSource>().volume = 1f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.chaching:
                GetComponent<AudioSource>().clip = audioClips[2];
                GetComponent<AudioSource>().volume = 0.4f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.clapping:
                GetComponent<AudioSource>().clip = audioClips[3];
                GetComponent<AudioSource>().volume = 0.5f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.harbour:
                GetComponent<AudioSource>().clip = audioClips[4];
                GetComponent<AudioSource>().volume = 1f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.loose:
                GetComponent<AudioSource>().clip = audioClips[5];
                GetComponent<AudioSource>().volume = 1f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.scrapping:
                GetComponent<AudioSource>().clip = audioClips[6];
                GetComponent<AudioSource>().volume = 1f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.troet:
                GetComponent<AudioSource>().clip = audioClips[7];
                GetComponent<AudioSource>().volume = 0.5f;
                GetComponent<AudioSource>().Play();
                break;
            case AudioClipType.truck:
                GetComponent<AudioSource>().clip = audioClips[8];
                GetComponent<AudioSource>().volume = 1f;
                GetComponent<AudioSource>().Play();
                break;
        }
    }
}
