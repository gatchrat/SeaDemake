using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Company {
    public static List<Ship> ownedShips;
    public static List<Ship> availableShips;
    public static List<Contract> openContracts;
    public static List<Contract> acceptedContracts;
    public static int curMoney = 0;
    public static void refreshAvailableShips() {

    }
    public static void refreshAvailableContracts() {

    }
    public static void Tick() {
        //substract all running costs
        foreach (Ship ship in ownedShips) {
            curMoney -= ship.runningCosts;
        }
        //advance all accepted contracts
    }
}

