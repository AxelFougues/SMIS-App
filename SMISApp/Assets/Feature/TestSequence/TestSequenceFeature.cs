using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestSequenceFeature : MonoBehaviour{
    [HideInInspector]
    public TestResults currentTest;

    public GameObject resultsField;
    public GameObject overlay;
    public Alert answerAlertFirstSecond;
    public Alert answerAlertYesNo;
    public TMP_Text intervalNumber;

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

    int breakItterations = 0;

    bool ampDet;
    bool ampDis;
    bool freqDis;
    bool tempDis;

    bool varyFreq;
    bool varySignal;

    const int BREAK_ITERRATIONS = 20;
    const float DEFAULT_FREQUENCY = 100f;
    //AmpDet
    const float AMP_DET_AMP_SUBDIVISION = 0.005f;
    const float AMP_DET_AMP_MAX = 0.1f;
    const float AMP_DET_AMP_MIN = 0.0f;

    const float AMP_DET_FREQ_MIN = 40f;
    const float AMP_DET_FREQ_MAX = 250f;
    const float AMP_DET_FREQ_SUBDIVISION = 25f;
    //AmpDis
    const float AMP_DIS_AMP_SUBDIVISION = 0.005f;
    const float AMP_DIS_AMP_MAX = 0.1f;
    const float AMP_DIS_AMP_MIN = 0.0f;

    const float AMP_DIS_FREQ_MIN = 40f;
    const float AMP_DIS_FREQ_MAX = 250f;
    const float AMP_DIS_FREQ_SUBDIVISION = 20f;

    const float AMP_DIS_AMP_STEP_MIN = 0.0f;
    const float AMP_DIS_AMP_STEP_MAX = 0.1f;
    const float AMP_DIS_AMP_STEP_SUBDIVISION = 0.01f;
    //FreqDis
    const float FREQ_DIS_FREQ_MIN = 40f;
    const float FREQ_DIS_FREQ_MAX = 250f;
    const float FREQ_DIS_FREQ_SUBDIVISION = 20f;

    const float FREQ_DIS_FREQ_STEP_MIN = 0f;
    const float FREQ_DIS_FREQ_STEP_MAX = 20f;
    const float FREQ_DIS_FREQ_STEP_SUBDIVISION = 4f;
    //TempDis
    const float TEMP_DIS_FREQ_MIN = 40f;
    const float TEMP_DIS_FREQ_MAX = 250f;
    const float TEMP_DIS_FREQ_SUBDIVISION = 20f;

    const float TEMP_DIS_TEMP_STEP_MIN = 0f;
    const float TEMP_DIS_TEMP_STEP_MAX = 1f;
    const float TEMP_DIS_TEMP_STEP_SUBDIVISION = 0.2f;

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
        calcTime();
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
        //AmpDet
        if (currentTest.amplitudeDetectionThresholdsSine.Count > 0) {
            writeResultsTitle("Amplitude detection: Sine");
            SortedDictionary<float,float> results = currentTest.getAmpDetResults("sine");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, Th = " + results[key] + "amp");
        }
        if (currentTest.amplitudeDetectionThresholdsSquare.Count > 0) {
            writeResultsTitle("Amplitude detection: Square");
            SortedDictionary<float, float> results = currentTest.getAmpDetResults("square");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, Th = " + results[key] + "amp");
        }
        if (currentTest.amplitudeDetectionThresholdsSaw.Count > 0) {
            writeResultsTitle("Amplitude detection: Saw");
            SortedDictionary<float, float> results = currentTest.getAmpDetResults("saw");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, Th = " + results[key] + "amp");
        }
        //AmpDis
        if (currentTest.amplitudeDiscriminationSine.Count > 0) {
            writeResultsTitle("Amplitude discrimination: Sine");
            SortedDictionary<float, SortedDictionary<float, float>> results = currentTest.getAmpDisResults("sine");
            foreach (float frequency in results.Keys) {
                if (results[frequency].Count > 0) {
                    writeResultsLine("Results @ " + frequency + " Hz = ");
                    foreach(float amplitude in results[frequency].Keys) writeResultsLine("At " + amplitude + " amp, Δamp = " + results[frequency][amplitude] + " amp");
                }
            }
        }
        if (currentTest.amplitudeDiscriminationSquare.Count > 0) {
            writeResultsTitle("Amplitude discrimination: Square");
            SortedDictionary<float, SortedDictionary<float, float>> results = currentTest.getAmpDisResults("square");
            foreach (float frequency in results.Keys) {
                if (results[frequency].Count > 0) {
                    writeResultsLine("Results @ " + frequency + " Hz = ");
                    foreach (float amplitude in results[frequency].Keys) writeResultsLine("At @ " + amplitude + " amp, Δamp = " + results[frequency][amplitude] + " amp");
                }
            }
        }
        if (currentTest.amplitudeDiscriminationSaw.Count > 0) {
            writeResultsTitle("Amplitude discrimination: saw");
            SortedDictionary<float, SortedDictionary<float, float>> results = currentTest.getAmpDisResults("saw");
            foreach (float frequency in results.Keys) {
                if (results[frequency].Count > 0) {
                    writeResultsLine("Results @ " + frequency + " Hz = ");
                    foreach (float amplitude in results[frequency].Keys) writeResultsLine("At @ " + amplitude + " amp, Δamp = " + results[frequency][amplitude] + " amp");
                }
            }
        }
        //FreqDis
        if (currentTest.frequencyDiscriminationSine.Count > 0) {
            writeResultsTitle("Frequency Discrimination: Sine");
            SortedDictionary<float, float> results = currentTest.getFreqDisResults("sine");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, ΔHz = " + results[key] + " Hz");
        }
        if (currentTest.frequencyDiscriminationSquare.Count > 0) {
            writeResultsTitle("Frequency Discrimination: Square");
            SortedDictionary<float, float> results = currentTest.getFreqDisResults("square");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, ΔHz = " + results[key] + " Hz");
        }
        if (currentTest.frequencyDiscriminationSaw.Count > 0) {
            writeResultsTitle("Frequency Discrimination: Saw");
            SortedDictionary<float, float> results = currentTest.getFreqDisResults("saw");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, ΔHz = " + results[key] + " Hz");
        }
        //TempDis
        if (currentTest.temporalDiscriminationSine.Count > 0) {
            writeResultsTitle("Temporal Discrimination: Sine");
            SortedDictionary<float, float> results = currentTest.getFreqDisResults("sine");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, Δt = " + results[key] + " sec");
        }
        if (currentTest.temporalDiscriminationSquare.Count > 0) {
            writeResultsTitle("Temporal Discrimination: Square");
            SortedDictionary<float, float> results = currentTest.getFreqDisResults("square");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, Δt = " + results[key] + " sec");
        }
        if (currentTest.temporalDiscriminationSaw.Count > 0) {
            writeResultsTitle("Temporal Discrimination: Saw");
            SortedDictionary<float, float> results = currentTest.getFreqDisResults("saw");
            foreach (float key in results.Keys) writeResultsLine("At " + key + " Hz, Δt = " + results[key] + " sec");
        }
    }

    void calcTime() {
        calculatedTime = 0;
        if (ampDet) calculatedTime += ((AMP_DET_AMP_MAX - AMP_DET_AMP_MIN) / AMP_DET_AMP_SUBDIVISION) * (varyFreq ? (AMP_DET_FREQ_MAX - AMP_DET_FREQ_MIN) / AMP_DET_FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 4;
        //for ampDetplusminus : ITERATIONS * (varyFreq ? (FREQ_MAX - FREQ_MIN) / FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 3;
        if (ampDis) calculatedTime += ((AMP_DIS_AMP_STEP_MAX - AMP_DIS_AMP_STEP_MIN) / AMP_DIS_AMP_STEP_SUBDIVISION) * ((AMP_DIS_AMP_MAX - AMP_DIS_AMP_MIN) / AMP_DIS_AMP_SUBDIVISION) * (varyFreq ? (AMP_DIS_FREQ_MAX - AMP_DIS_FREQ_MIN) / AMP_DIS_FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * 4;
        if (freqDis) calculatedTime += ((FREQ_DIS_FREQ_STEP_MAX - FREQ_DIS_FREQ_STEP_MIN) / FREQ_DIS_FREQ_STEP_SUBDIVISION) * ((FREQ_DIS_FREQ_MAX - FREQ_DIS_FREQ_MIN) / FREQ_DIS_FREQ_SUBDIVISION) * (varySignal ? 3 : 1) * 4;
        if (tempDis) calculatedTime += ((TEMP_DIS_TEMP_STEP_MAX - TEMP_DIS_TEMP_STEP_MIN) / TEMP_DIS_TEMP_STEP_SUBDIVISION) * (varyFreq ? (TEMP_DIS_FREQ_MAX - TEMP_DIS_FREQ_MIN) / TEMP_DIS_FREQ_SUBDIVISION : 1) * (varySignal ? 3 : 1) * (4 + (TEMP_DIS_TEMP_STEP_MIN + (TEMP_DIS_TEMP_STEP_MAX - TEMP_DIS_TEMP_STEP_MIN) / 2f));
        float hours = calculatedTime / 3600f;
        float seconds = calculatedTime % 60f;
        float minutes = (calculatedTime % 3600f) / 60f;
        
        estimatedTime.text = "Estimated test duration: " + (hours >= 10 ? ""+(int)hours : "0"+(int)hours) + ":" + (minutes >= 10 ? "" + (int)minutes : "0" + (int)minutes) + ":" + (seconds >= 10 ? "" + (int)seconds : "0" + (int)seconds);
    }

    public void launch() {
        Debug.Log("Launching");
        if(ampDet || ampDis || freqDis ||tempDis) StartCoroutine(doSequences());
    }

    IEnumerator takeABreak() {
        if (breakItterations == BREAK_ITERRATIONS) {
            askForAnswerYesNo("Feel free to take a short break now. When you are ready to continue press Yes or No to resume.");
            breakItterations = 0;
            yield return new WaitUntil(() => !wait); //wait for answer
        } else breakItterations++;
    }

    IEnumerator doSequences() {
        genie.enabled = true;
        genie.loadPreset(resetPreset);
        overlay.SetActive(true);
        breakItterations = 0;
        intervalNumber.text = "";
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
        for (float amplitude = AMP_DET_AMP_MIN; amplitude <= AMP_DET_AMP_MAX; amplitude += AMP_DET_AMP_SUBDIVISION) amps.Add(amplitude); //generating test values
        System.Random rand = new System.Random();
        amps = amps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = AMP_DET_FREQ_MIN; frequency <= AMP_DET_FREQ_MAX; frequency += AMP_DET_FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(DEFAULT_FREQUENCY);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //PREPING GENERATOR

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            foreach (float amp in amps) {
                bool order = Random.Range(0f, 1f) > 0.5f;

                intervalNumber.text = "";
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                intervalNumber.text = "1";
                genie.masterVolume = order? amp : 0;
                yield return new WaitForSecondsRealtime(1); //T1

                intervalNumber.text = "";
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                intervalNumber.text = "2";
                genie.masterVolume = order ? 0 : amp ;
                yield return new WaitForSecondsRealtime(1); //T2

                intervalNumber.text = "";
                genie.masterVolume = 0;
                askForAnswerFirstSecond("Which interval had the highest amplitude?");
                yield return new WaitUntil(() => !wait); //wait for answer
                //save
                if (genie.useSinusAudioWave) currentTest.amplitudeDetectionThresholdsSine.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                if (genie.useSquareAudioWave) currentTest.amplitudeDetectionThresholdsSquare.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                if (genie.useSawAudioWave) currentTest.amplitudeDetectionThresholdsSaw.Add(new AmpDetSet(frequency, amp, answerCheck(order)));
                //break
                yield return StartCoroutine(takeABreak());
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
        for (float amplitude = AMP_DIS_AMP_MIN; amplitude <= AMP_DIS_AMP_MAX; amplitude += AMP_DIS_AMP_SUBDIVISION) amps.Add(amplitude); //generating test values
        amps = amps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //step
        List<float> steps = new List<float>();
        for (float step = AMP_DIS_AMP_STEP_MIN; step <= AMP_DIS_AMP_STEP_MAX; step += AMP_DIS_AMP_STEP_SUBDIVISION) steps.Add(step); //generating test values
        steps = steps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = AMP_DIS_FREQ_MIN; frequency <= AMP_DIS_FREQ_MAX; frequency += AMP_DIS_FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(DEFAULT_FREQUENCY);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            foreach (float amp in amps) {
                foreach (float step in steps) {
                    bool order = Random.Range(0f, 1f) > 0.5f;

                    intervalNumber.text = "";
                    genie.masterVolume = 0;
                    yield return new WaitForSecondsRealtime(0.5f);//pause

                    intervalNumber.text = "1";
                    genie.masterVolume = order ? amp + step : amp;
                    yield return new WaitForSecondsRealtime(1); //T1

                    intervalNumber.text = "";
                    genie.masterVolume = 0;
                    yield return new WaitForSecondsRealtime(0.5f);//pause

                    intervalNumber.text = "2";
                    genie.masterVolume = order ? amp : amp + step;
                    yield return new WaitForSecondsRealtime(1); //T2

                    intervalNumber.text = "";
                    genie.masterVolume = 0;
                    askForAnswerFirstSecond("Which interval had the highest amplitude?");
                    yield return new WaitUntil(() => !wait); //wait for answer
                                                             //save
                    if (genie.useSinusAudioWave) currentTest.amplitudeDiscriminationSine.Add(new AmpDisSet(frequency, amp, step, answerCheck(order)));
                    if (genie.useSquareAudioWave) currentTest.amplitudeDiscriminationSquare.Add(new AmpDisSet(frequency, amp, step, answerCheck(order)));
                    if (genie.useSawAudioWave) currentTest.amplitudeDiscriminationSaw.Add(new AmpDisSet(frequency, amp, step, answerCheck(order)));

                    //break
                    yield return StartCoroutine(takeABreak());
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
        for (float step = FREQ_DIS_FREQ_STEP_MIN; step <= FREQ_DIS_FREQ_STEP_MAX; step += FREQ_DIS_FREQ_STEP_SUBDIVISION) steps.Add(step);
        steps = steps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        for (float frequency = FREQ_DIS_FREQ_MIN; frequency <= FREQ_DIS_FREQ_MAX; frequency += FREQ_DIS_FREQ_SUBDIVISION) frequencies.Add(frequency); 
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order

        //SEQUENCE
        foreach (float frequency in frequencies) {
            genie.mainFrequency = frequency;
            foreach (float step in steps) {
                bool order = Random.Range(0f, 1f) > 0.5f;

                intervalNumber.text = "";
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                intervalNumber.text = "1";
                genie.masterVolume = 1;
                genie.masterVolume = order ? frequency+step : frequency;
                yield return new WaitForSecondsRealtime(1); //T1

                intervalNumber.text = "";
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                intervalNumber.text = "2";
                genie.masterVolume = 1;
                genie.masterVolume = order ? frequency : frequency+step;
                yield return new WaitForSecondsRealtime(1); //T2

                intervalNumber.text = "";
                genie.masterVolume = 0;
                askForAnswerFirstSecond("Which interval had the highest amplitude?");
                yield return new WaitUntil(() => !wait); //wait for answer
                //save
                if (genie.useSinusAudioWave) currentTest.frequencyDiscriminationSine.Add(new SteppedSet(frequency, step, answerCheck(order)));
                if (genie.useSquareAudioWave) currentTest.frequencyDiscriminationSquare.Add(new SteppedSet(frequency, step, answerCheck(order)));
                if (genie.useSawAudioWave) currentTest.frequencyDiscriminationSaw.Add(new SteppedSet(frequency, step, answerCheck(order)));

                //break
                yield return StartCoroutine(takeABreak());
            }
        }
    }


    IEnumerator TemporalDiscrimination() {
        genie.loadPreset(resetPreset);
        genie.useSinusAudioWave = true;
        genie.sinusAudioWaveIntensity = 1;
        yield return StartCoroutine(TemporalDiscriminationSequence());

        if (varySignal) {
            genie.loadPreset(resetPreset);
            genie.useSquareAudioWave = true;
            genie.squareAudioWaveIntensity = 1;
            yield return StartCoroutine(TemporalDiscriminationSequence());

            genie.loadPreset(resetPreset);
            genie.useSawAudioWave = true;
            genie.sawAudioWaveIntensity = 1;
            yield return StartCoroutine(TemporalDiscriminationSequence());
        }
    }
    IEnumerator TemporalDiscriminationSequence() {
        Debug.Log("Prepping sequence");
        System.Random rand = new System.Random();
        //step
        List<float> steps = new List<float>();
        for (float step = TEMP_DIS_TEMP_STEP_MIN; step <= TEMP_DIS_TEMP_STEP_MAX; step += TEMP_DIS_TEMP_STEP_SUBDIVISION) steps.Add(step); //generating test values
        steps = steps.OrderBy(x => rand.Next()).ToList(); //randomizing order
        //frequencies
        List<float> frequencies = new List<float>();
        if (varyFreq) for (float frequency = TEMP_DIS_FREQ_MIN; frequency <= TEMP_DIS_FREQ_MAX; frequency += TEMP_DIS_FREQ_SUBDIVISION) frequencies.Add(frequency); //generating frequency steps
        else frequencies.Add(DEFAULT_FREQUENCY);
        frequencies = frequencies.OrderBy(x => rand.Next()).ToList(); //randomizing order

        //SEQUENCE
        foreach (float frequency in frequencies) {
            foreach (float step in steps) {
                genie.mainFrequency = frequency;
                bool order = Random.Range(0f, 1f) > 0.5f;

                intervalNumber.text = "";
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                intervalNumber.text = "1";
                genie.masterVolume = 1;
                yield return new WaitForSecondsRealtime(1f + (order ? step : 0f)); //T1

                intervalNumber.text = "";
                genie.masterVolume = 0;
                yield return new WaitForSecondsRealtime(0.5f);//pause

                intervalNumber.text = "2";
                genie.masterVolume = 1;
                yield return new WaitForSecondsRealtime(1f + (order ? 0f : step)); //T2

                intervalNumber.text = "";
                genie.masterVolume = 0;
                askForAnswerFirstSecond("Which interval was the longest?");
                yield return new WaitUntil(() => !wait); //wait for answer
                                                         //save
                if (genie.useSinusAudioWave) currentTest.temporalDiscriminationSine.Add(new SteppedSet(frequency, step, answerCheck(order)));
                if (genie.useSquareAudioWave) currentTest.temporalDiscriminationSquare.Add(new SteppedSet(frequency, step, answerCheck(order)));
                if (genie.useSawAudioWave) currentTest.temporalDiscriminationSaw.Add(new SteppedSet(frequency, step, answerCheck(order)));
                //break
                yield return StartCoroutine(takeABreak());
            }
        }
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

    public List<AmpDisSet> amplitudeDiscriminationSine = new List<AmpDisSet>();
    public List<AmpDisSet> amplitudeDiscriminationSquare = new List<AmpDisSet>();
    public List<AmpDisSet> amplitudeDiscriminationSaw = new List<AmpDisSet>();

    public List<SteppedSet> frequencyDiscriminationSine = new List<SteppedSet>();
    public List<SteppedSet> frequencyDiscriminationSquare = new List<SteppedSet>();
    public List<SteppedSet> frequencyDiscriminationSaw = new List<SteppedSet>();

    public List<SteppedSet> temporalDiscriminationSine = new List<SteppedSet>();
    public List<SteppedSet> temporalDiscriminationSquare = new List<SteppedSet>();
    public List<SteppedSet> temporalDiscriminationSaw = new List<SteppedSet>();
                            
                           //freq  threshold
    public SortedDictionary<float, float> getAmpDetResults(string signalType) {
        SortedDictionary<float, float> results = new SortedDictionary<float, float>();
        List<AmpDetSet> list = new List<AmpDetSet>();
        switch (signalType) {
            case "sine": list = amplitudeDetectionThresholdsSine; break;
            case "square": list = amplitudeDetectionThresholdsSquare; break;
            case "saw": list = amplitudeDetectionThresholdsSaw; break;
        }
        foreach (AmpDetSet ampDetSet in list) {
            if (ampDetSet.answer) {
                if (!results.ContainsKey(ampDetSet.frequency)) results.Add(ampDetSet.frequency, ampDetSet.amplitude);
                else if (ampDetSet.amplitude < results[ampDetSet.frequency]) results[ampDetSet.frequency] = ampDetSet.amplitude;
            }
        }
        return results;
    }
                          //freq                 amplitude step
    public SortedDictionary<float, SortedDictionary<float, float>> getAmpDisResults(string signalType) {
        SortedDictionary<float, SortedDictionary<float, float>> results = new SortedDictionary<float, SortedDictionary<float, float>>();
        List<AmpDisSet> list = new List<AmpDisSet>();
        switch (signalType) {
            case "sine": list = amplitudeDiscriminationSine; break;
            case "square": list = amplitudeDiscriminationSquare; break;
            case "saw": list = amplitudeDiscriminationSaw; break;
        }
        foreach (AmpDisSet ampDisSet in list) {
            if (ampDisSet.answer) {
                if (!results.ContainsKey(ampDisSet.frequency)) {
                    results.Add(ampDisSet.frequency, new SortedDictionary<float, float>());
                    results[ampDisSet.frequency].Add(ampDisSet.amplitude, ampDisSet.step);
                } else if (!results[ampDisSet.frequency].ContainsKey(ampDisSet.amplitude)) {
                    results[ampDisSet.frequency].Add(ampDisSet.amplitude, ampDisSet.step);
                } else if (ampDisSet.step < results[ampDisSet.frequency][ampDisSet.amplitude]) {
                    results[ampDisSet.frequency][ampDisSet.amplitude] = ampDisSet.step;
                }
            }
        }
        return results;
    }
                          //freq    step 
    public SortedDictionary<float, float> getFreqDisResults(string signalType) {
        SortedDictionary<float, float> results = new SortedDictionary<float, float>();
        List<SteppedSet> list = new List<SteppedSet>();
        switch (signalType) {
            case "sine": list = frequencyDiscriminationSine; break;
            case "square": list = frequencyDiscriminationSquare; break;
            case "saw": list = frequencyDiscriminationSaw; break;
        }
        foreach (SteppedSet steppedSet in list) {
            if (steppedSet.answer) {
                if (!results.ContainsKey(steppedSet.frequency)) results.Add(steppedSet.frequency, steppedSet.step);
                else if (steppedSet.step < results[steppedSet.frequency]) results[steppedSet.frequency] = steppedSet.step;
            }
        }
        return results;
    }
                          //freq    step
    public SortedDictionary<float, float> getTempDisResults(string signalType) {
        SortedDictionary<float, float> results = new SortedDictionary<float, float>();
        List<SteppedSet> list = new List<SteppedSet>();
        switch (signalType) {
            case "sine": list = temporalDiscriminationSine; break;
            case "square": list = temporalDiscriminationSquare; break;
            case "saw": list = temporalDiscriminationSaw; break;
        }
        foreach (SteppedSet steppedSet in list) {
            if (steppedSet.answer) {
                if (!results.ContainsKey(steppedSet.frequency)) results.Add(steppedSet.frequency, steppedSet.step);
                else if (steppedSet.step < results[steppedSet.frequency]) results[steppedSet.frequency] = steppedSet.step;
            }
        }
        return results;
    }

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

public class AmpDisSet {
    public AmpDisSet(float frequency, float amplitude, float step, bool answer) {
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.step = step;
        this.answer = answer;
    }
    public float frequency;
    public float amplitude;
    public float step;
    public bool answer;
}

public class SteppedSet {
    public SteppedSet(float frequency, float step, bool answer) {
        this.frequency = frequency;
        this.step = step;
        this.answer = answer;
    }
    public float frequency;
    public float step;
    public bool answer;
}
