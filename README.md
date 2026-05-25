# ToroidalWorld

Proyecto desarrollado como Trabajo de Fin de Grado (TFG).

## ▶️ Ejecutar directamente (recomendado)

Descarga el archivo `ToroidalWorld-v1.0.0.zip` desde la sección 
[Releases](../../releases/latest), descomprímelo y ejecuta `ToroidalWorld.exe` 
desde dentro de la carpeta resultante.

## 🔧 Compilar desde el código fuente

Si prefieres compilar el proyecto, necesitas:

1. [Visual Studio 2022](https://visualstudio.microsoft.com/) con el workload **.NET desktop development**
2. [.NET 8 SDK](https://dotnet.microsoft.com/download) o superior

Una vez instalado lo anterior:
1. Clona el repositorio
2. Abre `ToroidalWorld.sln` en Visual Studio
3. Compila y ejecuta

## 📁 Carpeta Resources

La carpeta `Resources/` contiene todo el contenido del juego y puede editarse sin recompilar:

- **`Config/`** — archivos JSON con las definiciones de entidades del juego (barcos, enemigos, torretas, proyectiles, oleadas...). Puedes modificar estadísticas o añadir nuevas entidades siguiendo el mismo formato.
- **`Sprites/`** — texturas en `.png`, cada una acompañada de un `.json` con el mismo nombre que define el spritesheet (tamaño de frame y animaciones).
- **`Textures/`** — texturas de fondo y otros elementos visuales estáticos.
- **`Music/`** — música de fondo en formato `.ogg`.
- **`SoundEffects/`** — efectos de sonido en formato `.wav`.
