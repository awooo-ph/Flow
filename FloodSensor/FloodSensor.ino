#include "Settings.h"
#include "Sms.h"

#define VERSION "1.0.0"

#define SIREN_POWER 7

uint8_t WaterLevel = 0;
uint8_t _prevWaterLevel = 0;
unsigned long _waterLevelChanged = 0;
unsigned long _lastLevelCheck = 0;

uint8_t WaterPIN[] = {A0,A1,A2,A3,A6};
uint8_t SirenPIN[] = {4,5,6};
uint8_t SignalPIN[] = {8,9,10,11};

void OnWaterLevelChanged(uint8_t level)
{
    digitalWrite(SIREN_POWER, HIGH);
    delay(777);

    for (auto i=0;i<3;i++)
    {
        auto pin = SirenPIN[i];
        if(Settings.Current.SirenLevel[i]==level)
        {
            digitalWrite(SIREN_POWER, LOW);
            delay(777);
            digitalWrite(pin,LOW);
        } else
        {
            digitalWrite(pin,HIGH);
        }
    }

    if(Settings.Current.NotifyLevel[level])
    {
        char * msg = ".0";
        sprintf(msg,".%d",level);
        for(auto number:Settings.Current.Monitor)
            Sms.send(msg,number);
        for(auto number:Settings.Current.NotifyNumbers)
            Sms.send(Settings.Current.LevelMessage[level],number);
    }
}

void GetLevel()
{
    if(millis()-_lastLevelCheck<4444) return;

    uint8_t level = 0;

    for (uint8_t i=0; i<4;i++)
        if(digitalRead(WaterPIN[i])) level = i;    

    
    if(WaterLevel!=level)
    {
        if(millis()-_waterLevelChanged<7777)
        {
            WaterLevel = _prevWaterLevel;
        } else
        {
            _waterLevelChanged = millis();
            _prevWaterLevel = WaterLevel;
            WaterLevel = level;
            OnWaterLevelChanged(level);
        }
    }
}

void onReceive(char* number, char* message)
{
    if(message[0]=='?')
    {
        Sms.startSend(number);
        Sms.write("!");
        Sms.write(message[1]);
        switch (message[1])
        {
        case 'v':
            Sms.write(VERSION);
            break;
        case 'n':
            Sms.write(Settings.Current.SensorName);
            break;
        case 'i':
            Sms.write(Sms.getIMEI());
            break;
        }

        Sms.commitSend();
    }
}



void setup() {
    pinMode(SIREN_POWER,OUTPUT);
    digitalWrite(SIREN_POWER,HIGH);

    for (auto i : WaterPIN) pinMode(i, INPUT_PULLUP);
    for (auto i : SirenPIN)
    {
        pinMode(i, OUTPUT);
        digitalWrite(i,HIGH);
    }

    Settings.LoadConfig();
    Sms.init();
    Sms.onReceive(onReceive);
}

// the loop function runs over and over again until power down or reset
void loop() {
    GetLevel();
    Sms.update();
}
