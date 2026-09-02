# Despliegue de demostración con Docker y Render

El `Dockerfile` permite enseñar cómo se empaqueta una aplicación ASP.NET Core sin
convertir el despliegue en un requisito de la práctica. Usa dos etapas: una con el SDK
para compilar y otra con solo el runtime para ejecutar la aplicación.

## Probar el contenedor en local

Desde la raíz del repositorio:

```bash
docker build -t biblioteca-aspnet .
docker run --rm -p 10000:10000 biblioteca-aspnet
```

Abrir `http://localhost:10000`. El contenedor crea SQLite en `App_Data` y las imágenes
subidas bajo `wwwroot/uploads`.

## Crear el servicio en Render

1. Subir este repositorio a GitHub.
2. En Render, elegir **New > Web Service** y conectar el repositorio.
3. Elegir el runtime **Docker**. Si el archivo sigue en la raíz, dejar `Dockerfile Path`
   con su valor por defecto.
4. Elegir región y plan, y crear el servicio. No hace falta indicar un comando de inicio:
   Render ejecuta el `ENTRYPOINT` del `Dockerfile`.
5. Definir `Health Check Path` como `/` (opcional, pero didácticamente útil).
6. Esperar el primer despliegue y abrir la URL `*.onrender.com` que muestra Render.

El `ENTRYPOINT` escucha en `0.0.0.0` y usa la variable `PORT` que Render entrega. Si
Render no define esa variable durante una prueba local, usa el puerto `10000`.

## Aviso importante sobre SQLite e imágenes

Este despliegue sirve para demostrar Docker y Render, no para producción. Render usa
un sistema de archivos efímero por defecto: la base SQLite y cualquier avatar o imagen
subida se pierden al reiniciar o redesplegar. Los datos demo versionados sí vuelven a
cargarse al iniciar.

Para una demostración persistente se puede contratar un disco de Render y montarlo en
una ruta bajo `/app` (por ejemplo `/app/storage`), pero habría que configurar tanto la
cadena de conexión SQLite como el directorio de subidas para escribir dentro de esa
ruta. Para una aplicación real con varios usuarios conviene usar Render Postgres y
almacenamiento de objetos para imágenes.

Referencias oficiales: [Docker on Render](https://render.com/docs/docker),
[Web Services y puertos](https://render.com/docs/web-services) y
[Persistent Disks](https://render.com/docs/disks).
