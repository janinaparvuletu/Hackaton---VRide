using UnityEngine;
using System;
using Newtonsoft.Json.Linq; // Inclus automat de Unity

public class BKUS_BPMReceiver : MonoBehaviour
{
    private SocketIOUnity socket;

    [Header("Setări Rețea")]
    // PUNETI IP-UL CORECT AL SERVERULUI AICI
    public string serverURL = "http://10.0.4.254:3000"; 

    [Header("Date Live (Nu modifica manual)")]
    public int currentBPM = 0;

    // Variabile pentru thread-safety
    private int threadSafeBPM = 0;
    private bool hasNewBPMData = false;
    private readonly object _lockObject = new object();

    void Start()
    {
        Debug.Log("🔌 Inițializare conexiune către server: " + serverURL);
        var uri = new Uri(serverURL);
        
        // Configurare Socket.io
        socket = new SocketIOUnity(uri);

        // Eveniment: Când ne-am conectat cu succes la serverul tău de Node.js
        socket.OnConnected += (sender, e) => {
            Debug.Log("✅ [SOCKET.IO] Conectat cu succes la serverul BKUS!");
        };

        // Eveniment: Ascultăm când serverul trimite 'update_bpm'
        socket.On("update_bpm", (response) => {
            try
            {
                // Răspunsul primit de la Node.js vine în format JSON array: [{"bpm": 125}]
                string jsonString = response.ToString();
                JArray jsonArray = JArray.Parse(jsonString);
                JObject dataObject = (JObject)jsonArray[0];
                
                int bpmCitit = (int)dataObject["bpm"];

                // Salvăm datele în siguranță (Thread-Safe Lock)
                lock (_lockObject)
                {
                    threadSafeBPM = bpmCitit;
                    hasNewBPMData = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("⚠️ Eroare la citirea/parsarea datelor JSON: " + ex.Message);
            }
        });

        // Pornim conexiunea efectivă
        socket.Connect();
    }

    void Update()
    {
        // Verificăm dacă a venit un puls nou de pe rețea
        lock (_lockObject)
        {
            if (hasNewBPMData)
            {
                currentBPM = threadSafeBPM;
                hasNewBPMData = false;
                
                // AICI SE COMENTEAZĂ SAU SE LEAGĂ LOGICA JOCULUI
                AplicaPulsInJoc(currentBPM);
            }
        }
    }

    // Aici se întâmplă magia proiectului vostru
    void AplicaPulsInJoc(int bpm)
    {
        Debug.Log("❤️ Puls aplicat în gameplay: " + bpm + " BPM");

        // EXEMPLU 1: Schimbarea textului de pe UI
        // textPuls.text = bpm.ToString() + " BPM";

        // EXEMPLU 2: Legarea vitezei de deplasare de puls
        // Daca are puls mic (warmup), merge incet. Daca trage tare, masina/racheta accelereaza
        // float vitezaNoua = bpm * 0.1f;
        // jucatorScript.SetViteza(vitezaNoua);
    }

    private void OnDestroy()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket.Dispose();
            Debug.Log("🔌 Socket deconectat curat la închiderea jocului.");
        }
    }
}