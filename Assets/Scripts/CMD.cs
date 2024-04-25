using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CMD : MonoBehaviour
{
    private String curLine = ">";
    private String History = "";
    public TextMeshProUGUI text;
    private int maxLines = 20;
    void Update()
    {
        updateText();
    }
    private void updateText()
    {
        String[] counter = History.Split("\n");
        if (counter.Length > maxLines - 2)
        {
            String newHistory = "";
            for (int i = counter.Length - maxLines + 2; i < counter.Length; i++)
            {
                if (i != counter.Length - 1)
                {
                    newHistory += counter[i] + "\n";
                }
                else
                {
                    newHistory += counter[i];
                }

            }
            History = newHistory;

            text.text = History + curLine;
        }
        else
        {
            text.text = History + "\n" + curLine;
        }

    }
    public void OnGUI()
    {
        Event evt = Event.current;

        if (evt.type == EventType.KeyDown)
        {
            if (evt.keyCode == KeyCode.Backspace)
            {
                return;
            }
            else if (evt.keyCode == KeyCode.Return)
            {
                History = History + "\n" + curLine;
                History += "\n" + CommandInterpreter.interprete(curLine);
                curLine = ">";
                return;
            }
            else
            {
                char c = evt.character;
                if (c >= 32 && c <= 126)
                {
                    //Debug.Log("test" + c.ToString() + "'");
                    curLine += c.ToString();
                }

            }
        }
        else if (evt.type == EventType.KeyUp)
        {
        }
    }
}
