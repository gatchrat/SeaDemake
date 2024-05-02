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
    static readonly Node[,] map = new Node[336, 189];
    static Pathfinder Instance;
    void Awake() {
        Instance = this;
        List<Vector2Int> path = new();
        for (int i = 0; i < 336; i++) {
            for (int y = 0; y < 189; y++) {
                map[i, y] = new Node {
                    isWater = IsWater(worldMap.GetPixel(LogicToPixelPos(new Vector2Int(i, y)).x, LogicToPixelPos(new Vector2Int(i, y)).y)),
                    pos = new Vector2Int(i, y)
                };
                if (map[i, y].isWater) {
                    if (i >= 299 && y >= 146) {
                        path.Add(new Vector2Int(i, y));
                    }

                }
            }
        }
        //spawnPieces(path);
        //spawnPieces(findPath(new Vector2Int(150, 60), new Vector2Int(200, 100)));
    }
    void Update() {
        /*Vector2 screen_pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 anchorPos = screen_pos - new Vector2(Pathfinder.Instance.parentT.position.x, Pathfinder.Instance.parentT.position.y);
        anchorPos = new Vector2(anchorPos.x / Pathfinder.Instance.parentT.lossyScale.x + 671, anchorPos.y / Pathfinder.Instance.parentT.lossyScale.y - 379);

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Debug.Log(PixelToLogicPos(new Vector2Int((int)anchorPos.x, (int)anchorPos.y)));
            GameObject curPiece = GameObject.Instantiate(Pathfinder.Instance.pathPiece);
            curPiece.transform.SetParent(Pathfinder.Instance.parentT, false);
            curPiece.GetComponent<RectTransform>().anchoredPosition = new Vector3(anchorPos.x, anchorPos.y, 0);
        }*/
    }
    public static List<GameObject> SpawnPieces(List<Vector2Int> path) {
        List<GameObject> pathObjects = new();
        foreach (Vector2Int pos in path) {
            GameObject curPiece = GameObject.Instantiate(Pathfinder.Instance.pathPiece);
            curPiece.transform.SetParent(Pathfinder.Instance.parentT, false);
            curPiece.GetComponent<RectTransform>().anchoredPosition = new Vector3(3 + 4 * pos.x, -2 - 4 * pos.y, 0);
            curPiece.transform.localScale = new Vector3(0.75f, 0.75f, 0);
            pathObjects.Add(curPiece);
        }
        return pathObjects;
    }
    static float Heuristic(Vector2Int from, Vector2Int to) {
        int xDiff = Math.Abs(from.x - to.x);
        int yDiff = Math.Abs(from.y - to.y);
        float heur = Math.Min(xDiff, yDiff) * 1.5f;
        heur += (Math.Max(xDiff, yDiff) - Math.Min(xDiff, yDiff));
        return heur + 3f;
    }
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end) {
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
                map[i, y].guestimate = Heuristic(new Vector2Int(i, y), end);
            }
        }
        List<Node> backlog = new();
        map[start.x, start.y].distance = 0;
        map[start.x, start.y].visited = true;
        //add neighboors
        foreach (Node neighbor in GetNeighboors(map[start.x, start.y])) {
            if (neighbor.isWater && !neighbor.visited) {
                if (neighbor.distance > map[start.x, start.y].distance - Vector2Int.Distance(map[start.x, start.y].pos, neighbor.pos)) {
                    neighbor.distance = map[start.x, start.y].distance + Vector2Int.Distance(map[start.x, start.y].pos, neighbor.pos);
                    //workaround to not pass the list itself but a copy
                    neighbor.pathToHere = map[start.x, start.y].pathToHere.GetRange(0, map[start.x, start.y].pathToHere.Count); ;
                    neighbor.pathToHere.Add(map[start.x, start.y].pos);
                    if (!backlog.Contains(neighbor)) {
                        //Debug.Log("added at start " + neighbor.pos + "with heuristic " + neighbor.guestimate + " and distance so far " + neighbor.distance + "score:" + (neighbor.guestimate + neighbor.distance));
                        backlog.Add(neighbor);
                    }

                }
            }
        }
        foreach (Node neighbor in GetdiagonalNeighboors(map[start.x, start.y])) {
            if (neighbor.isWater && !neighbor.visited) {
                if (neighbor.distance > map[start.x, start.y].distance - Vector2Int.Distance(map[start.x, start.y].pos, neighbor.pos)) {
                    neighbor.distance = map[start.x, start.y].distance + Vector2Int.Distance(map[start.x, start.y].pos, neighbor.pos);
                    //workaround to not pass the list itself but a copy
                    neighbor.pathToHere = map[start.x, start.y].pathToHere.GetRange(0, map[start.x, start.y].pathToHere.Count); ;
                    neighbor.pathToHere.Add(map[start.x, start.y].pos);
                    if (!backlog.Contains(neighbor)) {
                        //Debug.Log("added at start " + neighbor.pos + "with heuristic " + neighbor.guestimate + " and distance so far " + neighbor.distance + "score:" + (neighbor.guestimate + neighbor.distance));
                        backlog.Add(neighbor);
                    }

                }
            }
        }
        while (backlog.Count > 0) {
            float best = 999999;
            float bestPath = 999999;
            Node bestNode = null;
            foreach (Node node in backlog) {
                // Debug.Log("checking" + node.pos + "with score" + (node.distance + node.guestimate));
                if (node.distance + node.guestimate == best & node.distance < bestPath) {
                    best = node.distance + node.guestimate;
                    bestNode = node;
                    bestPath = node.distance;
                }
                if (node.distance + node.guestimate < best) {
                    best = node.distance + node.guestimate;
                    bestNode = node;
                    bestPath = node.distance;
                }

            }
            // Debug.Log("Investigating " + bestNode.pos + "with heuristic " + bestNode.guestimate + " and distance so far " + bestNode.distance + "score:" + (bestNode.guestimate + bestNode.distance));


            bestNode.visited = true;
            if (bestNode.pos.x == end.x && bestNode.pos.y == end.y) {
                return bestNode.pathToHere;
            }
            foreach (Node neighbor in GetNeighboors(bestNode)) {
                if (neighbor.isWater && !neighbor.visited) {
                    if (neighbor.distance > bestNode.distance + Vector2Int.Distance(bestNode.pos, neighbor.pos)) {
                        neighbor.distance = bestNode.distance + Vector2Int.Distance(bestNode.pos, neighbor.pos);
                        //workaround to not pass the list itself but a copy
                        neighbor.pathToHere = bestNode.pathToHere.GetRange(0, bestNode.pathToHere.Count); ;
                        neighbor.pathToHere.Add(bestNode.pos);
                        if (!backlog.Contains(neighbor)) {
                            backlog.Add(neighbor);
                        }

                    }
                }
            }
            foreach (Node neighbor in GetdiagonalNeighboors(bestNode)) {
                if (neighbor.isWater && !neighbor.visited) {
                    if (neighbor.distance > bestNode.distance + Vector2Int.Distance(bestNode.pos, neighbor.pos)) {
                        neighbor.distance = bestNode.distance + Vector2Int.Distance(bestNode.pos, neighbor.pos);
                        //workaround to not pass the list itself but a copy
                        neighbor.pathToHere = bestNode.pathToHere.GetRange(0, bestNode.pathToHere.Count); ;
                        neighbor.pathToHere.Add(bestNode.pos);
                        if (!backlog.Contains(neighbor)) {
                            backlog.Add(neighbor);
                        }

                    }
                }
            }
            backlog.Remove(bestNode);

        }
        Debug.Log("couldnt find any path");
        return null;
    }
    private static List<Node> GetNeighboors(Node n) {
        Vector2Int start = n.pos;
        List<Node> backlog = new();
        if (start.x < 335) {
            backlog.Add(map[start.x + 1, start.y]);
        } else {
            backlog.Add(map[0, start.y]);
        }
        if (start.x > 0) {
            backlog.Add(map[start.x - 1, start.y]);
        } else {
            backlog.Add(map[335, start.y]);
        }

        if (start.y < 188) {
            backlog.Add(map[start.x, start.y + 1]);
        }

        if (start.y > 0) {
            backlog.Add(map[start.x, start.y - 1]);
        }
        return backlog;
    }
    private static List<Node> GetdiagonalNeighboors(Node n) {
        Vector2Int start = n.pos;
        List<Node> backlog = new();
        List<Vector2Int> backlogv = new() {
            new Vector2Int(start.x + 1, start.y + 1),
            new Vector2Int(start.x + 1, start.y - 1),
            new Vector2Int(start.x - 1, start.y + 1),
            new Vector2Int(start.x - 1, start.y - 1)
        };
        foreach (Vector2Int pos in backlogv) {
            int x = pos.x;
            int y = pos.y;
            if (pos.x > 335) {
                x = 0;
            }
            if (pos.x < 0) {
                x = 335;
            }
            if (pos.y < 0 || pos.y > 188) {

            } else {
                backlog.Add(map[x, y]);
            }

        }
        return backlog;
    }
    private bool IsWater(Color c) {
        return (c.r + c.g + c.b) < 1.6f;
    }
    private Vector2Int LogicToPixelPos(Vector2Int logicPos) {
        Vector2Int outV = new(1 + logicPos.x * 4, 756 - logicPos.y * 4 - 2);
        return outV;
    }
    private Vector2Int PixelToLogicPos(Vector2Int logicPos) {
        Vector2Int outV = new((logicPos.x - 1) / 4, (logicPos.y) / 4);
        return outV;
    }


}
public class Node {
    public float distance = float.MaxValue;
    public float guestimate = float.MaxValue;
    public List<Vector2Int> pathToHere = new();
    public bool isWater;
    public bool visited = false;
    public Vector2Int pos;
}
