# August Plugin

August Plugin is a **BepInEx plugin for the [Bassment August Server](https://github.com/bassmentrr/August-Server)**.

It is primarily designed for an **August 17th, 2016 build of Rec Room**, and is intended to work alongside the [Bassment August Server](https://github.com/bassmentrr/August-Server).

From what I investigated, this plugin should work from the first ever version of Rec Room (June 28th, 2016) to August 30th, 2016, as they added levels in that version!

## Important!

If you want the version that I was using to test, put this into the Steam Console:

download_depot 471710 471711 7115392148801672603

Make sure to put a "steam_appid.txt" inside the root of the install. I set the ID inside the .txt to 471710 (Rec Room's ID) but if you want somebody who doesn't have Rec Room on Steam to play, make sure their "steam_appid.txt" is set to either 92 (Codename Gordon (steam://install/92)) or 480 (Spacewar (steam://install/480))

## Building

Make sure you have **.NET 9.0** installed.

### 1. Clone the repository

```bash
git clone https://github.com/bassmentrr/August-Plugin.git
cd August-Plugin
```

or if you're lazy, just download the ZIP from GitHub!

### 2. Build the plugin

```bash
dotnet build
```

The compiled plugin will be located at:

```bash
bin/Debug/net9.0/BassmentPlugin.dll
```

### 3. Install the plugin

Copy `BassmentPlugin.dll` into your BepInEx plugins folder:

```bash
BepInEx/plugins/
```

Make sure **BepInEx is installed and working before installing the plugin**.

The version of BepInEx that I use during testing is 5.4.21.0!

In the BepInEx config, make sure under "[Preloader.Entrypoint]" that "Type" is set to MonoBehaviour!

## Releases

Pre-built versions of the plugin are available from the **Releases** page!

The releases contain the plugin DLL directly, so you can download `BassmentPlugin.dll` and drop it straight into your `BepInEx/plugins/` folder!

