#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClient.h>
#include <WebServer.h>
#include "sensors/DhtService/DhtService.h"

#define WIFI_SSID "Wokwi-GUEST"
#define WIFI_PASSWORD ""
// Defining the WiFi channel speeds up the connection:
#define WIFI_CHANNEL 6

WebServer server(80); 

unsigned long last_measure_time = 0;

void sendWelcomePageHtml() {
  String response = R"(
    <!DOCTYPE html><html>
      <head>
        <title>Welcome</title>
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <style>
        </style>
      </head>
            
      <body>
        <h1>Welcome</h1>
      </body>
    </html>
  )";
  server.send(200, "text/html", response);
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
}

void loop() {
    //server
    server.handleClient();
    delay(2);

    //sensors
    if (millis() - last_measure_time >= 3000) { //every 3 seconds
        last_measure_time = millis();
        //dht
        float temperature = readDhtTemperature();
        float humidity = readDhtHumidity();
        //mqtt here
    }   
}