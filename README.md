# Biblioteca ASP.NET

Proyecto de referencia del curso para aprender C# y ASP.NET Core MVC. Incluye usuarios,
catálogo, carrito, pedidos, imágenes y datos demo. No necesita un servidor de base de
datos: usa SQLite local.

## Requisitos

- SDK de .NET 10. `global.json` selecciona el SDK 10.0.400 o una actualización
  compatible.
- Visual Studio Code y C# Dev Kit. Es el IDE estándar del curso en Windows, macOS y
  Linux.
- Git. Docker es opcional y solo se usa al explicar despliegue.

No instales ASP.NET, C#, SQLite ni Entity Framework por separado: los aporta el SDK o
se restauran como dependencias del proyecto.

## Instalación inicial

Instala .NET 10 SDK, Visual Studio Code y C# Dev Kit una sola vez en el ordenador.

### Windows (PowerShell)

```powershell
winget install --id Microsoft.DotNet.SDK.10 --exact
winget install --id Microsoft.VisualStudioCode --exact
winget install --id Git.Git --exact
```

Cierra y abre una terminal nueva; después instala la extensión:

```powershell
code --install-extension ms-dotnettools.csdevkit
```

### macOS (Terminal, con Homebrew)

```bash
brew install dotnet
brew install --cask visual-studio-code
brew install git
code --install-extension ms-dotnettools.csdevkit
```

Si no utilizas Homebrew, instala el SDK de .NET 10 con el instalador oficial de
[macOS](https://dotnet.microsoft.com/download/dotnet/10.0) y añade C# Dev Kit desde
el panel Extensions de VS Code.

### Ubuntu 26.04 (Terminal)

```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0 git
sudo snap install code --classic
code --install-extension ms-dotnettools.csdevkit
```

Para otra distribución Linux, sigue el instalador oficial de
[.NET para Linux](https://learn.microsoft.com/dotnet/core/install/linux) y, una vez
instalado VS Code, ejecuta el último comando.

Comprueba la instalación desde esta carpeta:

```bash
dotnet --version
```

`dotnet --version` debe mostrar `10.0.400` o una actualización compatible de .NET 10.
Si el comando `code` no se reconoce, abre VS Code e instala C# Dev Kit desde
**Extensions**.

## Primer arranque

Abre la carpeta `biblioteca-aspnet` en VS Code, no la carpeta padre. La extensión
carga automáticamente `BibliotecaAspNet.sln`.

```bash
code .
dotnet restore BibliotecaAspNet.sln
dotnet tool restore
dotnet run --project BibliotecaAspNet.csproj --launch-profile http
```

Abre `http://localhost:5251`. El primer arranque crea `App_Data/biblioteca.db`, aplica
las migraciones y añade datos demo.

Usuarios de demo:

- `admin` / `Admin123!`
- `user` / `User123!`

Tarjeta de checkout ficticia: `4242 4242 4242 4242`, caducidad `12/30`, CVV `123`.

## Comandos de trabajo diario

Todos se ejecutan desde la raíz de este repositorio.

```bash
# Restaurar dependencias NuGet y herramientas locales tras clonar o actualizar
dotnet restore BibliotecaAspNet.sln
dotnet tool restore

# Ejecutar la aplicación
dotnet run --project BibliotecaAspNet.csproj --launch-profile http

# Ejecutar con recarga al guardar archivos
dotnet watch --project BibliotecaAspNet.csproj run --launch-profile http

# Compilar y ejecutar los tests
dotnet build BibliotecaAspNet.sln
dotnet test BibliotecaAspNet.sln
```

### Entity Framework y SQLite

`dotnet-ef` está fijado en `.config/dotnet-tools.json`, por lo que `dotnet tool
restore` lo deja disponible sin instalar nada de forma global.

```bash
# Después de cambiar una entidad: crear y aplicar una migración
dotnet ef migrations add NombreDescriptivo
dotnet ef database update

# Corregir la última migración solo antes de aplicarla a la base de datos
dotnet ef migrations remove
```

Las migraciones se aplican también al arrancar la aplicación en desarrollo. El comando
`database update` se incluye para aprender el flujo explícito que se usará en clase.

### Git, CI y Docker

```bash
git status
git add .
git commit -m "feat: describe el cambio"
git pull --rebase
git push
```

[GitHub Actions](.github/workflows/build-and-test.yml) ejecuta restauración,
compilación Release y tests en cada `push` y *pull request*.

Docker es opcional:

```bash
docker build -t biblioteca-aspnet:local .
docker run --rm -p 10000:10000 biblioteca-aspnet:local
```

El contenedor queda disponible en `http://localhost:10000`.

## IDEs y guías de clase

En VS Code, selecciona el perfil `http` y pulsa F5 para depurar. Las tareas
`Biblioteca: compilar` y `Biblioteca: ejecutar tests` están disponibles en
**Terminal → Run Task**. En Windows, Visual Studio 2026 también abre
`BibliotecaAspNet.sln`; consulta [la guía de IDEs](docs/IDE-SETUP.md).

- [Cómo leer el código y las asociaciones](docs/GUÍA-CÓDIGO.md)
- [Qué conservar al crear el repositorio de un grupo](docs/BASE-COMUN-GRUPOS.md)
- [Docker y despliegue de demostración en Render](docs/DESPLIEGUE-RENDER.md)
