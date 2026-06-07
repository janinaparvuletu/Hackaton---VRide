using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Setari BPM -> Viteza")]
    public float bpmMin = 50f;
    public float bpmMax = 130f;
    public float vitezaLaMinBPM = 5f;
    public float vitezaLaMaxBPM = 60f;

    [Header("Setari Drum")]
    public float lungimeDrum = 2000f;

    [Header("Debug")]
    public float bpmCurent = 0f;
    public float vitezaMaximaCurenta = 0f;

    private float vitezaCurenta = 0f;
    private Vector3 pozitieStart;
    public BKUS_BPMReceiver bpmReceiver;

    void Start()
    {
        pozitieStart = transform.position;
        vitezaCurenta = 0f;

        bpmReceiver = FindAnyObjectByType<BKUS_BPMReceiver>();

        if (bpmReceiver == null)
            Debug.LogError("[PlayerController] Nu am gasit BKUS_BPMReceiver in scena!");
        else
            Debug.Log("[PlayerController] BKUS_BPMReceiver gasit cu succes!");
    }

    void Update()
    {
        if (bpmReceiver != null && bpmReceiver.currentBPM > 0)
        {
            bpmCurent = bpmReceiver.currentBPM;

            // Viteza se seteaza direct din BPM, fara tasta
            vitezaCurenta = Mathf.Lerp(
                vitezaLaMinBPM,
                vitezaLaMaxBPM,
                Mathf.InverseLerp(bpmMin, bpmMax, bpmCurent)
            );

            vitezaMaximaCurenta = vitezaCurenta; // doar pentru debug in Inspector
        }
        else
        {
            // Fara semnal BPM -> sta pe loc
            bpmCurent = 0;
            vitezaCurenta = 0f;
            vitezaMaximaCurenta = 0f;
        }

        // Miscare automata inainte
        transform.Translate(Vector3.forward * -vitezaCurenta * Time.deltaTime, Space.Self);

        // Reset pozitie la capatul drumului
        if (Vector3.Distance(pozitieStart, transform.position) > lungimeDrum)
            transform.position = pozitieStart;
    }
}