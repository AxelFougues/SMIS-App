using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class TestSequenceFeature : MonoBehaviour{
    [HideInInspector]
    public TestResults currentTest;

    public GameObject resultsField;
    public GameObject overlay;
    public Alert answerAlert;

    [Space]

    public GameObject resultsTitle;
    public GameObject resultsLine;

    [Space]

    public Toggle ampDetToggle;
    public Toggle ampDisToggle;
    public Toggle freqDisToggle;
    public Toggle tempDisToggle;

    public Toggle varyFreqToggle;
    public Toggle varySignalToggle;

    [Space]

    public TMP_Text estimatedTime;

    public bool wait = true;
    public bool answer = false;

    float calculatedTime = 0;

    bool ampDet;
    bool ampDis;
    bool freqDis;
    bool tempDis;

    bool varyFreq;
    bool varySignal;

    const float AMP_SUBDIVISION = 0.01f;
    const float AMP_MAX = 0.3f;
    const float AMP_MIN = 0.0f;

    const float FREQ_MIN = 10;
    const float FREQ_MAX = 250;
    const float FREQ_SUBDIVISION = 10;
    const float DEFAULT_FREQUENCY = 100;

    const float ITERATIONS = 10;

    SignalGenerator genie;
    GameObject audioOut;

    public SignalPreset resetPreset;

    private void Start() {
        audioOut = GameObject.FindGameObjectWithTag("AudioOut");
        genie = audioOut.GetComponents<SignalGenerator>()[2];
        genie.loadPreset(resetPreset);
        refreshSettingsVisuals();
    }

    private void OnDisable() {
        StopAllCoroutines();
        genie.loadPreset(resetPreset);
        overlay.SetActive(false);
        genie.enabled = false;
        answerAlert.gameObject.SetActive(false);
    }

    public void updateSettings() {
        ampDet = ampDetToggle.isOn;
        ampDis = ampDisToggle.isOn;
        freqDis = freqDisToggle.isOn;
        tempDis = tempDisToggle.isOn;
        varyFreq = varyFreqToggle.isOn;
        varySignal = varySignalToggle.isOn;
        calcTime();
    }

    public void refreshSettingsVisuals() {
        ampDetToggle.isOn = ampDet;
        ampDisToggle.isOn = ampDis;
        freqDisToggle.isOn = freqDis;
        tempDisToggle.isOn = tempDis;
        varyFreqToggle.isOn = varyFreq;
        varySignalToggle.isOn = varySignal;
    }

    public void answerYes() { answer = true; wait = false; }
    public void answerNo() { answer = false; wait = false; }

    void clearResults() {
        currentTest = new TestResults();
        foreach (Transform child in resultsField.transform) Destroy(child.gameObject);
    }

    void askForAnswer(string question) {
        answerAlert.gameObject.SetActive(true);
        answerAlert.variableText.text = question;
        wait = true;
        answer = false;
    }

    void writeResultsTitle(string title) {
        GameObject titleObject = Instantiate(resultsTitle, resultsField.transform);
        titleObject.GetComponent<TMP_Text>().text = title;
    }

    void writeResultsLine(string line) {
        GameObject lineObject = Instantiate(resultsLine, resultsField.transform);
        lineObject.GetComponent<TMP_Text>().text = line;
    }

    void writeResults() {
        if (currentTest.amplitudeDetectionThresholdsSine.Count > 0) {
            writeResultsTitle("Amplitude detection: Sine");
            foreach(KeyValuePair<float, float> pair in currentTest.amplitudeDetectionThresholdsSine) writeResultsLine("Threshold @ " + pair.Key + " Hz = " + pair.Value);
        }
        if (currentTest.amplitudeDetectionThresholdsSquare.Count > 0) {
            writeResultsTitle("Amplitude detection: Square");
            foreach (KeyValuePair<float, float> pair in currentTest.amplitudeDetectionThresholdsSquare) writeResultsLine("Threshold @ " + pair.Key + " Hz = " + pair.Value);
        }
        if (currentTest.amplitudeDetectionThresholdsSaw.Count > 0) {
            writeResultsTitle("Amplitude detection: Saw");
            foreach (KeyValuePair<float, float> pair in currentTest.amplitudeDetectionThresholdsSaw) writeResultsLine("Threshold @ " + pair.Key + " Hz = " + pair.Value);
        }
    }

    void calcTime() {
        calculatedTime = 0;
        if (ampDet) calculatedTime += ((AMP_MAX - AMP_MIN) / AMP_SUBDIVISION / 2) * (varyFreq ? (FREQ_MAX - FREQ_MIN) / FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 3;
        //for ampDetplusminus : ITERATIONS * (varyFreq ? (FREQ_MAX - FREQ_MIN) / FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 3;

        estimatedTime.text = "Estimated test duration: " + calculatedTime/60 + " minutes";
    }

    public void launch() {
        Debug.Log("Launching");
        StartCoroutine(doSequences());
    }

    IEnumerator doSequences() {
        genie.enabled = true;
        genie.loadPreset(resetPreset);
        overlay.SetActive(true);
        clearResults();

        //Start screen
        askForAnswer("Ready?");
        yield return new WaitUntil(() => !wait); //wait for answer
        if (answer) {
            //Amp Det
            if (ampDet) yield return StartCoroutine(amplitudeDetection());
            //Amp Dis
            if (ampDis) yield return StartCoroutine(amplitudeDiscrimination());
            //FreqDis
            if (freqDis) yield return StartCoroutine(FrequencyDiscrimination());
            //TempDis
            if (tempDis) yield return StartCoroutine(TemporalDiscrimination());
        }

        writeResults();
        overlay.SetActive(false);
        genie.enabled = false;

        yield return null;
    }


    IEnumerator amplitudeDetection() {
        genie.loadPreset(resetPreset);
        genie.useSinusAudioWave = true;
        genie.sinusAudioWaveIntensity = 1;
        
        yield return StartCoroutine(amplitudeDetectionSequence());

        if (varySignal) {
            genie.loadPreset(resetPreset);
            genie.useSquareAudioWave = true;
            genie.squareAudioWaveIntensity = 1;
            
            yield return StartCoroutine(amplitudeDetectionSequence());

            genie.loadPreset(resetPreset);
            genie.useSawAudioWave = true;
            genie.sawAudioWaveIntensity = 1;
            
            yield return StartCoroutine(amplitudeDetectionSequence());
        }
    }
    IEnumerator amplitudeDetectionSequencePlusMinus() {
        System.Random rand = new System.Random();
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = FREQ_MIN; frequency <= FREQ_MAX; frequency += FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(DEFAULT_FREQUENCY);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //PREPING GENERATOR

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            float amp = AMP_MIN + (AMP_MAX-AMP_MIN / 2f);
            float currentMin = AMP_MIN, currentMax = AMP_MAX;

            for(int itteration = 0; itteration < ITERATIONS; itteration ++){
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(1); //pause
                genie.masterVolume = amp;
                yield return new WaitForSecondsRealtime(1); //stimulation
                genie.masterVolume = 0;
                wait = true;
                yield return new WaitUntil(() => !wait); //wait for answer
                if (answer == true) currentMax = amp;
                else currentMin = amp;
                amp = currentMin + (currentMax - currentMin / 2);
            }
            //saving threshold
            if (genie.useSinusAudioWave) currentTest.amplitudeDetectionThresholdsSine.Add(frequency, amp);
            if (genie.useSquareAudioWave) currentTest.amplitudeDetectionThresholdsSquare.Add(frequency, amp);
            if (genie.useSawAudioWave) currentTest.amplitudeDetectionThresholdsSaw.Add(frequency, amp);
        }

    }
    IEnumerator amplitudeDetectionSequence() {
        //PREPING SEQUENCE
        Debug.Log("Prepping sequence");
        //Amp
        List<float> sequence = new List<float>();
        for (float amplitude = 0; amplitude <= AMP_MAX; amplitude += AMP_SUBDIVISION) sequence.Add(amplitude); //generating test values
        System.Random rand = new System.Random();
        sequence = sequence.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = FREQ_MIN; frequency <= FREQ_MAX; frequency += FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(200);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //PREPING GENERATOR

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            float threshold = 1;
            foreach (float amp in sequence) {
                if (amp < threshold) {
                    Debug.Log("Amp " + amp + " freq " + frequency);
                    genie.masterVolume = 0;
                    yield return new WaitForSecondsRealtime(1); //pause
                    genie.masterVolume = amp;
                    yield return new WaitForSecondsRealtime(1); //stimulation
                    genie.masterVolume = 0;

                    askForAnswer("Did you feel something?");
                    yield return new WaitUntil(() => !wait); //wait for answer
                    if (answer && amp < threshold) threshold = amp;
                }
            }
            if (genie.useSinusAudioWave) currentTest.amplitudeDetectionThresholdsSine.Add(frequency, threshold);
            if (genie.useSquareAudioWave) currentTest.amplitudeDetectionThresholdsSquare.Add(frequency, threshold);
            if (genie.useSawAudioWave) currentTest.amplitudeDetectionThresholdsSaw.Add(frequency, threshold);
        }

    }



    IEnumerator amplitudeDiscrimination() {
        genie.loadPreset(resetPreset);
        genie.useSinusAudioWave = true;
        genie.sinusAudioWaveIntensity = 1;
        yield return StartCoroutine(amplitudeDiscriminationSequence());

        if (varySignal) {
            genie.loadPreset(resetPreset);
            genie.useSquareAudioWave = true;
            genie.squareAudioWaveIntensity = 1;
            yield return StartCoroutine(amplitudeDiscriminationSequence());

            genie.loadPreset(resetPreset);
            genie.useSawAudioWave = true;
            genie.sawAudioWaveIntensity = 1;
            yield return StartCoroutine(amplitudeDiscriminationSequence());
        }
    }
    IEnumerator amplitudeDiscriminationSequence() {
        //PREPING SEQUENCE
        Debug.Log("Prepping sequence");
        //Amp
        List<float> sequence = new List<float>();
        for (float amplitude = 0; amplitude <= AMP_MAX; amplitude += AMP_SUBDIVISION) sequence.Add(amplitude); //generating test values
        System.Random rand = new System.Random();
        sequence = sequence.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = FREQ_MIN; frequency <= FREQ_MAX; frequency += FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(200);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //PREPING GENERATOR

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;

            foreach (float amp in sequence) {
                Debug.Log("Amp " + amp + " freq " + frequency);
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(1); //pause
                genie.masterVolume = amp;
                yield return new WaitForSecondsRealtime(1); //stimulation
                genie.masterVolume = 0;
                yield return new WaitUntil(() => !wait); //wait for answer
                
            }
            
        }

    }

    IEnumerator FrequencyDiscrimination() {
        yield return null;
    }
    IEnumerator FrequencyDiscriminationSequence() {
        yield return null;
    }

    IEnumerator TemporalDiscrimination() {
        yield return null;
    }
    IEnumerator TemporalDiscriminationSequence() {
        yield return null;
    }
}


public class TestResults {
    public enum Finger { thumb, index, middle, ring, little, other };
    public enum Placement { left, right, pad, other };

    public int age;
    public bool mainHand;
    public Finger finger;
    public Placement placement;
    public int timeInMonths;

    public Dictionary<float, float> amplitudeDetectionThresholdsSine = new Dictionary<float, float>();
    public Dictionary<float, float> amplitudeDetectionThresholdsSquare = new Dictionary<float, float>();
    public Dictionary<float, float> amplitudeDetectionThresholdsSaw = new Dictionary<float, float>();
}

public class AmpDetSet {
    public AmpDetSet(float frequency, float amplitude, bool answer) {
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.answer = answer;
    }
    public float frequency;
    public float amplitude;
    public bool answer;
}
