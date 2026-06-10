import asyncio
import socketio
from bleak import BleakClient
from bleak.exc import BleakError

# Configurații verificate din logurile tale
SERVER_URL = "http://10.0.4.254:3005"  # Portul 3005 care funcționează!
POLAR_MAC = "A0:9E:1A:AB:1D:15"
HR_CHARACTERISTIC_UUID = "00002a37-0000-1000-8000-00805f9b34fb"

sio = socketio.AsyncClient()

def callback_puls(sender, data):
    bpm = data[1]
    print(f"❤️ PULS REAL POLAR: {bpm} BPM")
    asyncio.run_coroutine_threadsafe(sio.emit('trimite_bpm', {'bpm': int(bpm)}), loop)

async def main():
    global loop
    loop = asyncio.get_event_loop()

    print(f"Conectare la serverul Node.js: {SERVER_URL}...")
    try:
        await sio.connect(SERVER_URL)
        print("✅ Conectat cu succes la server!")
    except Exception as e:
        print(f"❌ Server offline: {e}")
        return

    print(f"🎯 Pornim bucla de conectare directă la Polar [{POLAR_MAC}]...")
    print("💡 Scriptul va ignora erorile Windows și va reîncerca automat la fiecare secundă.")

    while True:
        try:
            #use_cached_services=False forțează Windows să ignore sesiunile fantomă vechi
            async with BleakClient(POLAR_MAC, timeout=10.0, winrt=dict(use_cached_services=False)) as client:
                print("🚀 CONEXIUNE BLE REUȘITĂ! Datele curg live spre server și Unity...")
                await client.start_notify(HR_CHARACTERISTIC_UUID, callback_puls)
                
                while client.is_connected:
                    await asyncio.sleep(1)
                    
        except BleakError as be:
            print(f"⚠️ Port agățat de Windows ({be}). Reîncercăm imediat...")
            await asyncio.sleep(1)
        except Exception as e:
            print(f"⚠️ Reîncercare conexiune... Asigură-te că banda e bine umezită!")
            await asyncio.sleep(1)

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n👋 Script oprit manual.")