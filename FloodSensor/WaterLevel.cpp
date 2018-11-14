// 
// 
// 

#include "WaterLevel.h"

void WaterLevel::init(uint8_t level1, uint8_t level2, uint8_t level3, uint8_t level4, uint8_t level5)
{
    _levelPIN[0] = level1;
    _levelPIN[1] = level2;
    _levelPIN[2] = level3;
    _levelPIN[3] = level4;
    _levelPIN[4] = level5;
    for (auto i : _levelPIN)
        pinMode(i, INPUT_PULLUP);
}

void WaterLevel::onLevelChange(void(*callback)(uint8_t level))
{
    onLevelChangeCallback = callback;
}

void WaterLevel::update()
{
    //if (millis() - _lastLevelCheck < 4444) return;

    uint8_t level = 0;

    for (uint8_t i = 0; i < 5; i++)
    {
        auto pin = digitalRead(_levelPIN[i]);
        if (pin) level = i+1;
    }
    
    if (_currentLevel != level && millis() - _waterLevelChanged > 7777)
    {
        _waterLevelChanged = millis();
        _currentLevel = level;
        if (onLevelChangeCallback) onLevelChangeCallback(level);        
    }
}
