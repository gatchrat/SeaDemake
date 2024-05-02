using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommandInterpreter {
    public static String interprete(String input) {
        input = input.Remove(0, 1); // remove > at the start
        String[] info = input.Split(' ');
        switch (info[0].ToLower()) //check name of command
        {
            case "help":
                return @"Here is a list of all commands:
                help - Lists all Commands
                send (ship) (target) - Sends the Ship from the current Harbour to the target Harbour, all goods that can will be unloaded
                load (ship) (good) (count) - Buys one of the specified goods and loads it into the specified ship, count is an optional Parameter
                buy (ship) (target) - Buys the specified ship and spawns it in the target harbour
                speed (speed) - sets the game speed to the specified speed, possible options are 1-5
                accept (contractNumber) - accepts the specified Contract
                unload (ship) (good) (count) - Sells the specified good from the specified ship in its current harbour, count is optional
                scrap (ship) - Scraps the ship, removing its running costs
                distance (harbour) (harbour) - Lists the distance in days between the two harbours
                pause - pauses the game, commands are still enabled
                resume - un-pauses
                exit - closes the game
                goods = wood,iron,coal,medicine,food
                load,unload,send commands use the last used ship if parameter ommited";
            case "load":
                if (info.Length == 2) {
                    return Company.load(info[1]);
                }
                if (info.Length == 3) {
                    return Company.load(info[1], info[2]);
                }
                if (info.Length == 4) {
                    return Company.load(info[1], info[2], info[3]);
                } else {
                    return "Missing or too many parameters";
                }
            case "unload":
                if (info.Length == 2) {
                    return Company.unload(info[1]);
                }
                if (info.Length == 3) {
                    return Company.unload(info[1], info[2]);
                }
                if (info.Length == 4) {
                    return Company.unload(info[1], info[2], info[3]);
                } else {
                    return "Missing or too many parameters";
                }
            case "scrap":
                if (info.Length != 2) {
                    return "Missing or too many parameters";
                }
                return Company.scrap(info[1]);
            case "accept":
                if (info.Length != 2) {
                    return "Missing or too many parameters";
                }
                return Company.acceptContract(info[1]);
            case "distance":
                if (info.Length != 3) {
                    return "Missing or too many parameters";
                }
                return Company.getDistance(info[1], info[2]);
            case "send":
                if (info.Length == 2) {
                    return Company.sendShip(info[1]);
                }
                if (info.Length == 3) {
                    return Company.sendShip(info[1], info[2]);
                } else {
                    return "Missing or too many parameters";
                }
            case "resume":
                Clock.Instance.startClock();
                return "Un-paused";
            case "pause":
                Clock.Instance.stopClock();
                return "paused";
            case "speed":
                if (info.Length != 2) {
                    return "Missing or too many parameters";
                }
                return Clock.Instance.setSpeed(info[1]);
            case "buy":
                if (info.Length != 3) {
                    return "Missing or too many parameters";
                } else {
                    return Company.buyShip(info[1], info[2]);
                }
            case "exit":
#if UNITY_STANDALONE
                Application.Quit();
#endif
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return "exiting...";
            default:
                return "Unkown command, use help to get a list of all commands";
        }
    }
}
