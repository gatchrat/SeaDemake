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
    public static String acceptContract(String ContractName) {
        Contract contract = null;
        foreach (Contract contracts in openContracts) {
            if (contracts.name == ContractName) {
                contract = contracts;
            }
        }
        if (contract == null) {
            return "No Contract with that ID found";
        }
        acceptedContracts.Add(contract);
        openContracts.Remove(contract);
        companyUiUpdate.Invoke();
        return "The Contract was accepted";
    }
    public static String scrap(String shipname) {
        Ship toScrap = null;
        foreach (Ship ship in ownedShips) {
            if (ship.name == shipname) {
                toScrap = ship;
            }
        }
        if (toScrap == null) {
            return "No Ship with that Name found";
        }
        ownedShips.Remove(toScrap);
        curMoney += 1000;
        companyUiUpdate.Invoke();
        return "The Ship was Scrapped, 1000$ in material cost was recovered";
    }
    public static String unload(String shipname, String goodName, String countAsString) {
        Ship toUnload = null;
        foreach (Ship ship in ownedShips) {
            if (ship.name == shipname) {
                toUnload = ship;
            }
        }
        if (toUnload == null) {
            return "No Ship with that Name found";
        }
        //decode count
        int count = 0;
        try {
            count = Int32.Parse(countAsString);
            if (count < 1) {
                return "Not a valid Count";
            }
        }
        catch {
            return "Not a valid Count";
        }
        //decode good
        TypeOfGoods targetGood;
        switch (goodName) {
            case "food":
                targetGood = TypeOfGoods.Food;
                break;
            case "medicine":
                targetGood = TypeOfGoods.Medicine;
                break;
            case "rawmaterials":
                targetGood = TypeOfGoods.rawMaterials;
                break;
            default:
                return "Unkown Cargo";
        }
        //check if good is loaded
        //check if good is loaded in required amount
        int loaded = 0;
        foreach (TypeOfGoods item in toUnload.content) {
            if (item == targetGood) {
                loaded++;
            }
        }
        if (loaded < count) {
            return "The Ship does not have enough of this type of cargo loaded";
        }
        int price = toUnload.dock.priceOf(targetGood);
        if (price < 1) {
            return "The Harbour is not interested in these goods";
        }
        for (int i = 0; i < count; i++) {
            toUnload.content.Remove(targetGood);
        }
        curMoney += price * count;
        return "The goods where sold successfully";
    }
    public static String load(String shipname, String goodName, String countAsString) {
        Ship toLoad = null;
        foreach (Ship ship in ownedShips) {
            if (ship.name == shipname) {
                toLoad = ship;
            }
        }
        if (toLoad == null) {
            return "No Ship with that Name found";
        }
        if (toLoad.targetDock != null) {
            return "Ship is currently not docked";
        }
        //decode count
        int count = 0;
        try {
            count = Int32.Parse(countAsString);
            if (count < 1) {
                return "Not a valid Count";
            }
        }
        catch {
            return "Not a valid Count";
        }
        //decode good
        TypeOfGoods targetGood;
        switch (goodName) {
            case "food":
                targetGood = TypeOfGoods.Food;
                break;
            case "medicine":
                targetGood = TypeOfGoods.Medicine;
                break;
            case "rawmaterials":
                targetGood = TypeOfGoods.rawMaterials;
                break;
            default:
                return "Unkown Cargo";
        }
        int price = toLoad.dock.priceOf(targetGood);
        if (price == 0) {
            return "The good is not sold here";
        }
        //check if Company can afford
        if (price * count > curMoney) {
            return "Your Company does not have the neccessary funds";
        }
        //check if ship has enough space
        if (toLoad.inventorySize - toLoad.content.Count < count) {
            return "The ship does not have the neccessary inventory space";
        }
        for (int i = 0; i < count; i++) {
            toLoad.content.Add(targetGood);
        }
        curMoney -= price * count;
        return "The Ship was loaded and Capital removed";
    }
    public static String load(String shipname, String goodName) {

        return load(shipname, goodName, "1");
    }
    public static String unload(String shipname, String goodName) {

        return unload(shipname, goodName, "1");
    }
}

