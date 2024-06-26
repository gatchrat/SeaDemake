using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class Logger : MonoBehaviour {
    private String History = "";
    public TextMeshProUGUI text;
    private readonly int maxLines = 20;
    public static Logger Instance;
    void Awake() {
        Instance = this;
    }
    void Update() {
        UpdateText();
    }
    private void UpdateText() {
        String[] counter = History.Split("\n");
        if (counter.Length > maxLines - 2) {
            String newHistory = "";
            for (int i = counter.Length - maxLines + 2; i < counter.Length; i++) {
                if (i != counter.Length - 1) {
                    newHistory += counter[i] + "\n";
                } else {
                    newHistory += counter[i];
                }

            }
            History = newHistory;
        }
        text.text = History;
    }
    public static void AddLog(String text, Color color) {
        Logger.Instance.History += "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color> \n";
    }
}
