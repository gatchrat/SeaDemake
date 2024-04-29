using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ship {
    public String name;
    public int inventorySize = 3;
    public List<TypeOfGoods> content = new List<TypeOfGoods>();
    private int standardPrice = 10000;
    public int runningCosts = 0;
    public int price = 0;
    public Vector2Int pos;
    //current dock and target
    public Harbour dock;
    public Harbour targetDock;
    private List<Vector2Int> curPath = null;
    private List<GameObject> curPathObjects = null;
    public bool isMoving() {
        return dock == null;
    }
    public Ship() {
        name = UnityEngine.Random.Range(0, 100) + "";
        price = (int)(standardPrice * (UnityEngine.Random.Range(60f, 140f) / 100f));
    }
    public void setPath(List<Vector2Int> newPath) {
        curPath = newPath;
        if (curPathObjects != null) {
            foreach (GameObject pathPiece in curPathObjects) {
                GameObject.Destroy(pathPiece);
            }
        }

        curPathObjects = Pathfinder.spawnPieces(curPath);
    }
    public void Tick() {
        if (curPath != null) {
            pos = curPath[0];
            curPath.RemoveAt(0);
            GameObject.Destroy(curPathObjects[0]);
            curPathObjects.RemoveAt(0);
            if (curPath.Count == 0) {
                curPath = null;
                dock = targetDock;
                targetDock = null;
                unloadContracts(Company.acceptedContracts);
                Logger.addLog("Ship " + name + " arrived at " + dock.name, Color.white);
            }
        }
    }
    private void unloadContracts(List<Contract> contracts) {
        foreach (Contract contract in contracts) {
            if (contract.targetHarbour == dock) {
                for (int i = 0; i < contract.toDeliverGoods.Count; i++) {
                    for (int x = 0; x < content.Count; x++) {
                        if (contract.toDeliverGoods[i] == content[x]) {
                            content.Remove(content[x]);
                            contract.deliver(contract.toDeliverGoods[i]);
                            x--;
                            i--;

                        }
                    }
                }
                if (contract.toDeliverGoods.Count == 0) {
                    Company.completeContract(contract);
                }
            }
        }
    }
    public void scrap() {
        //remove all graphics
        foreach (GameObject pathPiece in curPathObjects) {
            GameObject.Destroy(pathPiece);
        }
    }
}
