# ToroidalWorld
2D survival game set in a toroidal world generated with Perlin noise.

## ▶️ Run directly (recommended)
Download `ToroidalWorld-v1.0.1.zip` from the [Releases](../../releases/latest) section, extract it and run `ToroidalWorld.exe` from inside the extracted folder.

## 🔧 Build from source
If you prefer to build the project yourself, you need:
1. [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **.NET desktop development** workload
2. [.NET 8 SDK](https://dotnet.microsoft.com/download) or later

Once installed:
1. Clone the repository
2. Open `ToroidalWorld.sln` in Visual Studio
3. Build and run

## 📁 Resources folder
The `Resources/` folder contains all game content and can be edited without recompiling:
- **`Config/`** — JSON files defining all game entities (ships, enemies, turrets, projectiles, waves...). You can modify stats or add new entities by following the same format.
- **`Sprites/`** — `.png` textures, each paired with a `.json` file of the same name defining the spritesheet (frame size and animations).
- **`Textures/`** — background textures and other static visual elements.
- **`Music/`** — background music in `.ogg` format.
- **`SoundEffects/`** — sound effects in `.wav` format.
