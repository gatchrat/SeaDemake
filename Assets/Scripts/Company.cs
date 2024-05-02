using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Company {
    public static List<Ship> ownedShips;
    public static List<Ship> availableShips = new List<Ship>();
    public static List<Contract> openContracts = new List<Contract>();
    public static List<Contract> acceptedContracts = new List<Contract>();
    public static List<Harbour> allHarbours;
    public static List<Harbour> lockedHarbours = new List<Harbour>();
    public static int curMoney = 0;
    public delegate void companyUiUpdateEvent();
    public static event companyUiUpdateEvent companyUiUpdate;
    public static void refreshAvailableShips() {
        availableShips.Add(new Ship());
        if (availableShips.Count > 9) {
            availableShips.RemoveAt(0);
        }
        companyUiUpdate.Invoke();
    }
    public static void refreshAvailableContracts() {
        //generate max up to 6 contracts
        //add one when under 6
        //otherwise replace 3
        //never go above 8 overall contracts
        if (openContracts.Count < 6 && openContracts.Count + acceptedContracts.Count < 8) {
            openContracts.Add(ContractFactory.generateContract(allHarbours.Concat(lockedHarbours).ToList()));
        } else {
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
            acceptedContracts[i].updateGUI();
            if (acceptedContracts[i].daysToComplete < 0) {
                failContract(acceptedContracts[i]);
                i--;
            }

        }
        if (curMoney < 0) {
            Logger.addLog("Your company went bankrupt, the game has ended. Type Exit to close the game.", Color.red);
            Clock.disable();
            GameMaster.Instance.playAudio(AudioClipType.loose);
        }
        companyUiUpdate.Invoke();
        //advance all accepted contracts
    }
    public static String buyShip(String shipname, String harbourName) {
        if (ownedShips.Count == 9) {
            return "You already own the max. number of ships. Remember that you can scrap older ships";
        }
        Ship toBuy = null;
        foreach (Ship ship in availableShips) {
            if (ship.name.ToLower() == shipname.ToLower()) {
                toBuy = ship;
            }
        }
        if (toBuy == null) {
            return "No Ship with that Name found";
        } else if (toBuy.price > curMoney) {
            return "Not enough funds available";
        } else {
            Harbour toSpawn = null;
            foreach (Harbour harbour in allHarbours) {
                if (harbour.name.ToLower() == harbourName.ToLower()) {
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
            GameMaster.Instance.playAudio(AudioClipType.clapping);
            return "Ship successfully aquired";
        }
    }
    public static void completeContract(Contract toComplete) {
        curMoney += toComplete.reward;
        Logger.addLog("Contract " + toComplete.name + " completed. Reward: " + toComplete.reward + "$", Color.green);
        GameMaster.Instance.playAudio(AudioClipType.chaching);
        acceptedContracts.Remove(toComplete);
    }
    public static void failContract(Contract toComplete) {
        curMoney -= toComplete.penalty;
        Logger.addLog("Contract " + toComplete.name + " failed. Penalty: " + toComplete.penalty + "$", Color.red);
        GameMaster.Instance.playAudio(AudioClipType.buzzer);
        acceptedContracts.Remove(toComplete);
    }
    public static String sendShip(String shipname, String harbourName) {
        Ship toSend = null;
        foreach (Ship ship in ownedShips) {
            if (ship.name.ToLower() == shipname.ToLower()) {
                toSend = ship;
            }
        }
        if (toSend == null) {
            return "No Ship with that Name found";
        }
        if (toSend.targetDock != null) {
            return "Ship is already moving";
        }
        Harbour toDock = null;
        foreach (Harbour harbour in allHarbours) {
            if (harbour.name.ToLower() == harbourName.ToLower()) {
                toDock = harbour;
            }
        }
        if (toDock == null) {
            return "No Harbour with that Name found";
        }
        if (toSend.dock == toDock) {
            return "Ship is already at the target Dock";
        }
        toSend.targetDock = toDock;
        toSend.setPath(Pathfinder.findPath(toSend.pos, toDock.pos));
        companyUiUpdate.Invoke();
        GameMaster.Instance.playAudio(AudioClipType.troet);
        return "Ship successfully send";

    }
    public static String getDistance(String Harbour1, String Harbour2) {
        Harbour startDock = null;
        foreach (Harbour harbour in allHarbours) {
            if (harbour.name.ToLower() == Harbour1.ToLower()) {
                startDock = harbour;
            }
        }
        if (startDock == null) {
            return "No Harbour with that Name found";
        }
        Harbour endDock = null;
        foreach (Harbour harbour in allHarbours) {
            if (harbour.name.ToLower() == Harbour2.ToLower()) {
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
        contract.setAccepted();
        openContracts.Remove(contract);
        companyUiUpdate.Invoke();
        GameMaster.Instance.playAudio(AudioClipType.scribble);
        return "The Contract was accepted";
    }
    public static String scrap(String shipname) {
        Ship toScrap = null;
        foreach (Ship ship in ownedShips) {
            if (ship.name.ToLower() == shipname.ToLower()) {
                toScrap = ship;
            }
        }
        if (toScrap == null) {
            return "No Ship with that Name found";
        }
        ownedShips.Remove(toScrap);
        curMoney += 1000;
        companyUiUpdate.Invoke();
        GameMaster.Instance.playAudio(AudioClipType.scrapping);
        return "The Ship was Scrapped, 1000$ in material cost was recovered";
    }
    public static String unload(String shipname, String goodName, String countAsString) {
        Ship toUnload = null;
        foreach (Ship ship in ownedShips) {
            if (ship.name.ToLower() == shipname.ToLower()) {
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
        } catch {
            return "Not a valid Count";
        }
        //decode good
        TypeOfGoods targetGood;
        switch (goodName.ToLower()) {
            case "food":
                targetGood = TypeOfGoods.Food;
                break;
            case "medicine":
                targetGood = TypeOfGoods.Medicine;
                break;
            case "wood":
                targetGood = TypeOfGoods.Wood;
                break;
            case "iron":
                targetGood = TypeOfGoods.Iron;
                break;
            case "coal":
                targetGood = TypeOfGoods.Coal;
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
        companyUiUpdate.Invoke();
        GameMaster.Instance.playAudio(AudioClipType.truck);
        return "The goods where sold successfully";
    }
    public static String load(String shipname, String goodName, String countAsString) {
        Ship toLoad = null;
        foreach (Ship ship in ownedShips) {
            if (ship.name.ToLower() == shipname.ToLower()) {
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
        } catch {
            return "Not a valid Count";
        }
        //decode good
        TypeOfGoods targetGood;
        switch (goodName.ToLower()) {
            case "food":
                targetGood = TypeOfGoods.Food;
                break;
            case "medicine":
                targetGood = TypeOfGoods.Medicine;
                break;
            case "wood":
                targetGood = TypeOfGoods.Wood;
                break;
            case "iron":
                targetGood = TypeOfGoods.Iron;
                break;
            case "coal":
                targetGood = TypeOfGoods.Coal;
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
        companyUiUpdate.Invoke();
        GameMaster.Instance.playAudio(AudioClipType.truck);
        return "The Ship was loaded and Capital removed";
    }
    public static String load(String shipname, String goodName) {

        return load(shipname, goodName, "1");
    }
    public static String unload(String shipname, String goodName) {

        return unload(shipname, goodName, "1");
    }
}

