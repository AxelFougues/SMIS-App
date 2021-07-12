using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
[RequireComponent(typeof(BTSensorBridge))]
[ExecuteInEditMode]
public class SMIS : MonoBehaviour {

    [Header("Links")]
    public SignalGenerator left;
    public SignalGenerator right;
    public SignalPreset defaultSignal;
    public SignalPreset tactileFeedbackSignal;
    public SignalPreset sensorFeedbackSignal;
    public SMISSettings smisSettings;

    [Space]
    [Header("Tactile Settings")]
    [SerializeField]
    public List<Collider> virtualFingers;
    public bool simulateMotion = true;
    [Tooltip("Velocity in mm/s")]
    public float minSlidingVelocity = 1f;

    [Space]
    [Header("UI Texts")]
    public Text pressure;
    public Text surfaceVelocity;
    public Text feedbackFrequency;

    [Space]
    [Header("Test Sequence Settings")]
    [Range(0, 1)]
    public int channel;
    [Range(0, 10)]
    public float testDuration;
    public bool pauses;
    [Range(0, 300)]
    public float minFrequency;
    [Range(0, 300)]
    public float maxFrequency;
    [Range(0.1f, 50f)]
    public float step;
    public bool equalized;
    public bool sine;
    public bool square;
    public bool saw;

    [HideInInspector]
    public List<SignalGenerator> channels;

    AudioSource audioSource;
    [HideInInspector]
    public IEnumerator[] channelFeedbacks;
    RollingAverageFilter sensorFilter = new RollingAverageFilter(5, 2);


    RollingAverageFilter modulationFilter = new RollingAverageFilter(50, 2);

    private void OnEnable() {
        left.channel = 0;
        right.channel = 1;

        channels = new List<SignalGenerator>();
        channels.Add(left);
        channels.Add(right);

        channelFeedbacks = new IEnumerator[2];

        disableFeedback(0);
        disableFeedback(1);

        audioSource = GetComponent<AudioSource>();

    }


    public void disableFeedback(int channel) {
        if (channel < channels.Count) {
            channels[channel].enabled = false;
            if (channelFeedbacks[channel] != null) {
                StopCoroutine(channelFeedbacks[channel]);
                channelFeedbacks[channel] = null;
            }
        }
    }

    public void enableFeedback(int channel) {
        disableFeedback(channel); //reset if currently used
        channels[channel].loadPreset(defaultSignal);
        channels[channel].enabled = true;
    }



//####################################################################################### UTILITIES



    bool isBetween(double testValue, double bound1, double bound2) {
        if (bound1 > bound2)
            return testValue >= bound2 && testValue <= bound1;
        return testValue >= bound1 && testValue <= bound2;
    }

    float mapValue(float referenceValue, float fromMin, float fromMax, float toMin, float toMax) {
        // This function maps (converts) a Float value from one range to another
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    void startFeedbackCoroutine(int channel, IEnumerator coroutine) {
        channelFeedbacks[channel] = coroutine;
        StartCoroutine(coroutine);
    }

}
*/