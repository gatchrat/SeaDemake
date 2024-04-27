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
            ship.Tick();
        }
        for (int i = 0; i < acceptedContracts.Count; i++) {
            acceptedContracts[i].daysToComplete--;
            if (acceptedContracts[i].daysToComplete < 0) {
                failContract(acceptedContracts[i]);
            }
            i--;
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
    public static void completeContract(Contract toComplete) {
        curMoney += toComplete.reward;
        acceptedContracts.Remove(toComplete);
    }
    public static void failContract(Contract toComplete) {
        curMoney -= toComplete.penalty;
        acceptedContracts.Remove(toComplete);
    }
    public static String sendShip(String shipname, String harbourName) {
        Ship toSend = null;
        foreach (Ship ship in availableShips) {
            if (ship.name == shipname) {
                toSend = ship;
            }
        }
        if (toSend == null) {
            return "No Ship with that Name found";
        }
        Harbour toDock = null;
        foreach (Harbour harbour in allHarbours) {
            if (harbour.name == harbourName) {
                toDock = harbour;
            }
        }
        if (toDock == null) {
            return "No Harbour with that Name found";
        }
        toSend.targetDock = toDock;
        toSend.curPath = Pathfinder.findPath(toSend.pos, toDock.pos);
        companyUiUpdate.Invoke();
        return "Ship successfully send";

    }
}

