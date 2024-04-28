using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ContractFactory {
    public static Contract generateContract(List<Harbour> allHarbours) {
        Contract c = new Contract();
        c.reward = 10000;
        c.penalty = c.reward / 2;
        c.name = "test";
        c.daysToComplete = 10;
        c.targetHarbour = allHarbours[Random.Range(0, allHarbours.Count)];
        return c;
    }
}
