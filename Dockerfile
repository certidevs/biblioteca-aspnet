# Etapa de compilación: incluye SDK, restauración de paquetes y publicación Release.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BibliotecaAspNet.csproj", "./"]
RUN dotnet restore "BibliotecaAspNet.csproj"

COPY . .
RUN dotnet publish "BibliotecaAspNet.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

# Imagen final pequeña: solo contiene el runtime necesario para ejecutar ASP.NET Core.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Son directorios de escritura en desarrollo y en un despliegue de demostración.
RUN mkdir -p App_Data wwwroot/uploads/avatars wwwroot/uploads/author-photos wwwroot/uploads/book-covers

EXPOSE 10000

# Render define PORT. El valor 10000 permite ejecutar también el contenedor localmente.
ENTRYPOINT ["sh", "-c", "exec dotnet BibliotecaAspNet.dll --urls http://0.0.0.0:${PORT:-10000}"]
