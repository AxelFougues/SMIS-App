using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TechTweaking.Bluetooth;


public class ConnexionStatus : MonoBehaviour{
    public enum Device { NONE, CUBE, WIRE };

    public Device connexion = Device.NONE;
    public string status = "";
    public string cubeBTName = "XY-P5W";

    public Animator visual;
    public TMP_Text visualStatus;

    float scanTimeout = 20;

    void Start(){
        setSatus("Tap to scan for device");
        refresh();
    }


    IEnumerator scanConnexion() {
        connexion = Device.NONE;
        Events.current.connexionStatusChanged();
        setSatus("Scanning for device");
        visual.SetTrigger("Scan");
        float timeout = Time.time;

        while (connexion == Device.NONE && Time.time - timeout < scanTimeout) {

            // if (DetectHeadset.CanDetect() && DetectHeadset.Detect()) connexion = Device.WIRE;
            BluetoothDevice[] paired = BluetoothAdapter.getPairedDevices();
            if(paired != null) foreach (BluetoothDevice pairedDevice in paired) if (pairedDevice!=null && pairedDevice.Name == cubeBTName) connexion = Device.CUBE;
            // BluetoothDevice device = new BluetoothDevice();
            //device.Name = cubeBTName;
            //device.connect();

            yield return new WaitForFixedUpdate();
        }
        switch (connexion) {
            case Device.CUBE:
                visual.SetTrigger("Cube");
                setSatus("SMIS CUBE");
                break;
            case Device.WIRE:
                visual.SetTrigger("Wire");
                setSatus("SMIS WIRE");
                break;
            case Device.NONE:
                visual.SetTrigger("Idle");
                setSatus("Tap to scan for device");
                break;
        }
        
        Events.current.connexionStatusChanged();
    }

    public void refresh() {
        StartCoroutine("scanConnexion");
    }

    public void setCube() {
        connexion = Device.CUBE;
        Events.current.connexionStatusChanged();
    }

    public void setWire() {
        connexion = Device.WIRE;
        Events.current.connexionStatusChanged();
    }

    void setSatus(string status) {
        this.status = status;
        visualStatus.text = status;
    }

}
