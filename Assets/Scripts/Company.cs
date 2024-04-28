using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Company {
    public static List<Ship> ownedShips;
    public static List<Ship> availableShips;
    public static List<Contract> openContracts = new List<Contract>();
    public static List<Contract> acceptedContracts = new List<Contract>();
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
        //generate max up to 10 contracts
        //add one when under 10
        //otherwise replace 3
        //never go above 15 overall contracts
        if (openContracts.Count < 10 && openContracts.Count + acceptedContracts.Count < 15) {
            openContracts.Add(ContractFactory.generateContract(allHarbours));
        }
        else {
            for (int i = 0; i < Math.Min(openContracts.Count - 1, 2); i++) {
                openContracts[UnityEngine.Random.Range(0, openContracts.Count)] = ContractFactory.generateContract(allHarbours);
            }
        }
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
        Logger.addLog("Contract " + toComplete.name + " completed. Reward: " + toComplete.reward + "$", Color.green);
        acceptedContracts.Remove(toComplete);
    }
    public static void failContract(Contract toComplete) {
        curMoney -= toComplete.penalty;
        Logger.addLog("Contract " + toComplete.name + " failed. Penalty: " + toComplete.penalty + "$", Color.red);
        acceptedContracts.Remove(toComplete);
    }
    public static String sendShip(String shipname, String harbourName) {
        Ship toSend = null;
        foreach (Ship ship in ownedShips) {
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
        toSend.setPath(Pathfinder.findPath(toSend.pos, toDock.pos));
        companyUiUpdate.Invoke();
        return "Ship successfully send";

    }
    public static String getDistance(String Harbour1, String Harbour2) {
        Harbour startDock = null;
        foreach (Harbour harbour in allHarbours) {
            if (harbour.name == Harbour1) {
                startDock = harbour;
            }
        }
        if (startDock == null) {
            return "No Harbour with that Name found";
        }
        Harbour endDock = null;
        foreach (Harbour harbour in allHarbours) {
            if (harbour.name == Harbour2) {
                endDock = harbour;
            }
        }
        if (endDock == null) {
            return "No Harbour with that Name found";
        }
        return "The Distance is " + Pathfinder.findPath(startDock.pos, endDock.pos).Count + "Days";
    }
}

