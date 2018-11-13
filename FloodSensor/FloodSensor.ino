#include "Settings.h"
#include "Sms.h"
#include  "WaterLevel.h"

#define VERSION "1.0.0"
#define SIREN_POWER 7
#define SIGNAL_BARS 4
#define GSM_RX 11
#define GSM_TX 12

WaterLevel river;
SmsClass Sms(GSM_RX,GSM_TX);

uint8_t _prevWaterLevel = 0;
uint8_t SirenPIN[3] = { 4,5,6 };
uint8_t SignalPIN[SIGNAL_BARS] = { 8,9,10,11 };

void OnWaterLevelChanged(uint8_t level)
{
    digitalWrite(SIREN_POWER, HIGH);
    delay(777);

    for (auto i = 0; i < 3; i++)
    {
        auto pin = SirenPIN[i];
        if (Settings.Current.SirenLevel[i] == level)
        {
            digitalWrite(SIREN_POWER, LOW);
            delay(777);
            digitalWrite(pin, LOW);
        }
        else
        {
            digitalWrite(pin, HIGH);
        }
    }

    if (Settings.Current.NotifyLevel[level])
    {
        char * msg = ".0";
        sprintf(msg, ".%d", level);
        for (auto number : Settings.Current.Monitor)
            Sms.send(msg, number);
        for (auto number : Settings.Current.NotifyNumbers)
            Sms.send(Settings.Current.LevelMessage[level], number);
    }
}

void onReceive(char* number, char* message)
{
    if (message[0] == '?')
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
    pinMode(SIREN_POWER, OUTPUT);
    digitalWrite(SIREN_POWER, HIGH);

    river.onLevelChange(OnWaterLevelChanged);
    river.init(A0, A1, A2, A3, A6);

    for (auto i : SirenPIN)
    {
        pinMode(i, OUTPUT);
        digitalWrite(i, HIGH);
    }

    Settings.LoadConfig();

    Sms.onReceive(onReceive);
    Sms.init();

}

// the loop function runs over and over again until power down or reset
void loop() {
    river.update();
    Sms.update();

    // Signal LED
    for (auto i = 0; i < SIGNAL_BARS; i++)
        digitalWrite(SignalPIN[i], Sms.getSignal() > i);

}
