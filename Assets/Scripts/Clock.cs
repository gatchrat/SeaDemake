using System;
using UnityEngine;

public class Clock : MonoBehaviour {
    // Start is called before the first frame update
    public float[] timeSteps;
    float timePerTickCur;
    int curTimeStepIndex = 0;
    float curTimer;
    int tickCount = 0;
    Boolean started = false;
    public static Clock Instance;
    public delegate void TickEvent();
    public event TickEvent tick;
    void Awake() {
        Instance = this;
        timePerTickCur = timeSteps[curTimeStepIndex];
    }
    void Start() {
        curTimer = timePerTickCur;
    }
    public String setSpeed(String speedString) {
        int speed = 0;
        try {
            speed = Int32.Parse(speedString);
        }
        catch {
            return "Not a valid Speed";
        }
        if (speed < timeSteps.Length && speed >= 0) {
            timePerTickCur = timeSteps[speed];
            curTimer = timePerTickCur;
            return "Speed changed";
        }
        return "Not a valid Speed";
    }
    public void startClock() {
        started = true;
    }
    public void stopClock() {
        started = false;
    }
    // Update is called once per frame
    void Update() {
        if (started) {
            curTimer -= Time.deltaTime;
            if (curTimer < 0) {
                curTimer = timePerTickCur;
                // TICK TOCK MF
                tickCount++;
                tick.Invoke();
            }
        }

    }
}
