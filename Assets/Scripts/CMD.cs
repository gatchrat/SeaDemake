using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CMD : MonoBehaviour {
    private String curLine = ">";
    private List<String> curLineHistory = new List<string>();
    private String History = "";
    private int HistoryIndex = 0;
    public TextMeshProUGUI text;
    private int maxLines = 20;
    void Start() {
        curLineHistory.Add(">");
    }
    void Update() {
        updateText();
    }
    private void updateText() {
        String[] counter = History.Split("\n");
        if (counter.Length > maxLines - 2) {
            String newHistory = "";
            for (int i = counter.Length - maxLines + 2; i < counter.Length; i++) {
                if (i != counter.Length - 1) {
                    newHistory += counter[i] + "\n";
                }
                else {
                    newHistory += counter[i];
                }

            }
            History = newHistory;

            text.text = History + curLine + '_';
        }
        else {
            text.text = History + "\n" + curLine + '_';
        }

    }
    public void OnGUI() {
        Event evt = Event.current;

        if (evt.type == EventType.KeyDown) {
            if (evt.keyCode == KeyCode.Backspace) {
                if (curLine != ">") {
                    curLine = curLine.Remove(curLine.Length - 1);
                }
                return;
            }
            else if (evt.keyCode == KeyCode.Return) {
                curLineHistory.Add(curLine);
                History = History + "\n" + curLine;
                History += "\n" + CommandInterpreter.interprete(curLine);
                curLine = ">";
                HistoryIndex = curLineHistory.Count;
                return;
            }
            else if (evt.keyCode == KeyCode.UpArrow) {
                HistoryIndex = Math.Max(0, HistoryIndex - 1);
                curLine = curLineHistory[HistoryIndex];
            }
            else {
                char c = evt.character;
                if (c >= 32 && c <= 126) {
                    //Debug.Log("test" + c.ToString() + "'");
                    curLine += c.ToString();
                }

            }
        }
        else if (evt.type == EventType.KeyUp) {
        }
    }
}
