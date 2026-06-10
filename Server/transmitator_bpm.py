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