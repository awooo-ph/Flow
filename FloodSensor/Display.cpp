// 
// 
// 

#include "Display.h"

DisplayClass::DisplayClass(uint8_t address)
{
    lcd = new LiquidCrystal_I2C(address,16,2);
    _address = address;
    //_lcdFound = lcd;
}

void DisplayClass::showWelcome()
{    
    if(!_lcdFound) return;
    lcd->setCursor(0,0);
    lcd->print(F("SFC - GUIHULNGAN"));
    lcd->setCursor(0,1);
    lcd->print(F("BSIM  BATCH 2019"));
    delay(1111);
    lcd->setCursor(0,0);
    lcd->print(F("FLOOD MONITORING"));
    lcd->setCursor(0,1);
    lcd->print(F("& WARNING SYSTEM"));
    delay(1111);
}

void DisplayClass::draw()
{
    if(!_lcdFound) return;

    lcd->clear();

    lcd->createChar(0,antenna);  
    lcd->createChar(1,blank);
    lcd->createChar(2,blank);
    lcd->createChar(3,water_level);
    lcd->createChar(4,water_level_2);
    lcd->createChar(5,blank);
    lcd->createChar(6,level0);

    lcd->setCursor(0, 0);
    lcd->write(0);

    lcd->setCursor(1, 0);
    lcd->write(1);
    lcd->setCursor(2, 0);
    lcd->write(2);
    lcd->setCursor(15, 0);
    lcd->write(3);

    lcd->setCursor(0, 1);
    lcd->print(F("-------"));
    lcd->setCursor(15, 1);
    lcd->write(4);

    lcd->setCursor(14, 0);
    lcd->write(5);
    lcd->setCursor(14, 1);
    lcd->write(6);

    setLevel(0);
    setSignal(0);
}

void DisplayClass::begin()
{
    lcd->begin();
    _lcdFound = true;
}

void DisplayClass::setSignal(int signal)
{
    _signal = signal;
}

void DisplayClass::setLevel(uint8_t level)
{
    if(!_lcdFound) return;

    if(level==_waterLevel) return;
    _waterLevel = level;
    lcd->setCursor(5, 0);
    lcd->print(F(" LEVEL "));
    lcd->print(level);
    switch (level)
    {
    case 0:
        lcd->createChar(5,blank);
        lcd->createChar(6,level0);
        break;
        case 1:
        lcd->createChar(5,blank);
        lcd->createChar(6,level1);
        break;
        case 2:
        lcd->createChar(5,blank);
        lcd->createChar(6,level2);
        break;
        case 3:
        lcd->createChar(5,level3_U);
        lcd->createChar(6,level3_L);
        break;
        case 4:
        lcd->createChar(5,level4);
        lcd->createChar(6,level3_L);
        break;
        case 5:
        lcd->createChar(5,level5);
        lcd->createChar(6,level3_L);
        break;
    }
    
}

void DisplayClass::setDescription(char* desc)
{
    strcpy(description,desc);
}

void DisplayClass::update()
{
    if(!_lcdFound) return;

    if(millis()-_lastUpdate<1111) return;
    _lastUpdate = millis();
    switch (_signal)
    {
    case -1:
        lcd->createChar(1, no_signal);
        lcd->createChar(2, no_signal);
        break;
    case 0:
        lcd->createChar(1, blank);
        lcd->createChar(2, blank);
        break;
    case 1:
        lcd->createChar(1, bars_1);
        lcd->createChar(2, blank);
        break;
    case 2:
        lcd->createChar(1, bars_2);
        lcd->createChar(2, blank);
        break;
    case 3:
        lcd->createChar(1, bars_2);
        lcd->createChar(2, bars_3);
        break;
    case 4:
        lcd->createChar(1, bars_2);
        lcd->createChar(2, bars_4);
        break;
    }

    lcd->setCursor(0, 1);
    auto len = strlen(description);
    for(auto i=0;i<14;i++)
    {
        if(i==14) return;
        lcd->setCursor(i, 1);
        if(i<len)
            lcd->print(description[i]);
        else
            lcd->print(F(" "));
    }
}
