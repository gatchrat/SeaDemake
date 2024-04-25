using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommandInterpreter
{
    public static String interprete(String input)
    {
        input = input.Remove(0, 1); // remove > at the start
        String[] info = input.Split(' ');
        switch (info[0]) //check name of command
        {
            case "help":
                return @"Here is a list of all commands:
                help - Lists all Commands
                send (ship) (target) - Sends the Ship from the current Harbour to the target Harbour, all goods that can will be unloaded
                buy (ship) (good) (count) - Buys one of the specified goods and loads it into the specified ship, count is an optional Parameter
                buy (ship) (target) - Buys the specified ship and spawns it in the target harbour
                speed (speed) - sets the game speed to the specified speed, possible options are 1-5
                accept (contractNumber) - accepts the specified Contract
                price (good) - Lists the price of the good in all available Location
                sell (ship) (good) (count) - Sells the specified good from the specified ship in its current harbour, count is optional
                scrap (ship) - Scraps the ship, removing its running costs
                goods (harbour) - Lists the prices of all goods in the specified harbour
                distance (harbour) (harbour) - Lists the distance in days between the two harbours
                pause - pauses the game, commands are still enabled
                resume - un-pauses
                exit - closes the game";
            default:
                return "Unkown command, use help to get a list of all commands";
        }
        return " Multi \n  Line Return";
    }
}
