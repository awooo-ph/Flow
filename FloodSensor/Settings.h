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
    byte SirenLevel[3] = { 1,2,3 };
    byte NotifyLevel[5] = { true,true,true,true,true };
    char * Monitor[4];
    char * NotifyNumbers[7];
    char * LevelMessage[5];
    char * SensorName;
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