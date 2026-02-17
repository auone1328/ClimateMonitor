#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClient.h>
#include <WebServer.h>
#include <PubSubClient.h>
#include "sensors/DhtService/DhtService.h"
#include "pages/Pages.h"
#include "config/Config.h"

#define WIFI_SSID "Wokwi-GUEST"
#define WIFI_PASSWORD ""
// Defining the WiFi channel speeds up the connection:
#define WIFI_CHANNEL 6

#define AP_SSID_PREFIX "Climate-Setup-"
#define AP_PASSWORD "12345678"
#define MQTT_SERVER "test.mosquitto.org"
#define MQTT_PORT 1883

const char* projectPrefix = "cs2026_climate_7A3B9F2C/";

WiFiClient espClient;
PubSubClient mqttClient(espClient);

WebServer server(80);

bool apMode = false;
int wifiConnectAttempts = 0;
unsigned long lastMeasure = 0;

void handleRoot() {
    server.send(200, "text/html", getSetupPageHtml());
}

void handleSave() {
    String ssid = server.arg("ssid");
    String pass = server.arg("pass");
    String building = server.arg("building");
    String room = server.arg("room");

    if (ssid.isEmpty() || building.isEmpty() || room.isEmpty()) {
        server.send(400, "text/plain", "Missing fields");
        return;
    }

    if (saveConfig(ssid, pass, building, room)) {
        server.send(200, "text/html", "<h1>Saved! Rebooting...</h1>");
        delay(1000);
        ESP.restart();
    }
    else {
      server.send(500, "text/plain", "Save failed");
    }
}

void mqttCallback(char* topic, byte* payload, unsigned int length) {
    // Пока пусто — сюда придут команды включить/выключить кондиционер и т.д.
}

void startAPMode() {
    //Для wokwi пока без точки доступа
    // String apSSID = AP_SSID_PREFIX + String((uint32_t)ESP.getEfuseMac() >> 32, HEX).substring(0, 4);
    // WiFi.mode(WIFI_AP);
    // WiFi.softAP(apSSID.c_str(), AP_PASSWORD); 

    // Serial.printf("AP started: %s / %s\n", apSSID.c_str(), AP_PASSWORD);
    // Serial.printf("Open http://%s\n", WiFi.softAPIP().toString().c_str());

    apMode = true;
    Serial.println("AP started.");

    server.on("/", handleRoot);
    server.on("/save", HTTP_POST, handleSave);
    server.begin();
}

void reconnectMQTT() {
    while (!mqttClient.connected()) {
        String clientId = "ESP32-" + String(random(0xffff), HEX);
        Serial.print("Connecting MQTT...");
        if (mqttClient.connect(clientId.c_str())) {  
            Serial.println("connected");
            // mqttClient.subscribe(....);  // позже для команд управления
        } 
        else {
            Serial.printf("failed, rc=%d → retry in 5s\n", mqttClient.state());
            delay(5000);
        }
    }
}

void startStationMode() {
    apMode = false;
    WiFi.mode(WIFI_STA);
    WiFi.begin(config.wifi_ssid.c_str(), config.wifi_pass.c_str());

    int attempts = 0;
    while (WiFi.status() != WL_CONNECTED && attempts < 20) {
        delay(500);
        Serial.print(".");
        attempts++;
    }

    if (WiFi.status() == WL_CONNECTED) {
        Serial.println("\nWiFi connected. IP: " + WiFi.localIP().toString());

        mqttClient.setServer(MQTT_SERVER, MQTT_PORT);
        reconnectMQTT();
    } 
    else {
        Serial.println("\nWiFi failed -> fallback to AP");
        wifiConnectAttempts++;
        if (wifiConnectAttempts >= 3) {  // после 3 неудач — снова AP
            startAPMode();
        } 
        else {
            startStationMode();  // retry
        }
    }
}

void setup() {
    Serial.begin(115200);
    delay(100);

    //Подключение к wifi вместо точки доступа (для wokwi)
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD, WIFI_CHANNEL);     
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        printf(".");
    }

    loadConfig();

    //Sensors init
    dhtInit();

    if (config.isConfigured) {
        Serial.println("Trying Station mode...");
        startStationMode();
    } 
    else {
        Serial.println("Starting AP mode (not configured)");
        startAPMode();
    }

    // MQTT callback (пока не используется)
    mqttClient.setCallback(mqttCallback);
}

void loop() {
    if (apMode) {
        server.handleClient();
    } 
    else {
        if (!mqttClient.connected()) {
            reconnectMQTT();
        }
        mqttClient.loop();
    }

    // Чтение датчиков и публикация
    if (millis() - lastMeasure >= 5000 && !apMode && mqttClient.connected()) {
        lastMeasure = millis();

        float t = readDhtTemperature();
        float h = readDhtHumidity();

        if (!isnan(t)) {
            String topic = String(projectPrefix) + lowerFormat(config.building_name) + "/" +
                           lowerFormat(config.room_name) + "/temperature";
            mqttClient.publish(topic.c_str(), String(t, 1).c_str());
        }

        if (!isnan(h)) {
            String topic = String(projectPrefix) + lowerFormat(config.building_name) + "/" +
                           lowerFormat(config.room_name) + "/humidity";
            mqttClient.publish(topic.c_str(), String(h, 1).c_str());
        }
    }

    delay(10);
}