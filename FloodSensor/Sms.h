#ifndef _SMS_h
#define _SMS_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <SoftwareSerial.h>
#include "Settings.h"

class SmsClass
{
private:
    SoftwareSerial * sms;
    bool _isReady = false;
    bool _isRegistered = false;
    long _lastCREG = 0;
    bool waitOk();
    void readLine(char data[]);
    int csq = 0;
    void processCSQ(char command[]);
    void parseData(char * data);
    char * parseNumber(const char * data);
    bool startsWith(const char *pre, const char *str);
    void parseSMS(char* command);
    bool isAdmin(char * number);
    uint8_t errorCode = 0;
    bool _smsSendStarted = false;
    void(*onReceiveCallback)(char* number, char* message) = nullptr;
    unsigned long _lastCSQ=0;
    unsigned long _lastCNUM=0;
    void parseCNUM(char * data);

public:
    SmsClass(uint8_t rx, uint8_t tx);
    uint8_t getError() { return errorCode; }
    bool init();
    bool isReady() { return _isReady; }
    bool isRegistered() { return _isRegistered; }
    int getRSSI() { return csq; }
    int getSignal();
    void update();
    void send(char * number,char * message);
    void onReceive(void(*callback)(char * number, char * message));
    char * getIMEI();
    bool startSend(char * number);
    bool write(char * message);
    bool write(char text);
    bool commitSend();
    void cancelSend();
    void restart();
    void getNumber(char num[]);
    void readUnread();
    void sendWarning(uint8_t);

};

#endif