# Abrir Biblioteca ASP.NET en clase

Los comandos de instalación y de trabajo diario están en el
[README](../README.md). El entorno estándar es Visual Studio Code con C# Dev Kit,
porque funciona igual en Windows, macOS y Linux.

## Visual Studio Code

1. Abre la carpeta `biblioteca-aspnet`, no la carpeta que contiene los dos proyectos.
2. Acepta la recomendación de C# Dev Kit si aparece.
3. C# Dev Kit carga `BibliotecaAspNet.sln` y los perfiles de
   `Properties/launchSettings.json`.
4. Selecciona el perfil `http` y pulsa F5 para ejecutar y depurar.

Las tareas `Biblioteca: compilar` y `Biblioteca: ejecutar tests` aparecen en
**Terminal → Run Task**.

## Visual Studio 2026 (solo Windows)

1. En Visual Studio Installer, instala la carga de trabajo **ASP.NET and web
   development**.
2. Abre `BibliotecaAspNet.sln`.
3. Selecciona el perfil `http` junto al botón de inicio y pulsa F5.

No hay dos versiones del proyecto: los perfiles, migraciones, solución y tests son
los mismos que usa VS Code.
