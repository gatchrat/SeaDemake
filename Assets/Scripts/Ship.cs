using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private List<String> potentialNames = new List<string>();
    public bool isMoving() {
        return dock == null;
    }
    public Ship() {
        inventorySize = UnityEngine.Random.Range(1, 5);
        runningCosts = UnityEngine.Random.Range(1, 10);
        price = (int)(standardPrice * (UnityEngine.Random.Range(60f, 140f) / 100f) * inventorySize);
        string path = Application.streamingAssetsPath + "/female.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        String curName = reader.ReadLine();
        while (curName != null) {
            potentialNames.Add(curName);
            curName = reader.ReadLine();
        }
        name = potentialNames[UnityEngine.Random.Range(0, potentialNames.Count)];

        reader.Close();
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
        for (int y = 0; y < contracts.Count; y++) {
            if (contracts[y].targetHarbour == dock) {
                for (int i = 0; i < contracts[y].toDeliverGoods.Count; i++) {
                    for (int x = 0; x < content.Count; x++) {
                        if (contracts[y].toDeliverGoods[i] == content[x]) {
                            content.Remove(content[x]);
                            contracts[y].deliver(contracts[y].toDeliverGoods[i]);
                            x--;
                            i--;

                        }
                    }
                }
                if (contracts[y].toDeliverGoods.Count == 0) {
                    Company.completeContract(contracts[y]);
                    y--;
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
