using UnityEngine;
using UnityEngine.InputSystem; // Obligatoriu pentru Unity 6

public class PlayerController : MonoBehaviour
{
    [Header("Setari Viteza")]
    public float vitezaMaxima = 60f;    // Viteza maxima cand tii W apasat
    public float acceleratie = 40f;     // Cat de repede prinde viteza cand apesi W
    public float decelerare = 35f;      // Cat de repede franeaza cand lasi W
    
    private float vitezaCurenta = 0f;   // Porneste de la 0 (sta pe loc)

    [Header("Setari Drum")]
    public float lungimeDrum = 2000f; 
    private Vector3 pozitieStart;

    void Start()
    {
        pozitieStart = transform.position;
        vitezaCurenta = 0f; // Ne asiguram ca sta pe loc la inceput
    }

    void Update()
    {
        // 1. Detectam daca tasta W este apasata
        bool wApasat = false;
        if (Keyboard.current != null)
        {
            wApasat = Keyboard.current.wKey.isPressed;
        }

        // 2. Logica de pornire si oprire
        if (wApasat)
        {
            // Daca apesi W, accelereaza pana la viteza maxima
            vitezaCurenta += acceleratie * Time.deltaTime;
        }
        else
        {
            // Daca NU apesi W, franeaza pana ajunge la viteză 0
            vitezaCurenta -= decelerare * Time.deltaTime;
        }

        // Limitam viteza: minimul e 0 (sta pe loc), maximul e viteza maxima setata
        vitezaCurenta = Mathf.Clamp(vitezaCurenta, 0f, vitezaMaxima);

        // 3. MISCAREA (pastram directia care a functionat)
        transform.Translate(Vector3.forward * -vitezaCurenta * Time.deltaTime, Space.Self);

        // 4. Resetare Pozitie (Loop)
        if (Vector3.Distance(pozitieStart, transform.position) > lungimeDrum)
        {
            transform.position = pozitieStart;
        }
    }
}