# 🎵 BobekkBoi Sound Board - My personal soundboard

**BobekkBoi Sound Board** is a modern, highly optimized audio player (`.wav`) for Windows, built on **WPF (.NET 10)** technology.

The application was designed with a **"Zero Bloat"** philosophy. It is a clean tool that installs no services, does not run in the background, and **creates no configuration files or databases**. All behavior is defined dynamically at runtime via launch arguments.

---

<img width="950" height="600" alt="{3452F7A7-D3F2-4E6A-939A-19FAA5BF315F}" src="https://github.com/user-attachments/assets/e13d26f8-e315-42af-9df7-0272fb7a308f" />


## ✨ Key Features

* **⚡ Instant Response** – Asynchronous directory navigation (Batch Loading) and aggressive RAM management.
* **🎨 Custom UI** – Custom dark window design (Dark Mode) without standard Windows frames.
* **🔊 Audio Engine** – Built on the **NAudio** library. Supports mixing multiple sounds simultaneously (Soundboard mode).
* **🖥️ Kiosk Ready** – Special modes for public terminals (hide title bar, disable keyboard, always on top).
* **🛡️ Security** – Options to lock controls and hide system/hidden files.
* **📂 Stateless** – The app remembers nothing. It leaves no data or registry entries behind after closing.

---

## 🛠️ Command Line Interface (Arguments)

The application is configured **exclusively via launch arguments**. This allows you to create different shortcuts for specific purposes (e.g. *Kiosk*, *Stream Deck*, *Admin*).

| Argument            | Description                                                                                | Example              |
| ------------------- | ------------------------------------------------------------------------------------------ | -------------------- |
| **`-start "path"`** | Sets the directory to load immediately upon startup.                                       | `-start "C:\Sounds"` |
| **`-home "path"`**  | Defines the directory the **Home** button returns to.                                      | `-home "D:\Music"`   |
| **`-multi`**        | Enables simultaneous playback of multiple sounds. Default is *One at a time*.              | `-multi`             |
| **`-max`**          | Displays the maximize button (hidden by default).                                          | `-max`               |
| **`-maxstart`**     | Starts the application maximized across the entire screen.                                 | `-maxstart`          |
| **`-top`**          | Enables **Always on Top** (TopMost) mode. The window stays above other apps.               | `-top`               |
| **`-nobar`**        | Completely hides the top title bar and locks window resizing. Ideal for touch panels.      | `-nobar`             |
| **`-fullscreen`**   | **Kiosk Mode.** Hides the title bar, maximizes the window, and covers the Windows Taskbar. | `-fullscreen`        |
| **`-screen <id>`**  | Launches the app on a specific monitor (0 = primary, 1 = secondary…).                      | `-screen 1`          |
| **`-nokey`**        | **Security.** Completely disables keyboard input (cannot type path, cannot use Enter).     | `-nokey`             |
| **`-clean`**        | **Filter.** Hides files and folders marked as *System* or *Hidden*.                        | `-clean`             |

---

## 💡 Usage Examples (Scenarios)

### 1️⃣ Public Kiosk (Secure Terminal)

Launches the app on the second monitor, full screen (covering the taskbar), with the keyboard disabled and system files hidden.

```cmd
BobekkBoiSoundBoard.exe -screen 1 -fullscreen -nokey -clean -start "C:\PublicSounds"
```

### 2️⃣ Streamer / Soundboard

Window is always on top and allows mixing ("spamming") multiple sound effects at once.

```cmd
BobekkBoiSoundBoard.exe -top -multi -home "D:\StreamAssets\Sfx"
```

### 3️⃣ Static Widget Window

A window without a title bar that looks like a desktop component.

```cmd
BobekkBoiSoundBoard.exe -nobar -start "C:\Music\Ambient"
```

---

## 🎮 Controls

* **Left Mouse Click** – Play sound / Open folder
* **TextBox (Path)** – Manually enter a path (unless `-nokey` is active)
* **Enter** – Confirm selection / Play (unless `-nokey` is active)
* **Stop All Button** – Immediately stops all playing sounds

---

## 💾 Data Policy (Stateless)

This program uses **no external data files** for its operation.

* ❌ No `config.xml`, `settings.json`, or `.ini` files
* ❌ No local database (SQLite / LocalDB)
* ❌ No writing to the Windows Registry

Application behavior is **100% determined by launch arguments**, guaranteeing maximum portability and system cleanliness.

---

## ❗ Known Issues

 ```cmd
-start "C:\" -home "C:\"
```
### If this argument does not work, try this:
 ```cmd
-start "C:\\" -home "C:\\"
```

---

## 📋 Requirements

* **OS:** Windows 10 / 11 (x64)
* **Runtime:** .NET 8.0 Desktop Runtime

### 🧠 Memory (RAM) Usage Guidelines

* **At least 4 GB RAM** recommended for a *music library*
* **At least 2 GB RAM** recommended for a *library of sound effects*
* <ins>**At least 1 GB RAM** sufficient for *a couple of sounds*</ins>

> The application itself is lightweight and **can run with as little as ~500 MB RAM**, depending on usage.

*eh.. AI told so, but I think 500 MB is quite a lot for an app that just plays sounds… at least they play instantly, ay*

---

Built as a custom solution with a strong focus on **performance**, **portability**, and **modularity**.

---

# Mostly Gemini and ChatGPT built this program and documentation in about 6 hours. I take very little credit for this.
