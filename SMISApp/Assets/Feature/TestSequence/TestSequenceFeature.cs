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
    public Alert answerAlertFirstSecond;
    public Alert answerAlertYesNo;

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
    public string answer = "";

    float calculatedTime = 0;

    bool ampDet;
    bool ampDis;
    bool freqDis;
    bool tempDis;

    bool varyFreq;
    bool varySignal;

    const float AMP_SUBDIVISION = 0.05f;
    const float AMP_MAX = 0.3f;
    const float AMP_MIN = 0.0f;
    const float DEFAULT_AMP = 0.15f;

    const float FREQ_MIN = 30;
    const float FREQ_MAX = 250;
    const float FREQ_SUBDIVISION = 10;
    const float DEFAULT_FREQUENCY = 100;

    const float AMP_STEP_MIN = 0.1f;
    const float AMP_STEP_MAX = 0.3f;
    const float AMP_STEP_SUBDIVISION = 0.1f;

    const float FREQ_STEP_MIN = 1;
    const float FREQ_STEP_MAX = 51;
    const float FREQ_STEP_SUBDIVISION = 5;

    const float TONE_FREQ = 300;

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
        answerAlertFirstSecond.gameObject.SetActive(false);
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

    public void answerFirst() { answer = "First"; wait = false; }
    public void answerSecond() { answer = "Second"; wait = false; }
    public void answerNeither() { answer = "Neither"; wait = false; }
    public void answerYes() { answer = "Yes"; wait = false; }
    public void answerNo() { answer = "No"; wait = false; }

    void clearResults() {
        currentTest = new TestResults();
        foreach (Transform child in resultsField.transform) Destroy(child.gameObject);
    }

    void askForAnswerFirstSecond(string question) {
        answerAlertFirstSecond.gameObject.SetActive(true);
        answerAlertFirstSecond.variableText.text = question;
        wait = true;
        answer = "";
    }

    void askForAnswerYesNo(string question) {
        answerAlertYesNo.gameObject.SetActive(true);
        answerAlertYesNo.variableText.text = question;
        wait = true;
        answer = "";
    }

    bool answerCheck(bool correct) {
        if (answer == "Neither") return false;
        return (answer == "First") == correct;
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
            //foreach(KeyValuePair<float, float> pair in currentTest.amplitudeDetectionThresholdsSine) writeResultsLine("Threshold @ " + pair.Key + " Hz = " + pair.Value);
        }
        if (currentTest.amplitudeDetectionThresholdsSquare.Count > 0) {
            writeResultsTitle("Amplitude detection: Square");
            //foreach (KeyValuePair<float, float> pair in currentTest.amplitudeDetectionThresholdsSquare) writeResultsLine("Threshold @ " + pair.Key + " Hz = " + pair.Value);
        }
        if (currentTest.amplitudeDetectionThresholdsSaw.Count > 0) {
            writeResultsTitle("Amplitude detection: Saw");
            //foreach (KeyValuePair<float, float> pair in currentTest.amplitudeDetectionThresholdsSaw) writeResultsLine("Threshold @ " + pair.Key + " Hz = " + pair.Value);
        }
    }

    void calcTime() {
        calculatedTime = 0;
        if (ampDet) calculatedTime += ((AMP_MAX - AMP_MIN) / AMP_SUBDIVISION) * (varyFreq ? (FREQ_MAX - FREQ_MIN) / FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 4;
        //for ampDetplusminus : ITERATIONS * (varyFreq ? (FREQ_MAX - FREQ_MIN) / FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 3;
        if (ampDis) calculatedTime += ((AMP_STEP_MAX - AMP_STEP_MIN) / AMP_STEP_SUBDIVISION) * ((AMP_MAX - AMP_MIN) / AMP_SUBDIVISION) * (varyFreq ? (FREQ_MAX - FREQ_MIN) / FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 4;
        if (freqDis) calculatedTime += ((FREQ_STEP_MAX - FREQ_STEP_MIN) / FREQ_STEP_SUBDIVISION) * ((FREQ_MAX - FREQ_MIN) / FREQ_SUBDIVISION) * (varySignal ? 3 : 1) * 4;
        estimatedTime.text = "Estimated test duration: " + Mathf.RoundToInt(calculatedTime/60) + " minutes";
    }

    public void launch() {
        Debug.Log("Launching");
        if(ampDet || ampDis || freqDis ||tempDis) StartCoroutine(doSequences());
    }

    IEnumerator doSequences() {
        genie.enabled = true;
        genie.loadPreset(resetPreset);
        overlay.SetActive(true);
        clearResults();

        //Start screen
        askForAnswerYesNo("Ready?");
        yield return new WaitUntil(() => !wait); //wait for answer
        if (answer == "Yes") {
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
    IEnumerator amplitudeDetectionSequence() {
        //PREPING SEQUENCE
        Debug.Log("Prepping sequence");
        //Amp
        List<float> amps = new List<float>();
        for (float amplitude = 0; amplitude <= AMP_MAX; amplitude += AMP_SUBDIVISION) amps.Add(amplitude); //generating test values
        System.Random rand = new System.Random();
        amps = amps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = FREQ_MIN; frequency <= FREQ_MAX; frequency += FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(DEFAULT_FREQUENCY);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //PREPING GENERATOR

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            foreach (float amp in amps) {
                bool order = Random.Range(0f, 1f) > 0.5f;

                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                genie.masterVolume = order? amp : 0;
                yield return new WaitForSecondsRealtime(1); //T1

                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                genie.masterVolume = order ? 0 : amp ;
                yield return new WaitForSecondsRealtime(1); //T2

                genie.masterVolume = 0;
                askForAnswerFirstSecond("Which interval had the highest amplitude?");
                yield return new WaitUntil(() => !wait); //wait for answer
                //save
                if (genie.useSinusAudioWave) currentTest.amplitudeDetectionThresholdsSine.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                if (genie.useSquareAudioWave) currentTest.amplitudeDetectionThresholdsSquare.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                if (genie.useSawAudioWave) currentTest.amplitudeDetectionThresholdsSaw.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
            }
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
        System.Random rand = new System.Random();
        //Amp
        List<float> amps = new List<float>();
        for (float amplitude = 0; amplitude <= AMP_MAX; amplitude += AMP_SUBDIVISION) amps.Add(amplitude); //generating test values
        amps = amps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //step
        List<float> steps = new List<float>();
        for (float step = AMP_STEP_MIN; step <= AMP_STEP_MAX; step += AMP_STEP_SUBDIVISION) amps.Add(step); //generating test values
        amps = amps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = FREQ_MIN; frequency <= FREQ_MAX; frequency += FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(DEFAULT_FREQUENCY);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            foreach (float amp in amps) {
                foreach (float step in steps) {
                    bool order = Random.Range(0f, 1f) > 0.5f;

                    genie.masterVolume = 0;
                    yield return new WaitForSecondsRealtime(0.5f);//pause

                    genie.masterVolume = order ? amp + step : amp;
                    yield return new WaitForSecondsRealtime(1); //T1

                    genie.masterVolume = 0;
                    yield return new WaitForSecondsRealtime(0.5f);//pause

                    genie.masterVolume = order ? amp : amp + step;
                    yield return new WaitForSecondsRealtime(1); //T2

                    genie.masterVolume = 0;
                    askForAnswerFirstSecond("Which interval had the highest amplitude?");
                    yield return new WaitUntil(() => !wait); //wait for answer
                                                             //save
                    if (genie.useSinusAudioWave) currentTest.amplitudeDetectionThresholdsSine.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                    if (genie.useSquareAudioWave) currentTest.amplitudeDetectionThresholdsSquare.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                    if (genie.useSawAudioWave) currentTest.amplitudeDetectionThresholdsSaw.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                }
            }
        }
    }

    IEnumerator FrequencyDiscrimination() {
        genie.loadPreset(resetPreset);
        genie.useSinusAudioWave = true;
        genie.sinusAudioWaveIntensity = 1;
        yield return StartCoroutine(FrequencyDiscriminationSequence());

        if (varySignal) {
            genie.loadPreset(resetPreset);
            genie.useSquareAudioWave = true;
            genie.squareAudioWaveIntensity = 1;
            yield return StartCoroutine(FrequencyDiscriminationSequence());

            genie.loadPreset(resetPreset);
            genie.useSawAudioWave = true;
            genie.sawAudioWaveIntensity = 1;
            yield return StartCoroutine(FrequencyDiscriminationSequence());
        }
    }
    IEnumerator FrequencyDiscriminationSequence() {
        Debug.Log("Prepping sequence");
        System.Random rand = new System.Random();
        //step
        List<float> steps = new List<float>();
        for (float step = FREQ_STEP_MIN; step <= FREQ_STEP_MAX; step += FREQ_STEP_SUBDIVISION) steps.Add(step); //generating test values
        steps = steps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        for (float frequency = FREQ_MIN; frequency <= FREQ_MAX; frequency += FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            foreach (float step in steps) {
                bool order = Random.Range(0f, 1f) > 0.5f;

                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                genie.masterVolume = 1;
                genie.masterVolume = order ? frequency+step : frequency;
                yield return new WaitForSecondsRealtime(1); //T1

                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                genie.masterVolume = 1;
                genie.masterVolume = order ? frequency : frequency+step;
                yield return new WaitForSecondsRealtime(1); //T2

                genie.masterVolume = 0;
                askForAnswerFirstSecond("Which interval had the highest amplitude?");
                yield return new WaitUntil(() => !wait); //wait for answer
                //save
                if (genie.useSinusAudioWave) currentTest.frequencyDiscriminationSine.Add(new FreqDisSet(frequency, step, answerCheck(order)));
                if (genie.useSquareAudioWave) currentTest.frequencyDiscriminationSquare.Add(new FreqDisSet(frequency, step, answerCheck(order)));
                if (genie.useSawAudioWave) currentTest.frequencyDiscriminationSaw.Add(new FreqDisSet(frequency, step, answerCheck(order)));
            }
        }
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

    public List<AmpDetSet> amplitudeDetectionThresholdsSine = new List<AmpDetSet>();
    public List<AmpDetSet> amplitudeDetectionThresholdsSquare = new List<AmpDetSet>();
    public List<AmpDetSet> amplitudeDetectionThresholdsSaw = new List<AmpDetSet>();

    public List<FreqDisSet> frequencyDiscriminationSine = new List<FreqDisSet>();
    public List<FreqDisSet> frequencyDiscriminationSquare = new List<FreqDisSet>();
    public List<FreqDisSet> frequencyDiscriminationSaw = new List<FreqDisSet>();
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

public class FreqDisSet {
    public FreqDisSet(float frequency, float step, bool answer) {
        this.frequency = frequency;
        this.step = step;
        this.answer = answer;
    }
    public float frequency;
    public float step;
    public bool answer;
}
