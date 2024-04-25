using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameMaster : MonoBehaviour {
    // Start is called before the first frame update
    private int curDay = 0;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moneyText;
    void Start() {
        Company.curMoney = 10000;
        Company.ownedShips = new List<Ship>();
        Company.acceptedContracts = new List<Contract>();
        Company.refreshAvailableShips();
        Company.refreshAvailableContracts();
        Clock.Instance.tick += Tick;
    }
    void Tick() {
        curDay++;
        timeText.text = "Day " + curDay.ToString();
        if (curDay % 25 == 0) {
            Company.refreshAvailableShips();
        }
        if (curDay % 20 == 0) {
            Company.refreshAvailableContracts();
        }
        Company.Tick();
        moneyText.text = "Capital: " + Company.curMoney.ToString() + "$";
    }
}
