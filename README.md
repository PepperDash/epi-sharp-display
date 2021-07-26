# Sharp Display Essentials Plugin (c) 2021

## Overview

This plugin is designed to work with Sharp Displays controlled via TCP/IP or RS-232. For config information, see the [config snippets](##Configuration)

## Configuration

### RS-232

```json
{
  "key": "display-1",
  "uid": 4,
  "type": "sharpDisplay",
  "name": "Main Display",
  "group": "display",
  "properties": {
    "control": {
      "controlPortDevKey": "processor",
      "controlPortNumber": 1,
      "method": "com",
      "comParams": {
        "protocol": "RS232",
        "baudRate": 9600,
        "hardwareHandshake": "None",
        "softwareHandshake": "None",
        "dataBits": 8,
        "parity": "None",
        "stopBits": 1
      }
    },
    "Enabled": true,
    "Name": "Main Display",
    "Protocol": ""
  }
}
```

### TCP/IP

```json
{
  "key": "display-1",
  "uid": 4,
  "type": "sharpDisplay",
  "name": "Main Display",
  "group": "display",
  "properties": {
    "control": {
      "method": "tcpIp",
      "tcpSshProperties": {
        "port": 22,
        "address": "0.0.0.0",
        "username": "",
        "password": "",
        "autoReconnect": true,
        "autoReconnectIntervalMs": 5000,
        "bufferSize": 32768
      }
    },
    "Enabled": true,
    "Name": "Main Display",
    "Protocol": ""
  }
}
```

### Available Protocols

| Protocol | Description           | Config Value |
| -------- | --------------------- | ------------ |
| PN-UH501 | For PN-UH501 Displays | PN-UH501     |

- All other values including an empty string will use the default command format.

## License

Provided under [MIT License](LICENSE.md)

# Contributing

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They are referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

To install dependencies once nuget.exe is installed, run the following command from the root directory of your repository:
`nuget install .\packages.config -OutputDirectory .\packages -excludeVersion`.
To verify that the packages installed correctly, open the plugin solution in your repo and make sure that all references are found, then try and build it.

### Installing Different versions of PepperDash Core

If you need a different version of PepperDash Core, use the command `nuget install .\packages.config -OutputDirectory .\packages -excludeVersion -Version {versionToGet}`. Omitting the `-Version` option will pull the version indicated in the packages.config file.
