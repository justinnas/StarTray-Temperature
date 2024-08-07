<div align="center">
  <img src="https://github.com/user-attachments/assets/65abf490-58d8-42d7-acb4-bd4e593dbdf7" alt="StarTray" height="200">
</div>


## StarTray

**StarTray** - super lightweight, aesthetic, and easy-to-use open-source application for monitoring your computer’s processor’s and graphics card’s temperatures from the system tray.

**Supports**: Windows 10 and Windows 11 64-bit (x64) operating systems.

**Developed and designed by** [@justinnas](https://github.com/justinnas)

<br>

## Download

You can download the latest version of StarTray from [GitHub Releases](https://github.com/justinnas/StarTray-Temperature/releases). Scroll down to 'Assets' section and choose between the installer or the portable version based on your preference.

<br>

***Disclaimer:** This appliaction requires administrative privileges in order to read hardware temperature data. Since this application is new, it might trigger some antivirus systems. If you prefer, you can review the source code and compile the application yourself.*

<br>

## Screenshots
***Icons can be customized through the themes menu.***
<div display="flex">
<img src="https://github.com/user-attachments/assets/76065f81-ba0a-40bd-9435-693942228ac4" height="100">
<img src="https://github.com/user-attachments/assets/11d6266f-b3bb-4dfd-aaf0-54aa7db3aee8" height="100">
<img src="https://github.com/user-attachments/assets/1a3a37aa-e4d4-4dee-9056-d82b64b0bb69" height="100">
<img src="https://github.com/user-attachments/assets/4e827dc7-9aee-46bd-a0a4-7f14a4d8011b" height="100">
<img src="https://github.com/user-attachments/assets/b2935b69-e9e1-41c4-99c4-7793dcb18788" height="100">
<img src="https://github.com/user-attachments/assets/0bfddd1c-c068-4d33-982f-97c528865943" height="100">
</div>

<br>

## Usage

This application is very simple to use. After launching the application, you will see CPU and/or GPU icons in the system tray. 
You can right-click one of these icons to open the menu panel. 
Right-clicking GPU icon brings up GPU icon menu, right-clicking CPU icon brings up CPU icon menu.

### Menu Panel

**Theme** *(Specific to an icon)*

- **Change Icon Theme:**
    - Hover over the "Theme" tab.
    - Select either "CPU theme" or "GPU theme" and click on the desired theme to change the icon's appearance.

**Options** *(Global, applies to both icons)*

- **Show/Hide Icons:**
    - Hover over the "Options" tab.
    - Select "Show GPU icon" or "Show CPU icon" to enable or disable the respective icon.
- **Run on Startup:**
    - Hover over the "Options" tab.
    - Select "Run on Startup" to enable or disable the application’s ability to start when your system boots up.
- **Change Temperature Units:**
    - Hover over the "Options" tab.
    - Choose "Change to Fahrenheit" or "Change to Celsius" to switch between temperature units.

**Info** *(Specific to an icon)*

- **View Hardware Info:**
    - Hover over the "Info" tab to display your processor's or graphics card's name.

**Exit** *(Global, applies to both icons)*

- **Close the Application:**
    - Click the "Exit" button to close StarTray.

<br>

## Source Code

If you prefer, you can review the source code and compile the application yourself. After compiling the application, copy the 'Resources' and 'Licenses' folders and their contents to the same directory as the compiled .exe file.

<br>

## License

This project uses the following libraries: LibreHardwareMonitorLib (MPL 2.0 License), HidSharp (Apache License), 
System.CodeDom (MIT License), System.Management (MIT License), TaskScheduler (MIT License).

This project uses the Open Sans font, designed by Steve Matteson, licensed under SIL Open Font License, Version 1.1.

Their respective license files can be found [here](https://github.com/justinnas/StarTray-Temperature/tree/main/Licenses).

StarTray is licensed under GNU General Public License v3.0 license, please see the [license file](https://github.com/justinnas/StarTray-Temperature/blob/main/LICENSE.txt) for more details.
