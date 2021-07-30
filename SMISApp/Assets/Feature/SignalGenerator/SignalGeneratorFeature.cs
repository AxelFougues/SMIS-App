using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SignalGeneratorFeature : MonoBehaviour{

    [Header("Links")]
    
    public SignalPreset defaultSignal;

    public TMP_InputField frequencyField;
    public Slider frequencySlider;
    public TMP_Text minFreqText;
    public TMP_Text maxFreqText;
    public Toggle playToggle;
    public Slider stereoSlider;
    public Toggle LToggle;
    public Toggle LRToggle;
    public Toggle RToggle;
    public Toggle SineToggle;
    public Toggle SquareToggle;
    public Toggle SawToggle;
    public Slider SineSlider;
    public Slider SquareSlider;
    public Slider SawSlider;
    public GameObject eqContainer;
    public GameObject freqMod;
    public GameObject ampMod;

    public GameObject EQRow;

    public AnimationCurve eq = new AnimationCurve();

    GameObject audioOut;
    SignalGenerator left;
    SignalGenerator right;
    SignalGenerator stereo;
    SignalGenerator current;

    List<Slider> eqValues;
    List<TMP_Text> eqNotes;

    private void Start() {

        Events.current.onSettingsChanged += onSettingsChanged;
        Events.current.onThemeChanged += onThemeChanged;

        audioOut = GameObject.FindGameObjectWithTag("AudioOut");

        left = audioOut.GetComponents<SignalGenerator>()[0];
        right = audioOut.GetComponents<SignalGenerator>()[1];
        stereo = audioOut.GetComponents<SignalGenerator>()[2];

        refreshEQ();

        reset();
    }

    public void reset() {
        left.loadPreset(defaultSignal);
        right.loadPreset(defaultSignal);
        stereo.loadPreset(defaultSignal);
        left.mainFrequency = Global.current.settings.minFrequency;
        right.mainFrequency = Global.current.settings.minFrequency;
        stereo.mainFrequency = Global.current.settings.minFrequency;
        playToggle.isOn = false;
        resetStereo();
    }

    public void updateVisuals() {
        updateFrequencyField();
        updateFrequencySlider();
        updatePlayVisuals();
        updateEQVisuals();
        updateModulationVisuals();
    }

    //FREQUENCY######################################################################################################

    public void incrementFrequency() {
        if(current.mainFrequency < Global.current.settings.maxFrequency) current.mainFrequency++;
        updateVisuals();
    }

    public void decrementFrequency() {
        if (current.mainFrequency > Global.current.settings.minFrequency)  current.mainFrequency--;
        updateVisuals();
    }

    public void devideFrequency() {
        if ((int)(current.mainFrequency / 2) >= Global.current.settings.minFrequency) current.mainFrequency = (int)(current.mainFrequency / 2);
        updateVisuals();
    }

    public void multiplyFrequency() {
        if ((int)(current.mainFrequency * 2) <= Global.current.settings.maxFrequency) current.mainFrequency = (int)(current.mainFrequency * 2);
        updateVisuals();
    }

    public void changeFrequencyFromField() {
        string value = frequencyField.text;
        float frequency = 0;
        if (float.TryParse(value, out frequency) && isBetween(frequency, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) {
            current.mainFrequency = frequency;
            useEQ();
        }
        updateFrequencySlider();
    }

    public void changeFrequencyFromSlider() {
        float frequency = frequencySlider.value;
        if (isBetween(frequency, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) {
            current.mainFrequency = Mathf.Round(frequency * 100f) / 100f;
            useEQ();
        }
        updateFrequencyField();
    }

    public void updateFrequencyField() {
        frequencyField.text = current.mainFrequency.ToString();
    }

    public void updateFrequencySlider() {
        frequencySlider.minValue = Global.current.settings.minFrequency;
        minFreqText.text = Global.current.settings.minFrequency.ToString();
        frequencySlider.maxValue = Global.current.settings.maxFrequency;
        maxFreqText.text = Global.current.settings.maxFrequency.ToString();
        frequencySlider.value = (float)current.mainFrequency;
    }

    //STEREO##########################################################################################################

    public void changeStereo() {
        if (stereoSlider.value < 0.5f) {
            right.masterVolume = mapValue(stereoSlider.value, 0, 0.5f, 0, 1);
            left.masterVolume = 1;
        } else if (stereoSlider.value > 0.5f) {
            left.masterVolume = mapValue(stereoSlider.value, 0.5f, 1, 1, 0);
            right.masterVolume = 1;
        } else {
            left.masterVolume = 1;
            right.masterVolume = 1;
        }
        stereo.stereoPan = mapValue(stereoSlider.value, 0, 1, -1, 1);
    }

    public void resetStereo() {
        setStereoMode(1);
        RToggle.isOn = LToggle.isOn = false;
        LRToggle.isOn = true;
        stereoSlider.value = 0.5f;
        changeStereo();
    }

    public void setStereoMode(int mode) {//0 Left, 1 stereo, 2 right   
        switch (mode) {
            case 0:
                stereo.enabled = false;
                current = left;
                break;
            case 1:
                right.enabled = false;
                left.enabled = false;
                current = stereo;
                break;
            case 2:
                stereo.enabled = false;
                current = right;
                break;
        }
        useEQ();
        updateVisuals();
    }

    //PLAY/PAUSE########################################################################################################

    public void play() {
        current.enabled = playToggle.isOn;
    }

    public void updatePlayVisuals() {
        playToggle.isOn = current.enabled;
        SineToggle.isOn = current.useSinusAudioWave;
        SquareToggle.isOn = current.useSquareAudioWave;
        SawToggle.isOn = current.useSawAudioWave;
        SineSlider.value = current.sinusAudioWaveIntensity;
        SquareSlider.value = current.squareAudioWaveIntensity;
        SawSlider.value = current.sawAudioWaveIntensity;
    }

    public void changeWaves() {
        current.useSinusAudioWave = SineToggle.isOn;
        current.useSquareAudioWave = SquareToggle.isOn;
        current.useSawAudioWave = SawToggle.isOn;
        current.sinusAudioWaveIntensity = SineSlider.value;
        current.squareAudioWaveIntensity = SquareSlider.value;
        current.sawAudioWaveIntensity = SawSlider.value;
    }

    public void changeEQ(float e) {
        for (int i = 0; i < Global.current.settings.EQSteps; i++) eq.MoveKey(i, new Keyframe(eq[i].time, eqValues[i].value));
        useEQ();
    }

    public void updateEQVisuals() {  
        for (int i = 0; i < Global.current.settings.EQSteps; i++) eqValues[i].value = eq[i].value;
        for (int i = 0; i < Global.current.settings.EQSteps; i++) eqNotes[i].text = ""+ (int)mapValue(eq[i].time, 0, 1, Global.current.settings.minFrequency, Global.current.settings.maxFrequency);
    }

    public void useEQ() {
        current.masterVolume = eq.Evaluate(mapValue((float)current.mainFrequency, Global.current.settings.minFrequency, Global.current.settings.maxFrequency, 0, 1));
    }

    public void refreshEQ() {
        while(eqContainer.transform.childCount > 0) GameObject.DestroyImmediate(eqContainer.transform.GetChild(0).gameObject);
        Debug.Log(eqContainer.transform.childCount);
        for (int i = 0; i < Global.current.settings.EQSteps; i++) {
            GameObject row = Instantiate(EQRow, eqContainer.transform);
            Slider s = row.GetComponentInChildren<Slider>();
            s.onValueChanged.AddListener(changeEQ);
        }
        eqValues = eqContainer.GetComponentsInChildren<Slider>().ToList();
        eqNotes = eqContainer.GetComponentsInChildren<TMP_Text>().ToList();
        Debug.Log(eqContainer.transform.childCount);
        eq = new AnimationCurve();
        for (float i = 0f; i <= 1f; i += 1f / Global.current.settings.EQSteps) eq.AddKey(i, 1f);
    }


    //MODULATION#############################################################################################

    public void changeModulation() {
        bool correctFrequencies = false;
        //FREQ
        current.useFrequencyModulation = freqMod.GetComponentInChildren<Toggle>().isOn;
        int freqFreq;
        if (!int.TryParse(freqMod.GetComponentInChildren<TMP_InputField>().text, out freqFreq) || !isBetween(freqFreq, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) {
            freqFreq = (int)Global.current.settings.minFrequency;
            correctFrequencies = true;
        }
        current.frequencyModulationOscillatorFrequency =  freqFreq;
        current.frequencyModulationOscillatorIntensity = freqMod.GetComponentInChildren<Slider>().value;

        //AMP
        current.useAmplitudeModulation = ampMod.GetComponentInChildren<Toggle>().isOn;
        int ampFreq;
        if (!int.TryParse(ampMod.GetComponentInChildren<TMP_InputField>().text, out ampFreq) || !isBetween(ampFreq, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) {
            ampFreq = (int)Global.current.settings.minFrequency;
            correctFrequencies = true;
        }
        current.amplitudeModulationOscillatorFrequency = ampFreq;

        if (correctFrequencies) updateModulationVisuals();
    }

    public void updateModulationVisuals() {
        //FREQ
        freqMod.GetComponentInChildren<Toggle>().isOn = current.useFrequencyModulation;
        if (!isBetween(current.frequencyModulationOscillatorFrequency, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) current.frequencyModulationOscillatorFrequency = Global.current.settings.minFrequency;
        freqMod.GetComponentInChildren<TMP_InputField>().text = "" + (int)current.frequencyModulationOscillatorFrequency;
        freqMod.GetComponentInChildren<Slider>().value = current.frequencyModulationOscillatorIntensity;

        //AMP
        ampMod.GetComponentInChildren<Toggle>().isOn = current.useAmplitudeModulation;
        if (!isBetween(current.amplitudeModulationOscillatorFrequency, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) current.amplitudeModulationOscillatorFrequency = Global.current.settings.minFrequency;
        ampMod.GetComponentInChildren<TMP_InputField>().text = "" + (int)current.amplitudeModulationOscillatorFrequency;
    }

    //EVENTS##################################################################################################

    protected void onSettingsChanged() {
        if(!isBetween((float)left.mainFrequency, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) left.mainFrequency = Global.current.settings.minFrequency;
        if (!isBetween((float)right.mainFrequency, Global.current.settings.minFrequency, Global.current.settings.maxFrequency)) right.mainFrequency = Global.current.settings.minFrequency;
        refreshEQ();
        updateVisuals();
    }

    protected void onThemeChanged() {
        //TBD
    }


    private void OnDisable() {
        reset();
    }

    //UTILS#######################################################################################################

    bool isBetween(float testValue, float bound1, float bound2) {
        if (bound1 > bound2)
            return testValue >= bound2 && testValue <= bound1;
        return testValue >= bound1 && testValue <= bound2;
    }

    float mapValue(float referenceValue, float fromMin, float fromMax, float toMin, float toMax) {
        // This function maps (converts) a Float value from one range to another
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
}
