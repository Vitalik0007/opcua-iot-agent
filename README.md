# IoT Production Agent

---

## 📌 Opis projektu

Projekt przedstawia IoT Production Agent, który łączy lokalny serwer OPC UA z Azure IoT Hub.
Demonstruje cykl integracji urządzenia przemysłowego z chmurą, w tym:

- wysyłanie telemetrii
- synchronizację Device Twin
- obsługę Direct Methods

---

## ⚙️ Funkcjonalności

### ✅ Telemetria
- Wysyłanie telemetrii jednorazowo (na żądanie).
- Wysyłanie telemetrii automatycznie co 5 sekund.

### ✅ Device Twin
- Odczyt bieżącego stanu Device Twin z IoT Hub.  
- Aktualizacja reported properties po wysłaniu telemetrii.  
- Zmiana desired properties bezpośrednio z konsoli.

### ✅ Direct Methods
Agent obsługuje Direct Methods wywoływane z IoT Hub:
- ResetCounters → resetuje liczniki produkcji w OPC UA.
- StopProduction → zatrzymuje automatyczne wysyłanie telemetrii.

---

## 👨🏻‍💻 Autor
Projekt wykonany w celach edukacyjnych: Vitalii Yavorskyi
