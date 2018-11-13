// Sms.h

#ifndef _SMS_h
#define _SMS_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include <SoftwareSerial.h>

class SmsClass
{
 private:
    SoftwareSerial* sms;
    bool _isReady = false;
    bool _isRegistered = false;
    long _lastCREG = 0;
    bool waitOk();
    char * readLine();
    uint8_t csq=0;
    void processCSQ(char command[]);
    void parseData(char * data);
    char * getNumber(const char * data);
    bool startsWith(const char *pre,const char *str);
    void parseSMS(char* command);
    bool isAdmin(char * number);
    void (*onReceiveCallback)(char* number, char* message) = nullptr;
    
 public:
    SmsClass(uint8_t rx, uint8_t tx);
	void init();
    bool isReady(){return _isReady;}
    bool isRegistered(){return _isRegistered;}
    uint8_t getRSSI(){return csq;}
    uint8_t getSignal();
    void update();
    void send(char * message, char * number);
    void onReceive(void (*callback)(char * number,char * message));
    char * getIMEI();
    void startSend(char * number);
    void write(char * message);
    void write(char text);
    void commitSend();
    
};

#endif