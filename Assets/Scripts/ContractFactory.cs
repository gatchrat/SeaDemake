using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ContractFactory {
    static int counter = 1;
    public static Contract generateContract(List<Harbour> allHarbours) {
        Contract c = new Contract();
        for (int i = 0; i < Random.Range(1, 6); i++) {
            switch (Random.Range(0, 5)) {

                case 0:
                    c.toDeliverGoods.Add(TypeOfGoods.Food);
                    break;
                case 1:
                    c.toDeliverGoods.Add(TypeOfGoods.Coal);
                    break;
                case 2:
                    c.toDeliverGoods.Add(TypeOfGoods.Iron);
                    break;
                case 3:
                    c.toDeliverGoods.Add(TypeOfGoods.Medicine);
                    break;
                case 4:
                    c.toDeliverGoods.Add(TypeOfGoods.Wood);
                    break;
            }
        }
        float baseRewardPerPiece = Random.Range(1500, 4000);


        c.name = counter + "";
        counter++;
        c.targetHarbour = allHarbours[Random.Range(0, allHarbours.Count)];
        //ensure a viable day count with the unlocked harbours
        c.daysToComplete = (int)(Random.Range(1f, 4f) * (float)Pathfinder.findPath(getRandomButNotThis(c.targetHarbour, allHarbours).pos, c.targetHarbour.pos).Count);
        //reward scales with difficulty, but scales down the reward for easy timings
        c.reward = (int)(baseRewardPerPiece * c.toDeliverGoods.Count * (250 / c.daysToComplete));
        c.penalty = c.reward / Random.Range(1, 5);

        return c;
    }
    static Harbour getRandomButNotThis(Harbour notThis, List<Harbour> allHarbours) {
        Harbour ret = allHarbours[Random.Range(0, allHarbours.Count)];
        while (ret == notThis) {
            ret = allHarbours[Random.Range(0, allHarbours.Count)];
        }
        return ret;
    }
}
