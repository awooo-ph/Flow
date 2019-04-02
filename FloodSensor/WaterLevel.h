// WaterLevel.h

#ifndef _WATERLEVEL_h
#define _WATERLEVEL_h

#define LEVEL1 7
#define LEVEL2 6
#define LEVEL3 5
#define LEVEL4 4
#define LEVEL5 3

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include "Settings.h"

class WaterLevel
{
private:
    void (*onLevelChangeCallback)(uint8_t level) = nullptr;
    uint8_t _currentLevel = 0;
    uint8_t _newWaterLevel = 0;
    uint8_t _levelPIN[5] = { LEVEL1,LEVEL2,LEVEL3,LEVEL4,LEVEL5 };
    //uint8_t _levelEnablePin = 10;
    unsigned long _lastLevelCheck = 0;
    unsigned long _waterLevelChanged = 0;

 public:
    void init(uint8_t level1, uint8_t level2, uint8_t level3, uint8_t level4, uint8_t level5,uint8_t levelEnable);
    void onLevelChange(void (*onLevelChangeCallback)(uint8_t level));
    void update();
    uint8_t getLevel() {return _currentLevel;}
};

#endif