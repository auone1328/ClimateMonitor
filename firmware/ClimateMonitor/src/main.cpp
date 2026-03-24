#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClient.h>
#include <WebServer.h>
#include <PubSubClient.h>
#include <esp_system.h>
#include "sensors/DhtService/DhtService.h"
#include "pages/Pages.h"
#include "config/Config.h"

#define WIFI_SSID "Wokwi-GUEST"
#define WIFI_PASSWORD ""
#define WIFI_CHANNEL 6

#define AP_SSID_PREFIX "Climate-Setup-"
#define AP_PASSWORD "12345678"
#define MQTT_SERVER "broker.hivemq.com"
#define MQTT_PORT 1883

#define RELAY_HEATER_PIN 2
#define RELAY_COOLER_PIN 4
#define MQ2_PIN 34

const char* projectPrefix = "cs2026_climate_7A3B9F2C/";

WiFiClient espClient;
PubSubClient mqttClient(espClient);

WebServer server(80);

bool apMode = false;
int wifiConnectAttempts = 0;
unsigned long lastMeasure = 0;
unsigned long lastMqttAttempt = 0;
unsigned long lastWifiAttempt = 0;
unsigned long lastBeat = 0;
unsigned long lastDebug = 0;
bool heaterState = false;
bool coolerState = false;
String lastQrJson;
String lastQrSvg;
float lastTemperature = NAN;
float lastHumidity = NAN;
float lastCo2 = NAN;

static String normalizeMac(const String& mac) {
    String s = mac;
    s.toLowerCase();
    s.replace(":", "");
    s.replace("-", "");
    return s;
}

static String deviceMac() {
    uint8_t mac[6] = {0};
    esp_read_mac(mac, ESP_MAC_WIFI_STA);
    char buf[18];
    snprintf(buf, sizeof(buf), "%02X:%02X:%02X:%02X:%02X:%02X",
             mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
    return String(buf);
}

static String configuredMac() {
    if (!config.device_mac.isEmpty()) {
        return config.device_mac;
    }
    return deviceMac();
}

static String telemetryTopic() {
    return String(projectPrefix) + normalizeMac(configuredMac()) + "/telemetry";
}

static String heaterSetTopic() {
    return String(projectPrefix) + normalizeMac(configuredMac()) + "/heater/set";
}

static String coolerSetTopic() {
    return String(projectPrefix) + normalizeMac(configuredMac()) + "/cooler/set";
}

static String relaySetTopic() {
    return String(projectPrefix) + normalizeMac(configuredMac()) + "/relay/set";
}

void handleRoot() {
    Serial.println("[HTTP] GET /");
    server.send(200, "text/html", getSetupPageHtml());
}

void handleSave() {
    Serial.println("[HTTP] POST /save");
    String ssid     = server.arg("ssid");
    String pass     = server.arg("pass");
    String building = server.arg("building");
    String room     = server.arg("room");

    Serial.printf("[HTTP] ssid='%s' building='%s' room='%s'\n",
                  ssid.c_str(), building.c_str(), room.c_str());

    if (ssid.isEmpty() || building.isEmpty() || room.isEmpty()) {
        Serial.println("[HTTP] Missing fields -> 400");
        server.send(400, "text/plain", "Missing fields");
        return;
    }

    String mac = deviceMac();
    if (saveConfig(ssid, pass, building, room, mac)) {
        Serial.println("[HTTP] Config saved, building QR");
        lastQrJson = "{\"mac\":\"" + mac +
                     "\",\"building\":\"" + building +
                     "\",\"room\":\"" + room +
                     "\",\"secret\":\"" + config.registration_secret + "\"}";
        lastQrSvg = getQrSvg(lastQrJson);

        String redirectPage = "<!DOCTYPE html><html><head><meta charset='utf-8'>"
                              "<meta http-equiv='refresh' content='0; url=/qr'/>"
                              "</head><body>"
                              "<p>Saved. Open <a href='/qr'>/qr</a>.</p>"
                              "</body></html>";
        server.send(200, "text/html; charset=utf-8", redirectPage);
    }
    else {
        Serial.println("[HTTP] Save failed -> 500");
        server.send(500, "text/plain", "Save failed");
    }
}

void handleFinish() {
    server.send(200, "text/plain", "Restarting...");
    delay(500);
    ESP.restart();
}

void mqttCallback(char* topic, byte* payload, unsigned int length) {
    String message;
    for (unsigned int i = 0; i < length; i++) {
        message += (char)payload[i];
    }

    String t = String(topic);
    bool state = message.indexOf("true") >= 0 || message.indexOf("1") >= 0;

    if (t.endsWith("/heater/set") || t.endsWith("/relay/set")) {
        heaterState = state;
        digitalWrite(RELAY_HEATER_PIN, heaterState ? HIGH : LOW);
    }

    if (t.endsWith("/cooler/set")) {
        coolerState = state;
        digitalWrite(RELAY_COOLER_PIN, coolerState ? HIGH : LOW);
    }
}

void startAPMode() {
    apMode = true;
    Serial.println("AP started.");

    server.on("/", handleRoot);
    server.on("/save", HTTP_POST, handleSave);
    server.on("/save", HTTP_GET, []() { server.send(405, "text/plain", "Use POST"); });
    server.on("/qr", HTTP_GET, []() {
        Serial.println("[HTTP] GET /qr");
        if (lastQrJson.isEmpty() || lastQrSvg.isEmpty()) {
            server.send(404, "text/plain", "QR not ready");
            return;
        }
        String page = getQrPageHtml(lastQrJson);
        server.send(200, "text/html; charset=utf-8", page);
    });
    server.on("/qr.svg", HTTP_GET, []() {
        if (lastQrSvg.isEmpty()) {
            server.send(404, "text/plain", "QR not ready");
            return;
        }
        server.send(200, "image/svg+xml", lastQrSvg);
    });
    server.on("/finish", HTTP_POST, handleFinish);
    server.onNotFound([]() {
        Serial.printf("[HTTP] 404 %s\n", server.uri().c_str());
        server.send(404, "text/plain", "Not found");
    });
    server.begin();
}

void reconnectMQTT() {
    if (mqttClient.connected()) {
        return;
    }

    if (millis() - lastMqttAttempt < 5000) {
        return;
    }
    lastMqttAttempt = millis();

    String clientId = "ESP32-" + String(random(0xffff), HEX);
    Serial.print("Connecting MQTT...");
    if (mqttClient.connect(clientId.c_str())) {
        Serial.println("connected");
        mqttClient.subscribe(heaterSetTopic().c_str());
        mqttClient.subscribe(coolerSetTopic().c_str());
        mqttClient.subscribe(relaySetTopic().c_str());
    }
    else {
        Serial.printf("failed, rc=%d -> retry in 5s\n", mqttClient.state());
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
        if (wifiConnectAttempts >= 3) {
            startAPMode();
        }
    }
}

float readMq2Co2() {
    int raw = analogRead(MQ2_PIN);
    return (float)raw;
}

void setup() {
    Serial.begin(115200);
    delay(100);

    pinMode(RELAY_HEATER_PIN, OUTPUT);
    pinMode(RELAY_COOLER_PIN, OUTPUT);
    digitalWrite(RELAY_HEATER_PIN, LOW);
    digitalWrite(RELAY_COOLER_PIN, LOW);

    WiFi.begin(WIFI_SSID, WIFI_PASSWORD, WIFI_CHANNEL);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        printf(".");
    }

    loadConfig();

    dhtInit();

    if (config.isConfigured) {
        Serial.println("Trying Station mode...");
        startStationMode();
    }
    else {
        Serial.println("Starting AP mode (not configured)");
        startAPMode();
    }

    mqttClient.setCallback(mqttCallback);
}

void loop() {
    if (millis() - lastBeat >= 2000) {
        lastBeat = millis();
        Serial.println("beat");
    }

    if (apMode) {
        server.handleClient();
    }
    else {
        if (WiFi.status() != WL_CONNECTED) {
            if (millis() - lastWifiAttempt >= 5000) {
                lastWifiAttempt = millis();
                Serial.println("WiFi disconnected, reconnecting...");
                WiFi.disconnect();
                WiFi.begin(config.wifi_ssid.c_str(), config.wifi_pass.c_str());
            }
        }

        if (!mqttClient.connected()) {
            if (WiFi.status() == WL_CONNECTED) {
                Serial.println("MQTT not connected, trying to reconnect...");
            }
            reconnectMQTT();
        }
        mqttClient.loop();
    }

    if (millis() - lastMeasure >= 5000 && !apMode) {
        lastMeasure = millis();

        float t = readDhtTemperature();
        float h = readDhtHumidity();
        float co2 = readMq2Co2();

        if (isnan(t) || t < -40.0f || t > 80.0f) {
            t = lastTemperature;
        }
        if (isnan(h) || h < 0.0f || h > 100.0f) {
            h = lastHumidity;
        }
        if (isnan(co2) || co2 < 100.0f || co2 > 10000.0f) {
            co2 = lastCo2;
        }

        if (isnan(t) || isnan(h) || isnan(co2)) {
            Serial.println("Telemetry skipped: invalid sensor data");
            delay(10);
            return;
        }

        if (millis() - lastDebug >= 5000) {
            lastDebug = millis();
            Serial.printf("Telemetry sample: t=%.2f h=%.2f co2=%.0f\n", t, h, co2);
        }

        lastTemperature = t;
        lastHumidity = h;
        lastCo2 = co2;

        String payload = "{\"temperature\":" + String(t, 1) +
                         ",\"humidity\":" + String(h, 1) +
                         ",\"co2\":" + String(co2, 0) +
                         ",\"relay\":" + String(heaterState ? "true" : "false") +
                         ",\"heater\":" + String(heaterState ? "true" : "false") +
                         ",\"cooler\":" + String(coolerState ? "true" : "false") + "}";

        if (mqttClient.connected()) {
            mqttClient.publish(telemetryTopic().c_str(), payload.c_str());
        } else {
            Serial.println("Telemetry skipped: MQTT not connected");
        }
    }

    delay(10);
}

