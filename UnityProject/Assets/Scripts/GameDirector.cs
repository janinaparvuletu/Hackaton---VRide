using UnityEngine;
using TMPro;
using System.Collections;

public class GameDirector : MonoBehaviour
{
    public TextMeshProUGUI textHUD;
    public float lapDuration = 5f; 
    public int totalLaps = 5;
    
    private int currentLap = 1;
    private float timer = 0f;

    void Start()
{
    // Caută automat un obiect numit "HUD_Main" în scenă
    GameObject textObj = GameObject.Find("HUD_Main");
    
    if (textObj != null)
    {
        textHUD = textObj.GetComponent<TextMeshProUGUI>();
    }

    if (textHUD == null)
    {
        Debug.LogError("Eroare: Nu am găsit obiectul HUD_Main în scenă! Redenumește textul tău în HUD_Main.");
    }
    else
    {
        textHUD.gameObject.SetActive(false);
        StartCoroutine(ShowPhase(1));
    }
}

    void Update()
    {
        // Această linie va rula doar dacă Start() a fost completat cu succes
        timer += Time.deltaTime;

        if (timer >= lapDuration)
        {
            timer = 0f;
            currentLap++;

            if (currentLap <= totalLaps)
            {
                StartCoroutine(ShowPhase(currentLap));
            }
            else
            {
                textHUD.text = "RACE FINISHED!";
                textHUD.gameObject.SetActive(true);
                this.enabled = false;
            }
        }
    }

    IEnumerator ShowPhase(int phaseNumber)
    {
        textHUD.text = "PHASE " + phaseNumber;
        textHUD.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        textHUD.gameObject.SetActive(false);
    }
}