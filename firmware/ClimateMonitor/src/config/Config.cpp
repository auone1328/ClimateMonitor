#include "Config.h"
#include <Arduino.h>

Preferences prefs;
DeviceConfig config;

void loadConfig() {
    prefs.begin("climate-cfg", false);
    
    config.wifi_ssid = prefs.getString("wifi_ssid", "");
    config.wifi_pass = prefs.getString("wifi_pass", "");
    config.building_name = prefs.getString("building", "");
    config.room_name = prefs.getString("room", "");
    
    config.isConfigured = !config.wifi_ssid.isEmpty() && !config.room_name.isEmpty();
    
    prefs.end();
    
    Serial.printf("Loaded config: building=%s room=%s ssid=%s\n", 
                  config.building_name.c_str(), config.room_name.c_str(), config.wifi_ssid.c_str());
}

bool saveConfig(const String& ssid, const String& pass, const String& building, const String& room) {
    prefs.begin("climate-cfg", false);
    
    String secret = String((uint32_t)ESP.getEfuseMac(), HEX) + String(random(0xFFFFFF), HEX);
    
    prefs.putString("wifi_ssid", ssid);
    prefs.putString("wifi_pass", pass);
    prefs.putString("building", building);
    prefs.putString("room", room);
    prefs.putString("secret", secret);

    bool success = prefs.getString("wifi_ssid", "") == ssid;
    prefs.end();
    return success;
}

String lowerFormat(const String& str) {
    String s = str;
    s.toLowerCase();
    s.replace(" ", "-");

    return s;
}