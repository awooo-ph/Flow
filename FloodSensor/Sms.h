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
    char BUFFER[147];
    int BUFFER_INDEX = 0;
    bool _parsingData = false;
    unsigned int _initStart = 0;
    bool _isReady = false;
    bool _isRegistered = false;
    long _lastCREG = 0;
    uint8_t _simStatus = 0;
    bool waitOk();
    bool readLine();
    int csq = 0;
    void processCSQ(char command[]);
    void parseData(char * data);
    void parseNumber(const char * data, char* number);
    bool startsWith(const char *pre, const char *str);
    void parseSMS(char* command);
    //bool isAdmin(char * number);
    bool _modemDetected = false;
    uint8_t errorCode = 0;
    bool _smsSendStarted = false;
    void(*_onSignalChanged)(int) = nullptr;
    void(*_onSimNumberChanged)(void) = nullptr;
    unsigned long _lastCSQ=0;
    unsigned long _lastCNUM=0;
    void parseCNUM(char * data);
    void ProcessSettings(char *);
    void ProcessSensors(char * message);

public:
    SmsClass(uint8_t rx, uint8_t tx);
    uint8_t getError() { return errorCode; }
    bool init();
    void onSignalChanged(void(*)(int));
    void onNumberChanged(void(*)(void));
    bool modemDetected(){return _modemDetected;}
    bool isReady() { return _isReady; }
    uint8_t getSimStatus() {return _simStatus;};
    bool isRegistered() { return _isRegistered; }
    int getRSSI() { return csq; }
    int getSignal();
    void update();
    void send(char * number,char * message);
    
    void getIMEI(char *);
    bool startSend(char * number);
    bool write(char * message);
    bool write(char text);
    bool commitSend();
    void cancelSend();
    void restart();
    void getNumber(char *);
    //void readUnread();
    //void sendWarning(uint8_t);
};

#endif