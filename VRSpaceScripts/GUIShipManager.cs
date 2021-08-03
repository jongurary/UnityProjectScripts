/*
 * Acts as a link between the ship preview GUI and the various components of this ship
 * 
 * 
 * 07.07.20 initial commit v1
 * Polls reactor, shields, and reactor core health and reports them back to the main preview canvas
 * Activates preview camera so it can be picked up by the previewCamera raw image
 * 
 * Version 1 by Jon Gurary 07.07.20
 */

using UnityEngine;
using UnityEngine.UI;

public class GUIShipManager : MonoBehaviour{

    public bool isActive;

    public GameObject rootCanvas;

    public Image shieldDisplay;
    public Image healthDisplay;
    public Image reactorDisplay;

    public GameObject previewCamera;
    public ReactorManager reactor;
    public LifeManager life;
    public PartLife reactorLife;


    void Start(){
        if (previewCamera == null) {
            previewCamera = GetComponentInChildren<Camera>().gameObject;
        }
        if (reactor == null) {
            reactor = GetComponent<ReactorManager>();
        }
        if (life == null) {
            life = GetComponent<LifeManager>();
        }
        if (reactorLife == null) {
            reactorLife= reactor.gameObject.GetComponent<PartLife>();
        }
        if (rootCanvas != null) {
            rootCanvas.SetActive(false);
        }
        isActive = false;
    }

    void Update(){

    }

    private void OnGUI() {
        if (isActive) {
            shieldDisplay.fillAmount = (float)life.shieldLife / (float)life.maxShield;
            healthDisplay.fillAmount = (float)reactorLife.currentLife / (float)reactorLife.maxLife;
            reactorDisplay.fillAmount = (float)reactor.currentEnergy / (float)reactor.maxEnergy;
        }
    }

    public void activate() {
        rootCanvas.SetActive(true);
        previewCamera.SetActive(true);
        isActive = true;
    }

    public void deactivate() {
        rootCanvas.SetActive(false);
        previewCamera.SetActive(false);
        isActive = false;
    }
}
