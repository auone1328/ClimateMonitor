#include "DhtService.h"
#include <Arduino.h>

#define DHTPIN 15
#define DHTTYPE DHT22

DHT_Unified dht(DHTPIN, DHTTYPE);

void dhtInit() {
    dht.begin();

    sensor_t sensor;
    dht.temperature().getSensor(&sensor);
    printf("DHT Sensor: %s\n", sensor.name);
    printf("Max Value: %.1f°C\n", sensor.max_value);
    printf("Min Value: %.1f°C\n", sensor.min_value);
    printf("Resolution: %.1f°C\n", sensor.resolution);
}

float readDhtTemperature() {
    sensors_event_t event;

    //Getting temperature
    dht.temperature().getEvent(&event); 
    if (isnan(event.temperature)) {
        printf("Error reading temperature!\n");
    }
    else {
        printf("Temperature: %.2f°C\n", event.temperature);
    }

    return event.temperature;
}

float readDhtHumidity() {
    sensors_event_t event;

    //Getting humidity
    dht.humidity().getEvent(&event);
    if (isnan(event.relative_humidity)) {
        printf("Error reading humidity!\n");
    }
    else {
        printf("Humidity: %.2f%\n", event.relative_humidity);
    }

    return event.relative_humidity;
}