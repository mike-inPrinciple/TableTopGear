using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSiren : MonoBehaviour {

    public bool isActive;

    private Light[] spotlights;

    [SerializeField] private float[] lightRanges;

    private Transform model;
    [SerializeField] private Vector3[] modelScales;

    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float appearSpeed = 5f;
    [SerializeField] private AnimationCurve curve_AppearSpeed;

    private Coroutine COR_Siren;
    private Vector3 sScale;
    private Vector3 eScale;
    private float currentRange;
    private float sRange;
    private float eRange;

    public void Setup(int playerID) {
        model = transform.Find("Model");
        spotlights = model.GetComponentsInChildren<Light>();
        for (int i = 0; i < spotlights.Length; i++) {
            spotlights[i].color = GameManager.script.playerColors[playerID];
        }
        TurnOnOff(false, true);
    }

    public void TurnOnOff(bool onOff, bool insta = false) {
        sScale = model.localScale;
        sRange = currentRange;
        if (onOff) {
            model.gameObject.SetActive(onOff);
            eScale = modelScales[1];
            eRange = lightRanges[1];
        } else {
            eScale = modelScales[0];
            eRange = lightRanges[0];
        }
        StopSiren();
        if (insta) {
            for (int i = 0; i < spotlights.Length; i++) {
                spotlights[i].range = eRange;
            }
            currentRange = eRange;
            model.localScale = eScale;
            model.localRotation = Quaternion.identity;
            model.gameObject.SetActive(onOff);
        } else {
            COR_Siren = StartCoroutine(Siren(onOff));
        }
        isActive = onOff;
    }

    void StopSiren() {
        if (COR_Siren != null) {
            StopCoroutine(COR_Siren);
        }
    }

    IEnumerator Siren(bool onOff) {
        float passingTime = 0f;
        while (passingTime < 1f) {
            passingTime += Time.deltaTime * appearSpeed;
            model.localScale = Vector3.Lerp(sScale, eScale, curve_AppearSpeed.Evaluate(passingTime));
            currentRange = Mathf.Lerp(sRange, eRange, curve_AppearSpeed.Evaluate(passingTime));
            for (int i = 0; i < spotlights.Length; i++) {
                spotlights[i].range = currentRange;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if (onOff) {
            passingTime = 0f;
            while (true) {
                passingTime += Time.deltaTime * rotationSpeed;
                if (passingTime > 1f) {
                    passingTime -= 1f;
                }
                model.rotation = Quaternion.Euler(Vector3.up * Mathf.Lerp(0f, 360f - Time.deltaTime, passingTime) + Vector3.right * 270f);
                yield return new WaitForSeconds(Time.deltaTime);
            }
        } else {
            model.gameObject.SetActive(onOff);
        }
    }

}
