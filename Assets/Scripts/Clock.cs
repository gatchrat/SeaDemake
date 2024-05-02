using System;
using UnityEngine;

public class Clock : MonoBehaviour {
    // Start is called before the first frame update
    public float[] timeSteps;
    float timePerTickCur;
    float curTimer;
    Boolean started = false;
    Boolean gameRunning = true;
    public static Clock Instance;
    public delegate void TickEvent();
    public event TickEvent Tick;
    void Awake() {
        Instance = this;
        timePerTickCur = timeSteps[0];
    }
    void Start() {
        curTimer = timePerTickCur;
    }
    public String SetSpeed(String speedString) {
        int speed;
        try {
            speed = Int32.Parse(speedString);
        } catch {
            return "Not a valid Speed";
        }
        if (speed < timeSteps.Length && speed >= 0) {
            timePerTickCur = timeSteps[speed];
            curTimer = timePerTickCur;
            return "Speed changed";
        }
        return "Not a valid Speed";
    }
    public static void Disable() {
        Clock.Instance.StopClock();
        Clock.Instance.gameRunning = false;
    }
    public void StartClock() {
        if (gameRunning) {
            started = true;
        }

    }
    public void StopClock() {
        started = false;
    }
    // Update is called once per frame
    void Update() {
        if (started) {
            curTimer -= Time.deltaTime;
            if (curTimer < 0) {
                curTimer = timePerTickCur;
                Tick.Invoke();
            }
        }

    }
}
