using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Company {
    public static List<Ship> ownedShips;
    public static List<Ship> availableShips = new();
    public static List<Contract> openContracts = new();
    public static List<Contract> acceptedContracts = new();
    public static List<Harbour> allHarbours;
    public static List<Harbour> lockedHarbours = new();
    public static int curMoney = 0;
    public delegate void companyUiUpdateEvent();
    public static event companyUiUpdateEvent CompanyUiUpdate;
    private static Ship lastUsedShip;
    public static void RefreshAvailableShips() {
        availableShips.Add(new Ship());
        if (availableShips.Count > 9) {
            availableShips.RemoveAt(0);
        }
        CompanyUiUpdate.Invoke();
    }
    public static void RefreshAvailableContracts() {
        //generate max up to 6 contracts
        //add one when under 6
        //otherwise replace 3
        //never go above 8 overall contracts
        if (openContracts.Count < 6 && openContracts.Count + acceptedContracts.Count < 8) {
            openContracts.Add(ContractFactory.GenerateContract(allHarbours.Concat(lockedHarbours).ToList()));
        } else {
            for (int i = 0; i < Math.Min(openContracts.Count - 1, 2); i++) {
                openContracts[UnityEngine.Random.Range(0, openContracts.Count)] = ContractFactory.GenerateContract(allHarbours);
            }
        }
        CompanyUiUpdate.Invoke();
    }
    public static void Tick() {
        //substract all running costs
        foreach (Ship ship in ownedShips) {
            curMoney -= ship.runningCosts;
            ship.Tick();
        }
        for (int i = 0; i < acceptedContracts.Count; i++) {
            acceptedContracts[i].daysToComplete--;
            acceptedContracts[i].UpdateGUI();
            if (acceptedContracts[i].daysToComplete < 0) {
                FailContract(acceptedContracts[i]);
                i--;
            }

        }
        if (curMoney < 0) {
            Logger.AddLog("Your company went bankrupt, the game has ended. Type Exit to close the game.", Color.red);
            Clock.Disable();
            GameMaster.Instance.PlayAudio(AudioClipType.loose);
        }
        CompanyUiUpdate.Invoke();
        //advance all accepted contracts
    }
    public static String BuyShip(String shipname, String harbourName) {
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
            CompanyUiUpdate.Invoke();
            GameMaster.Instance.PlayAudio(AudioClipType.clapping);
            lastUsedShip = toBuy;
            return "Ship successfully aquired";
        }
    }
    public static void CompleteContract(Contract toComplete) {
        curMoney += toComplete.reward;
        Logger.AddLog("Contract " + toComplete.name + " completed. Reward: " + toComplete.reward + "$", Color.green);
        GameMaster.Instance.PlayAudio(AudioClipType.chaching);
        acceptedContracts.Remove(toComplete);
    }
    public static void FailContract(Contract toComplete) {
        curMoney -= toComplete.penalty;
        Logger.AddLog("Contract " + toComplete.name + " failed. Penalty: " + toComplete.penalty + "$", Color.red);
        GameMaster.Instance.PlayAudio(AudioClipType.buzzer);
        acceptedContracts.Remove(toComplete);
    }
    public static String SendShip(String shipname, String harbourName) {
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
        toSend.SetPath(Pathfinder.FindPath(toSend.pos, toDock.pos));
        CompanyUiUpdate.Invoke();
        GameMaster.Instance.PlayAudio(AudioClipType.troet);
        lastUsedShip = toSend;
        return "Ship successfully send";

    }
    public static String GetDistance(String Harbour1, String Harbour2) {
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
        return "The Distance is " + Pathfinder.FindPath(startDock.pos, endDock.pos).Count + "Days";
    }
    public static String AcceptContract(String ContractName) {
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
        contract.SetAccepted();
        openContracts.Remove(contract);
        CompanyUiUpdate.Invoke();
        GameMaster.Instance.PlayAudio(AudioClipType.scribble);
        return "The Contract was accepted";
    }
    public static String Scrap(String shipname) {
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
        toScrap.scrap();
        curMoney += 1000;
        CompanyUiUpdate.Invoke();
        GameMaster.Instance.PlayAudio(AudioClipType.scrapping);
        return "The Ship was Scrapped, 1000$ in material cost was recovered";
    }
    public static String Unload(String shipname, String goodName, String countAsString) {
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
        int count;
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
        int price = toUnload.dock.PriceOf(targetGood);
        if (price < 1) {
            return "The Harbour is not interested in these goods";
        }
        for (int i = 0; i < count; i++) {
            toUnload.content.Remove(targetGood);
        }
        curMoney += price * count;
        CompanyUiUpdate.Invoke();
        GameMaster.Instance.PlayAudio(AudioClipType.truck);
        lastUsedShip = toUnload;
        return "The goods where sold successfully";
    }
    public static String Load(String shipname, String goodName, String countAsString) {
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
        int count;
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
        int price = toLoad.dock.PriceOf(targetGood);
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
        CompanyUiUpdate.Invoke();
        GameMaster.Instance.PlayAudio(AudioClipType.truck);
        lastUsedShip = toLoad;
        return "The Ship was loaded and Capital removed";
    }
    public static String Load(String shipnameOrGoodname, String goodNameOrCount) {
        try {
            int count = Int32.Parse(goodNameOrCount);
            return Load(lastUsedShip.name, shipnameOrGoodname, goodNameOrCount);
        } catch {
            return Load(shipnameOrGoodname, goodNameOrCount, "1");
        }
    }
    public static String Unload(String shipnameOrGoodname, String goodNameOrCount) {
        try {
            int count = Int32.Parse(goodNameOrCount);
            return Unload(lastUsedShip.name, shipnameOrGoodname, goodNameOrCount);
        } catch {
            return Unload(shipnameOrGoodname, goodNameOrCount, "1");
        }
    }
    public static String Load(String goodName) {

        return Load(lastUsedShip.name, goodName, "1");
    }
    public static String Unload(String goodName) {

        return Unload(lastUsedShip.name, goodName, "1");
    }
    public static String SendShip(String harbourName) {

        return SendShip(lastUsedShip.name, harbourName);
    }
}

