#include "Settings.h"

void SettingsClass::LoadConfig()
{

	Current = Config();
	Current.checksum = 0;
	unsigned int sum = 0;
	unsigned char t;

	for (unsigned int i = 0; i < sizeof(Current); i++) {
		t = (unsigned char)EEPROM.read(i);
		*((char *)&Current + i) = t;
		if (i < sizeof(Current) - sizeof(Current.checksum)) {
			/* Don't checksum the checksum! */
			sum = sum + t;
		}
	}

	/* Now check the data we just read */
	if (Current.checksum != sum) {
		Current = Config();
	} 
	
}

void SettingsClass::ResetConfig() {
	Current = Config();
	SaveConfig();
}

void SettingsClass::SaveConfig() {
	unsigned int sum = 0;
    
	unsigned int size = sizeof(Config);

	for (unsigned int i = 0; i < size; i++) {

		if (i == sizeof(Current) - sizeof(Current.checksum)) {
			Current.checksum = sum;
		}

	    const auto t = *((unsigned char*)&Current + i);
		if (i < sizeof(Current) - sizeof(Current.checksum)) {
			sum = sum + t;
		}

		EEPROM.write(i,t);
		
	}

}

SettingsClass Settings;