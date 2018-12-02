// 
// 
// 

#include "WaterLevel.h"

void WaterLevel::init(uint8_t level1, uint8_t level2, uint8_t level3, uint8_t level4, uint8_t level5,uint8_t levelEnable)
{
    _levelPIN[0] = level1;
    _levelPIN[1] = level2;
    _levelPIN[2] = level3;
    _levelPIN[3] = level4;
    _levelPIN[4] = level5;
    _levelEnablePin = levelEnable;
    for (auto i : _levelPIN)
        pinMode(i, INPUT_PULLUP);
    //pinMode(levelEnable, INPUT_PULLUP);
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
        if (pin == HIGH) level = i+1;
    }
    
    if(_newWaterLevel!=level)
    {
        _waterLevelChanged = millis();
        _newWaterLevel = level;
    } else
    {
        if(_currentLevel!=_newWaterLevel && millis() - _waterLevelChanged > 1111)
        {
            _currentLevel = _newWaterLevel;
            if (onLevelChangeCallback) onLevelChangeCallback(level);
        }
    }

}
