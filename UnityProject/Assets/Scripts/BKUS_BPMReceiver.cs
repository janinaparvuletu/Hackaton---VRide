using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using SocketIOClient;

public class BKUS_BPMReceiver : MonoBehaviour
{
    private SocketIOUnity socket;

    [Header("Setări Rețea")]
    public string serverURL = "http://192.168.1.100:3000";

    [Header("Date Live (Nu modifica manual)")]
    public int currentBPM = 0;

    private int threadSafeBPM = 0;
    private bool hasNewBPMData = false;
    private readonly object _lockObject = new object();

    void Start()
    {
        Debug.Log("Initializare conexiune catre server: " + serverURL);
        var uri = new Uri(serverURL);

        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            EIO = SocketIOClient.EngineIO.V4
        });

        socket.OnConnected += (sender, e) => {
            Debug.Log("[SOCKET.IO] Conectat cu succes la serverul BKUS!");
        };

        socket.OnDisconnected += (sender, e) => {
            Debug.Log("[SOCKET.IO] Deconectat de la server: " + e);
        };

        socket.OnError += (sender, e) => {
            Debug.LogError("[SOCKET.IO] Eroare: " + e);
        };

        socket.On("update_bpm", (response) => {
            try
            {
                string jsonString = response.ToString();
                JArray jsonArray = JArray.Parse(jsonString);
                JObject dataObject = (JObject)jsonArray[0];

                int bpmCitit = (int)dataObject["bpm"];

                lock (_lockObject)
                {
                    threadSafeBPM = bpmCitit;
                    hasNewBPMData = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Eroare la parsarea JSON: " + ex.Message);
            }
        });

        socket.Connect();
    }

    void Update()
    {
        lock (_lockObject)
        {
            if (hasNewBPMData)
            {
                currentBPM = threadSafeBPM;
                hasNewBPMData = false;
                AplicaPulsInJoc(currentBPM);
            }
        }
    }

    void AplicaPulsInJoc(int bpm)
    {
        Debug.Log("Puls aplicat in gameplay: " + bpm + " BPM");
        // EXEMPLU 1: Schimbarea textului de pe UI
        // textPuls.text = bpm.ToString() + " BPM";

        // EXEMPLU 2: Legarea vitezei de deplasare de puls
        // float vitezaNoua = bpm * 0.1f;
        // jucatorScript.SetViteza(vitezaNoua);
    }

    private void OnDestroy()
    {
        if (socket != null)
        {
            socket.Disconnect();
            socket.Dispose();
            Debug.Log("Socket deconectat curat la inchiderea jocului.");
        }
    }
}