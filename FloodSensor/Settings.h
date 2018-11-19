// Settings.h

#ifndef _SETTINGS_h
#define _SETTINGS_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <EEPROM.h>
#include <avr/pgmspace.h>

struct Config
{
    unsigned int LevelTimeout = 7777;
    byte SirenLevel[3] = { 1,2,3 };
    bool NotifyLevel[5] = { true,true,true,true,true };
    char * Monitor[4];
    char * NotifyNumbers[17];
    char * SensorName;
    char * Location;
    unsigned int checksum;
};

class SettingsClass
{
private:


public:
    void SaveConfig();
    void ResetConfig();
    void LoadConfig();

    Config Current;
};

extern SettingsClass Settings;
#endif