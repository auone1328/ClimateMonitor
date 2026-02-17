#ifndef CONFIG_H
#define CONFIG_H

#include <Preferences.h>

extern Preferences prefs;

struct DeviceConfig {
    String wifi_ssid;
    String wifi_pass;
    String building_name;
    String room_name;
    bool isConfigured = false;
};

extern DeviceConfig config;

void loadConfig();
bool saveConfig(const String& ssid, const String& pass, const String& building, const String& room);
String lowerFormat(const String& str); 

#endif