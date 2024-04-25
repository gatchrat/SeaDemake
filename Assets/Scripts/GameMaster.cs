using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {
    // Start is called before the first frame update
    private int curDay = 0;
    void Start() {
        Company.curMoney = 10000;
        Clock.Instance.tick += Tick;
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
    }
}
