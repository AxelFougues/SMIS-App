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
        BluetoothAdapter.OnDeviceDiscovered += onDeviceDiscovered;
        BluetoothAdapter.OnDevicePicked += onDevicePicked;
        BluetoothAdapter.OnConnected += onDeviceConnected;
        setSatus("Tap to scan for device");
        refresh();
    }


    public void onDeviceDiscovered(BluetoothDevice device, short RSSI) {
        if (status == "Scanning for device" && device.Name == cubeBTName) {
            if(device.IsConnected) connexion = Device.CUBE;
            else device.connect();
        } 
    }

    public void onDevicePicked(BluetoothDevice device) {
        if (status == "Scanning for device" && device.Name == cubeBTName) {
            if (device.IsConnected) connexion = Device.CUBE;
            else device.connect();
        }
    }

    public void onDeviceConnected(BluetoothDevice device) {
        if (status == "Scanning for device" && device.Name == cubeBTName) connexion = Device.CUBE;
    }

    IEnumerator scanConnexion() {
        connexion = Device.NONE;
        Events.current.connexionStatusChanged();
        setSatus("Scanning for device");
        visual.SetTrigger("Scan");
        float timeout = Time.time;
        if (BluetoothAdapter.isBluetoothEnabled()) {
            BluetoothAdapter.startDiscovery();
            //BluetoothAdapter.showDevices();
        }
        while (connexion == Device.NONE && Time.time - timeout < scanTimeout) {

            if (DetectHeadset.CanDetect() && DetectHeadset.Detect()) connexion = Device.WIRE;

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
