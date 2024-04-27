using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Company {
    public static List<Ship> ownedShips;
    public static List<Ship> availableShips;
    public static List<Contract> openContracts;
    public static List<Contract> acceptedContracts;
    public static List<Harbour> allHarbours;
    public static int curMoney = 0;
    public delegate void companyUiUpdateEvent();
    public static event companyUiUpdateEvent companyUiUpdate;
    public static void refreshAvailableShips() {
        availableShips = new List<Ship>();
        availableShips.Add(new Ship());
        availableShips.Add(new Ship());
        availableShips.Add(new Ship());
        companyUiUpdate.Invoke();
    }
    public static void refreshAvailableContracts() {
        companyUiUpdate.Invoke();
    }
    public static void Tick() {
        //substract all running costs
        foreach (Ship ship in ownedShips) {
            curMoney -= ship.runningCosts;
        }
        companyUiUpdate.Invoke();
        //advance all accepted contracts
    }
    public static String buyShip(String shipname, String harbourName) {
        Ship toBuy = null;
        foreach (Ship ship in availableShips) {
            if (ship.name == shipname) {
                toBuy = ship;
            }
        }
        if (toBuy == null) {
            return "No Ship with that Name found";
        }
        else if (toBuy.price > curMoney) {
            return "Not enough funds available";
        }
        else {
            Harbour toSpawn = null;
            foreach (Harbour harbour in allHarbours) {
                if (harbour.name == harbourName) {
                    toSpawn = harbour;
                }
            }
            if (toSpawn == null) {
                return "No Harbour with that Name found";
            }
            curMoney -= toBuy.price;
            availableShips.Remove(toBuy);
            ownedShips.Add(toBuy);
            toBuy.pos = toSpawn.pos;
            toBuy.dock = toSpawn;
            companyUiUpdate.Invoke();
            return "Ship successfully aquired";
        }
    }
}

