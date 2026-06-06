using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Conexiune Senzor")]
    [Tooltip("Trage aici obiectul [NetworkManager] din scenă care are scriptul BKUS_BPMReceiver")]
    public BKUS_BPMReceiver bpmReceiver;

    [Header("Setari Viteza & BPM (Bio-Feedback)")]
    public float vitezaMaxima = 60f;    
    public float acceleratie = 40f;     
    public float decelerare = 35f;      

    [Tooltip("Pulsul optim. La acest BPM sau mai jos, mașina are viteză maximă.")]
    public float bpmCalm = 80f; 
    [Tooltip("Pulsul de stres. La acest BPM sau mai sus, mașina se oprește complet.")]
    public float bpmAgitat = 120f;

    private float vitezaCurenta = 0f;  
    private float lungimeDrum = 2000f; 
    private Vector3 pozitieStart;

    void Start()
    {
        pozitieStart = transform.position;
        vitezaCurenta = 0f; 
    }

    void Update()
    {
        // 1. Luăm pulsul live de la senzor. Dacă e 0 (eroare/scos de pe piept), mașina va sta pe loc.
        int bpmActual = (bpmReceiver != null && bpmReceiver.currentBPM > 0) ? bpmReceiver.currentBPM : 0;
        
        float vitezaTinta = 0f;

        // 2. Logica BKUS: Calculăm viteza țintă pe baza stresului
        if (bpmActual > 0)
        {
            // InverseLerp calculează cât de "stresat" este copilul pe o scară de la 0 la 1 (între 80 și 120 BPM)
            float factorStres = Mathf.InverseLerp(bpmCalm, bpmAgitat, bpmActual);
            
            // Inversăm factorul ca să îl premiem pe cel calm: Stres 0 = Viteză 100%
            float factorCalm = 1f - factorStres; 

            vitezaTinta = vitezaMaxima * factorCalm;
        }

        // 3. Accelerăm sau frânăm lin către noua viteză dictată de inimă
        if (vitezaCurenta < vitezaTinta)
        {
            vitezaCurenta += acceleratie * Time.deltaTime;
        }
        else
        {
            vitezaCurenta -= decelerare * Time.deltaTime;
        }

        // Limităm stric viteza
        vitezaCurenta = Mathf.Clamp(vitezaCurenta, 0f, vitezaMaxima);

        // 4. MISCAREA
        transform.Translate(Vector3.forward * -vitezaCurenta * Time.deltaTime, Space.Self);

        // 5. Resetare Pozitie (Loop pentru demo nesfârșit)
        if (Vector3.Distance(pozitieStart, transform.position) > lungimeDrum)
        {
            transform.position = pozitieStart;
        }
    }
}