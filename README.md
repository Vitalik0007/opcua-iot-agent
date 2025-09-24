# IoT Production Agent

---

## 📌 Opis projektu

Projekt przedstawia IoT Production Agent, który łączy lokalny serwer OPC UA z Azure IoT Hub.  
Celem projektu jest demonstrowanie integracji urządzeń przemysłowych z chmurą, w tym:

- wysyłanie telemetrii (D2C messages)
- synchronizacja stanu urządzenia (Device Twin)
- obsługa Direct Methods
- implementacja podstawowej logiki biznesowej

---

## ⚙️ Funkcjonalności

### ✅ Telemetria
- Wysyłanie telemetrii jednorazowo (na żądanie)
- Wysyłanie telemetrii automatycznie co 5 sekund
- Telemetria obejmuje:
  - status produkcji
  - liczba dobrych i złych produktów
  - temperatura
  - błędy urządzenia

### ✅ Device Twin
- Odczyt bieżącego stanu Device Twin z IoT Hub  
- Aktualizacja reported properties po wysłaniu telemetrii  

### ✅ Direct Methods
Agent obsługuje następujące Direct Methods z IoT Hub:

| Metoda               | Działanie                                              |
|----------------------|--------------------------------------------------------|
| StartProduction    | Rozpoczyna automatyczne wysyłanie telemetrii         |
| StopProduction     | Zatrzymuje automatyczne wysyłanie telemetrii         |
| EmergencyStop      | Natychmiast zatrzymuje produkcję                     |
| ResetErrorStatus   | Resetuje stan błędów urządzenia                       |

### ✅ Logika Biznesowa
- Automatyczny EmergencyStop, gdy urządzenie zgłosi więcej niż 3 błędy w krótkim czasie
- Zmniejszenie Desired Production Rate o 10%, jeśli wskaźnik dobrej produkcji spadnie poniżej 90%

---

## 👨🏻‍💻 Autor
Projekt wykonany w celach edukacyjnych przez: Vitalii Yavorskyi
