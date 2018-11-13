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
    for (auto i : _levelPIN) pinMode(i, INPUT_PULLUP);
}

void WaterLevel::onLevelChange(void(* callback)(uint8_t level))
{
    onLevelChangeCallback = callback;
}

void WaterLevel::update()
{
    if (millis() - _lastLevelCheck < 4444) return;

    uint8_t level = 0;

    for (uint8_t i = 0; i < 4; i++)
        if (digitalRead(_levelPIN[i])) level = i;


    if (_currentLevel != level)
    {
        if (millis() - _waterLevelChanged < 7777)
        {
            _currentLevel = _prevWaterLevel;
        }
        else
        {
            _waterLevelChanged = millis();
            _prevWaterLevel = _currentLevel;
            _currentLevel = level;
            if(onLevelChangeCallback) onLevelChangeCallback(level);
        }
    }
}
