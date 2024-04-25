using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pathfinder : MonoBehaviour {
    public Texture2D worldMap;
    public GameObject pathPiece;
    public Transform parentT;
    // Start is called before the first frame update
    //(x,y), (0,0) is top left
    //(336,189)
    Node[,] map = new Node[336, 189];
    void Start() {
        Debug.Log(logicToPixelPos(new Vector2Int(120, 10)));
        List<Vector2Int> path = new List<Vector2Int>();
        for (int i = 0; i < 336; i++) {
            for (int y = 0; y < 189; y++) {
                map[i, y] = new Node();
                map[i, y].isWater = isWater(worldMap.GetPixel(logicToPixelPos(new Vector2Int(i, y)).x, logicToPixelPos(new Vector2Int(i, y)).y));
                map[i, y].pos = new Vector2Int(i, y);
                if (map[i, y].isWater) {
                    path.Add(new Vector2Int(i, y));
                }
            }
        }
        spawnPieces(findPath(new Vector2Int(150, 60), new Vector2Int(200, 100)));
        //spawnPieces(path);
    }
    void spawnPieces(List<Vector2Int> path) {
        foreach (Vector2Int pos in path) {
            GameObject curPiece = GameObject.Instantiate(pathPiece);
            curPiece.transform.SetParent(parentT, false);
            curPiece.GetComponent<RectTransform>().anchoredPosition = new Vector3(3 + 4 * pos.x, -2 - 4 * pos.y, 0);
            curPiece.transform.localScale = new Vector3(0.75f, 0.75f, 0);
        }
    }
    public static float OctileDistance(Vector2Int from, Vector2Int to) {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        // return Mathf.Max(dx, dy) + (Mathf.Sqrt(2) - 1) * Mathf.Min(dx, dy);
        return Vector2Int.Distance(from, to);
    }
    public List<Vector2Int> findPath(Vector2Int start, Vector2Int end) {
        if (!map[start.x, start.y].isWater || !map[end.x, end.y].isWater) {
            Debug.Log("Cant path on land");
        }
        //reset map
        for (int i = 0; i < 336; i++) {
            for (int y = 0; y < 189; y++) {
                map[i, y].distance = 9999999;
                map[i, y].pathToHere = new List<Vector2Int>();
                map[i, y].visited = false;
                //map[i, y].guestimate = Math.Abs(i - end.x) + Math.Abs(y - end.y);
                map[i, y].guestimate = OctileDistance(new Vector2Int(i, y), end);
            }
        }
        List<Node> backlog = new List<Node>();
        map[start.x, start.y].distance = 0;
        map[start.x, start.y].visited = true;
        //add neighboors
        foreach (Node neighbor in getNeighboors(map[start.x, start.y])) {
            if (neighbor.isWater && !neighbor.visited) {
                if (neighbor.distance > map[start.x, start.y].distance - 1) {
                    neighbor.distance = map[start.x, start.y].distance + 1;
                    //workaround to not pass the list itself but a copy
                    neighbor.pathToHere = map[start.x, start.y].pathToHere.GetRange(0, map[start.x, start.y].pathToHere.Count); ;
                    neighbor.pathToHere.Add(map[start.x, start.y].pos);
                    if (!backlog.Contains(neighbor)) {
                        backlog.Add(neighbor);
                    }

                }
            }
        }
        foreach (Node neighbor in getdiagonalNeighboors(map[start.x, start.y])) {
            if (neighbor.isWater && !neighbor.visited) {
                if (neighbor.distance > map[start.x, start.y].distance - OctileDistance(map[start.x, start.y].pos, neighbor.pos)) {
                    neighbor.distance = map[start.x, start.y].distance + OctileDistance(map[start.x, start.y].pos, neighbor.pos);
                    //workaround to not pass the list itself but a copy
                    neighbor.pathToHere = map[start.x, start.y].pathToHere.GetRange(0, map[start.x, start.y].pathToHere.Count); ;
                    neighbor.pathToHere.Add(map[start.x, start.y].pos);
                    if (!backlog.Contains(neighbor)) {
                        backlog.Add(neighbor);
                    }

                }
            }
        }
        bool found = false;
        while (backlog.Count > 0 && !found) {
            float best = 999999;
            Node bestNode = null;
            foreach (Node node in backlog) {
                if (node.distance + node.guestimate < best) {
                    best = node.distance + node.guestimate;
                    bestNode = node;
                }
            }
            backlog.Remove(bestNode);

            bestNode.visited = true;
            if (bestNode.pos.x == end.x && bestNode.pos.y == end.y) {
                found = true;
                Debug.Log("found path with length" + bestNode.pathToHere.Count);
                return bestNode.pathToHere;
            }
            foreach (Node neighbor in getNeighboors(bestNode)) {
                if (neighbor.isWater && !neighbor.visited) {
                    if (neighbor.distance > bestNode.distance - 1) {
                        neighbor.distance = bestNode.distance + 1;
                        //workaround to not pass the list itself but a copy
                        neighbor.pathToHere = bestNode.pathToHere.GetRange(0, bestNode.pathToHere.Count); ;
                        neighbor.pathToHere.Add(bestNode.pos);
                        if (!backlog.Contains(neighbor)) {
                            backlog.Add(neighbor);
                        }

                    }
                }
            }
            foreach (Node neighbor in getdiagonalNeighboors(bestNode)) {
                if (neighbor.isWater && !neighbor.visited) {
                    if (neighbor.distance > bestNode.distance - OctileDistance(bestNode.pos, neighbor.pos)) {
                        neighbor.distance = bestNode.distance + OctileDistance(bestNode.pos, neighbor.pos);
                        //workaround to not pass the list itself but a copy
                        neighbor.pathToHere = bestNode.pathToHere.GetRange(0, bestNode.pathToHere.Count); ;
                        neighbor.pathToHere.Add(bestNode.pos);
                        if (!backlog.Contains(neighbor)) {
                            backlog.Add(neighbor);
                        }

                    }
                }
            }

        }
        Debug.Log("couldnt find any path");
        return null;
    }
    private List<Node> getNeighboors(Node n) {
        Vector2Int start = n.pos;
        List<Node> backlog = new List<Node>();
        if (start.x < 335) {
            backlog.Add(map[start.x + 1, start.y]);
        }
        else {
            backlog.Add(map[0, start.y]);
        }
        if (start.x > 0) {
            backlog.Add(map[start.x - 1, start.y]);
        }
        else {
            backlog.Add(map[335, start.y]);
        }

        if (start.y < 188) {
            backlog.Add(map[start.x, start.y + 1]);
        }
        else {
            backlog.Add(map[start.x, 0]);
        }

        if (start.y > 0) {
            backlog.Add(map[start.x, start.y - 1]);
        }
        else {
            backlog.Add(map[start.x, 188]);
        }
        return backlog;
    }
    private List<Node> getdiagonalNeighboors(Node n) {
        Vector2Int start = n.pos;
        List<Node> backlog = new List<Node>();
        List<Vector2Int> backlogv = new List<Vector2Int>();
        backlogv.Add(new Vector2Int(start.x + 1, start.y + 1));
        backlogv.Add(new Vector2Int(start.x + 1, start.y - 1));
        backlogv.Add(new Vector2Int(start.x - 1, start.y + 1));
        backlogv.Add(new Vector2Int(start.x - 1, start.y - 1));
        foreach (Vector2Int pos in backlogv) {
            int x = pos.x;
            int y = pos.y;
            if (pos.x > 335) {
                x = 0;
            }
            if (pos.x < 0) {
                x = 335;
            }
            if (pos.y > 188) {
                y = 0;
            }
            if (pos.y < 0) {
                y = 188;
            }
            backlog.Add(map[x, y]);
        }
        return backlog;
    }
    private bool isWater(Color c) {
        return (c.r + c.g + c.b) < 1.6f;
    }
    private Vector2Int logicToPixelPos(Vector2Int logicPos) {
        Vector2Int outV = new Vector2Int(1 + logicPos.x * 4, 756 - logicPos.y * 4 - 2);
        return outV;
    }
    private Vector2Int PixelToLogicPos(Vector2Int logicPos) {
        Vector2Int outV = new Vector2Int((logicPos.x - 1) / 4, (logicPos.y) / 4);
        return outV;
    }


}
public class Node {
    public float distance = 9999999;
    public float guestimate = 9999999;
    public List<Vector2Int> pathToHere = new List<Vector2Int>();
    public bool isWater;
    public bool visited = false;
    public Vector2Int pos;
}
