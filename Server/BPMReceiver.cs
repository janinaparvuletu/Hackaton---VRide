using UnityEngine;
using System;
using Newtonsoft.Json.Linq; // Inclus automat de Unity

public class BPMReceiver : MonoBehaviour
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

























import asyncio
import socketio
import sys
import io
from bleak import BleakClient, BleakScanner
from datetime import datetime

# Force UTF-8 output on Windows
sys.stdout = io.TextIOWrapper(
    sys.stdout.buffer, 
    encoding='utf-8', 
    errors='replace',
    line_buffering=True
)

SERVER_URL = "http://localhost:3000"
HR_CHARACTERISTIC_UUID = "00002a37-0000-1000-8000-00805f9b34fb"

sio = socketio.AsyncClient()
loop = None


def handle_heart_rate_data(sender, data):
    try:
        flags = data[0]
        if flags & 0x01:
            bpm_value = int.from_bytes(data[1:3], byteorder='little')
        else:
            bpm_value = data[1]

        timestamp = datetime.now().strftime("%H:%M:%S")
        print(f"[{timestamp}] Puls detectat: {bpm_value} BPM")
        
        asyncio.run_coroutine_threadsafe(
            sio.emit("trimite_bpm", {"bpm": bpm_value, "timestamp": timestamp}),
            loop
        )
    except Exception as e:
        print(f"[ERR] Eroare la procesarea BPM: {e}")


KNOWN_POLAR_ADDRESS = None


async def main():
    global loop, KNOWN_POLAR_ADDRESS
    loop = asyncio.get_running_loop()

    try:
        await sio.connect(SERVER_URL)
        print("[OK] Conectat la server!")
    except Exception as e:
        print(f"[ERR] Nu ma pot conecta la server: {e}")
        return

    while True:
        if KNOWN_POLAR_ADDRESS is None:
            print("[...] Cautare senzori Bluetooth...")
            devices = await BleakScanner.discover()
            for d in devices:
                # print(f"Gasit: {d.name} | {d.address}")  # comentat
                if d.name and any(
                    kw in d.name.upper()
                    for kw in ["POLAR", "GARMIN", "HRM", "HEART", "WAHOO"]
                ):
                    KNOWN_POLAR_ADDRESS = d.address
                    print(f"[>>] Senzor gasit: {d.name} [{d.address}]")
                    break

        if KNOWN_POLAR_ADDRESS is None:
            print("[ERR] Nu am gasit niciun senzor. Reincerc in 5s...")
            await asyncio.sleep(5)
            continue

        print(f"[->] Conectare la {KNOWN_POLAR_ADDRESS}")
        try:
            async with BleakClient(
                KNOWN_POLAR_ADDRESS,
                disconnected_callback=lambda c: print("[!] Deconectat de la senzor!")
            ) as client:
                print("[OK] Conectat!")

                # Asteapta ca GATT sa fie gata
                await asyncio.sleep(2)

                # Verifica daca caracteristica HR exista
                services = client.services
                hr_char = None
                for service in services:
                    for char in service.characteristics:
                        if HR_CHARACTERISTIC_UUID in char.uuid:
                            hr_char = char
                            break

                if hr_char is None:
                    print("[ERR] Caracteristica HR nu a fost gasita. Rescanez...")
                    KNOWN_POLAR_ADDRESS = None
                    continue

                await client.start_notify(HR_CHARACTERISTIC_UUID, handle_heart_rate_data)
                print("[~] Ascult notificarile BPM...")

                while client.is_connected:
                    await asyncio.sleep(1)
                    try:
                        await client.read_gatt_char("00002a19-0000-1000-8000-00805f9b34fb")
                    except:
                        pass

        except Exception as e:
            print(f"[ERR] EROARE BLE: {e}")
            if any(kw in str(e).lower() for kw in ["not found", "unreachable", "characteristic"]):
                KNOWN_POLAR_ADDRESS = None

        print("[R] Reconectare in 3 secunde...")
        await asyncio.sleep(3)


if __name__ == "__main__":
    asyncio.run(main())

KNOWN_POLAR_ADDRESS = None


async def main():
    global loop, KNOWN_POLAR_ADDRESS
    loop = asyncio.get_running_loop()

    try:
        await sio.connect(SERVER_URL)
        print("[OK] Conectat la server!")
    except Exception as e:
        print(f"[ERR] Nu ma pot conecta la server: {e}")
        return

    while True:
        if KNOWN_POLAR_ADDRESS is None:
            print("[...] Cautare senzori Bluetooth...")
            devices = await BleakScanner.discover()
            for d in devices:
                # print(f"Gasit: {d.name} | {d.address}")  # comentat
                if d.name and any(
                    kw in d.name.upper()
                    for kw in ["POLAR", "GARMIN", "HRM", "HEART", "WAHOO"]
                ):
                    KNOWN_POLAR_ADDRESS = d.address
                    print(f"[>>] Senzor gasit: {d.name} [{d.address}]")
                    break

        if KNOWN_POLAR_ADDRESS is None:
            print("[ERR] Nu am gasit niciun senzor. Reincerc in 5s...")
            await asyncio.sleep(5)
            continue

        print(f"[->] Conectare la {KNOWN_POLAR_ADDRESS}")
        try:
            async with BleakClient(
                KNOWN_POLAR_ADDRESS,
                disconnected_callback=lambda c: print("[!] Deconectat de la senzor!")
            ) as client:
                print("[OK] Conectat!")

                # Asteapta ca GATT sa fie gata
                await asyncio.sleep(2)

                # Verifica daca caracteristica HR exista
                services = client.services
                hr_char = None
                for service in services:
                    for char in service.characteristics:
                        if HR_CHARACTERISTIC_UUID in char.uuid:
                            hr_char = char
                            break

                if hr_char is None:
                    print("[ERR] Caracteristica HR nu a fost gasita. Rescanez...")
                    KNOWN_POLAR_ADDRESS = None
                    continue

                await client.start_notify(HR_CHARACTERISTIC_UUID, handle_heart_rate_data)
                print("[~] Ascult notificarile BPM...")

                while client.is_connected:
                    await asyncio.sleep(1)
                    try:
                        await client.read_gatt_char("00002a19-0000-1000-8000-00805f9b34fb")
                    except:
                        pass

        except Exception as e:
            print(f"[ERR] EROARE BLE: {e}")
            if any(kw in str(e).lower() for kw in ["not found", "unreachable", "characteristic"]):
                KNOWN_POLAR_ADDRESS = None

        print("[R] Reconectare in 3 secunde...")
        await asyncio.sleep(3)


if __name__ == "__main__":
    asyncio.run(main())