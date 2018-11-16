// 
// 
// 

#include "Display.h"



DisplayClass::DisplayClass(uint8_t address)
{
    lcd = new LiquidCrystal_I2C(address,16,2);
}

void DisplayClass::showWelcome(char * version)
{
    lcd->begin();
    lcd->setCursor(0,0);
    lcd->print(F("SFC - GUIHULNGAN"));
    lcd->setCursor(0,1);
    lcd->print(F("BSIM  BATCH 2019"));
    delay(4444);
    lcd->setCursor(0,0);
    lcd->print(F("FLOOD MONITORING"));
    lcd->setCursor(0,1);
    lcd->print(F("& WARNING SYSTEM"));
    delay(4444);
}

void DisplayClass::showLoading()
{
    lcd->setCursor(0,0);
    lcd->print(F("STARTING MODEM  "));
    lcd->setCursor(0,1);
    lcd->print(F("PLEASE WAIT...  "));
    delay(1111);
}

void DisplayClass::init()
{
    if(_initialized) return;
    _initialized = true;
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

void DisplayClass::setSignal(int signal)
{
    switch (signal)
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
}

void DisplayClass::setLevel(uint8_t level)
{
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
    lcd->setCursor(0, 1);
    lcd->print(F("              "));
    for(auto i=0;i<strlen(desc);i++)
    {
        if(i==14) return;
        lcd->setCursor(i, 1);
        lcd->print(desc[i]);
    }
    
}
