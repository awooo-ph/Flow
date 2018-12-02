// Settings.h

#ifndef _SETTINGS_h
#define _SETTINGS_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <EEPROM.h>

struct Config
{
    unsigned int LevelTimeout = 4444;
    int SirenLevel[3] = { 1,2,3 };
    int WarningLevel = 0;
    char * SensorName;
    char * Location;
    char Monitor[15] = "09334875443";
    char Sensors[7][15];
    //char NotifyNumbers[4][15];
    
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