# Biblioteca ASP.NET

Proyecto de referencia del curso, equivalente a
[proyecto_biblioteca](https://github.com/alansastre/proyecto_biblioteca), implementado
con C# y ASP.NET Core MVC. Está pensado para que se pueda leer desde cero y reutilizar
como base de los proyectos de grupo.

## Stack

- .NET 10 LTS y C# 14.
- ASP.NET Core MVC: controladores y vistas Razor (`.cshtml`).
- Entity Framework Core 10 + SQLite, equivalente práctico de JPA/Hibernate + H2.
- ASP.NET Core Identity: registro, login por cookie, roles y contraseñas.
- Bootstrap 5.3, Font Awesome 7 y modo claro/oscuro.
- Docker para explicar el empaquetado y un despliegue demostrativo en Render.

SQLite se guarda localmente en `App_Data/biblioteca.db`; no requiere instalar un
servidor de base de datos. Las migraciones crean el esquema y el inicializador aporta
datos demo al primer arranque.

## Funcionalidades incluidas

- CRUD de libros, autores y categorías para el rol administrador.
- Búsquedas, filtros, favoritos y reseñas.
- Registro, login, logout, perfil, avatar y cambio de contraseña.
- Administración de usuarios: alta, edición, rol, estado y borrado seguro.
- Carrito con cantidades, checkout ficticio y pedidos históricos.
- Portadas de libros y fotos de autores; subida de imágenes validada.
- Datos demo: seis libros, autores, categorías, reseñas y un pedido.

## Ejecutar

Desde esta carpeta:

```bash
dotnet run
```

Abrir la URL que muestra la consola (normalmente `http://localhost:5085`).

Usuarios de demo:

- `admin` / `Admin123!`
- `user` / `User123!`

Tarjeta de checkout ficticia: `4242 4242 4242 4242`, caducidad `12/30`, CVV `123`.

## Arquitectura elegida para el curso

```text
Navegador → Controller → servicio concreto solo si aporta una regla → DbContext → SQLite
                   ↓
              ViewModel → Razor + Bootstrap
```

`ApplicationDbContext` ya es la unidad de trabajo y el repositorio de EF Core. Por
eso **no hay carpeta `Repositories/` ni interfaces `I...Service`**: una interfaz solo
tiene sentido si existen varias implementaciones o si el proyecto necesita desacoplar
un módulo de verdad. Para este MVP docente serían ficheros y saltos de lectura sin
valor.

Los servicios concretos son pequeños y solo se conservan cuando aclaran una operación
que no pertenece a una acción HTTP sencilla:

- `BookService`, `AuthorService`, `CategoryService` y `ReviewService`: consultas y
  reglas de su dominio usando EF Core de forma visible.
- `CartService`: conserva IDs y cantidades en sesión, nunca precios.
- `OrderService`: vuelve a validar el carrito y crea `Order` + `OrderItem`.
- `ImageStorage`: valida y guarda archivos locales.
- `UserService`: agrupa las operaciones de usuarios de Identity que se reutilizarán
  en los proyectos de grupo.

El CRUD habitual usa métodos síncronos y `SaveChanges()`. Solo las operaciones de
Identity conservan `async`/`await`, porque `UserManager`, `RoleManager` y
`SignInManager` solo ofrecen su API de contraseñas, roles y cookies de esa manera. No
se usan `CancellationToken`, concurrencia ni patrones asíncronos en el dominio.

## Carpetas importantes

```text
Controllers/    # Rutas HTTP, ModelState y selección de vista
Data/           # DbContext, migraciones y datos demo
Models/         # Entidades y asociaciones EF Core
Services/       # Ayudas concretas que aportan una regla real
ViewModels/     # DTOs de formularios y de cada pantalla
Views/          # Razor, Bootstrap y layout común
wwwroot/        # CSS, JS, Bootstrap, Font Awesome e imágenes
Utilities/      # Funciones puras como ISBN o precios
docs/           # Guías de código, base de grupos y despliegue
```

Un `ViewModel` es el equivalente más cercano a un DTO de Spring Boot. Por ejemplo,
`BookFormViewModel` contiene `IFormFile` e IDs de categorías que existen solo en el
formulario; `Book` conserva las relaciones reales que se persisten.

## Base común de los grupos

Se pueden conservar `ApplicationUser`, Identity, cuenta, perfil, usuarios,
`ImageStorage` y las vistas asociadas. Cuando una entidad pertenece a una cuenta,
añade esta relación:

```csharp
public string UserId { get; set; } = string.Empty;
public ApplicationUser User { get; set; } = null!;
```

Cada equipo añade después su entidad (`Product`, `Movie`, `Ticket`, `Dish`…) como un
slice vertical: modelo, relación y migración, ViewModel, controlador, vistas y datos
demo. La base de usuarios queda resuelta desde el primer commit.

## Guías

- [Cómo leer el código y las asociaciones](docs/GUÍA-CÓDIGO.md)
- [Qué conservar al crear el repositorio de un grupo](docs/BASE-COMUN-GRUPOS.md)
- [Docker y despliegue de demostración en Render](docs/DESPLIEGUE-RENDER.md)
