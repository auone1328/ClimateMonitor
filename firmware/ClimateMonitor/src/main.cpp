#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClient.h>
#include <WebServer.h>
#include <PubSubClient.h>
#include "sensors/DhtService/DhtService.h"
#include "pages/Pages.h"

#define WIFI_SSID "Wokwi-GUEST"
#define WIFI_PASSWORD ""
// Defining the WiFi channel speeds up the connection:
#define WIFI_CHANNEL 6
#define PROJECT_ID "cs2026_climate_7A3B9F2C/"

const char* mqtt_server = "test.mosquitto.org";
const int mqtt_port = 1883;

WiFiClient espClient;
PubSubClient client(espClient);

WebServer server(80);

unsigned long last_measure_time = 0;

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Create a random client ID
    String clientId = "ESP32Client-";
    clientId += String(random(0xffff), HEX);
    // Attempt to connect
    if (client.connect(clientId.c_str())) {
      printf("connected\n");
      // ... and resubscribe
      //client.subscribe();
    } 
    else {
      printf("failed, rc=\n");
      printf("%d\n", client.state());
      printf(" try again in 5 seconds\n");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void setup() {
  //WiFi init
  delay(10);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD, WIFI_CHANNEL);     
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    printf(".");
  }
  
  printf("\nWiFi connected");
  printf("SSID: %s\n", WIFI_SSID);
  printf("IP Address: %s\n", WiFi.localIP().toString().c_str());

  //sensors init
  dhtInit();

  //server init
  server.on("/", sendWelcomePageHtml);
  server.begin();
  printf("\nHTTP server started (http://localhost:8180)");

  //mqtt
  client.setServer(mqtt_server, 1883);
  //client.setCallback(callback);
}

void loop() {
    //server
    server.handleClient();
    delay(2);

    //mqtt
    if (!client.connected()) {
      reconnect();
    }
    client.loop();

    //sensors
    if (millis() - last_measure_time >= 3000) { //every 3 seconds
      last_measure_time = millis();
      //dht
      float temperature = readDhtTemperature();
      float humidity = readDhtHumidity();
      //mqtt here
      String dhtTopic = String(PROJECT_ID) + "/temperature";
      client.publish(dhtTopic.c_str(), String(temperature).c_str());
    }   
}