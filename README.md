# Biblioteca ASP.NET

Aplicación web de referencia del curso, equivalente a
[proyecto_biblioteca](https://github.com/alansastre/proyecto_biblioteca), implementada con
C# y ASP.NET Core MVC.

## Stack

- .NET 10 LTS / C# 14.
- ASP.NET Core MVC con controladores y vistas Razor (`.cshtml`).
- Entity Framework Core 10 con SQLite.
- ASP.NET Core Identity con autenticación por cookies y roles.
- Bootstrap 5.3.8 servido desde `wwwroot/lib`, con un tema visual propio responsive.
- Subida local de avatares y portadas con validación de tamaño, extensión y firma del archivo.
- Migraciones EF Core y datos de demo idempotentes.
- Sin tests automáticos, según el alcance docente solicitado.

SQLite ocupa aquí el lugar práctico de H2: es una base de datos relacional embebida,
multiplataforma y no requiere instalar un servidor. En desarrollo se guarda en
`App_Data/biblioteca.db`; el esquema se recrea en cualquier equipo aplicando las
migraciones versionadas.

## Funcionalidades

- Dashboard inicial con estadísticas.
- Listado de libros con búsqueda por texto, autor, categoría, disponibilidad y favoritos.
- Ficha de libro con autor, categorías, sinopsis y reseñas.
- CRUD de libros, autores y categorías, protegido para administradores.
- Registro, login por usuario o email, logout, bloqueo temporal tras intentos fallidos y roles `User` y `Admin`.
- Perfil editable: nombre visible, email, avatar y cambio de contraseña.
- Administración completa de usuarios: listar, buscar, consultar actividad, crear, editar, activar/desactivar, cambiar rol, restablecer contraseña y eliminar.
- Reglas de seguridad para no eliminar el propio admin ni el último administrador.
- Favoritos y carrito de compra para usuarios autenticados: añadir, quitar, cambiar cantidades y vaciar.
- Checkout ficticio con tarjeta de prueba, validación de servidor y creación de pedidos con varias líneas.
- Historial de pedidos del usuario y consulta global para administradores.
- Reseñas de 1 a 5 estrellas; cada usuario puede modificar o borrar sus reseñas y el admin puede moderarlas.
- Perfil con favoritos, pedidos, reseñas y total gastado.
- Portadas de libros gestionadas desde el formulario de alta/edición.
- Protección antiforgery automática para formularios POST.

Las imágenes se guardan fuera de Git en `wwwroot/uploads/avatars` y
`wwwroot/uploads/book-covers`. La base de datos solo almacena un nombre aleatorio
generado por la aplicación, no la ruta ni el nombre original del archivo.

La explicación guiada de las asociaciones, el flujo de una petición y el checklist
para añadir una entidad está en [`docs/GUÍA-CÓDIGO.md`](docs/GUÍA-CÓDIGO.md).

## Base común para los proyectos de grupos

La parte de usuarios está pensada como infraestructura transversal. Al crear un
proyecto nuevo se conserva el bloque de Identity, cuenta, perfil, gestión de usuarios
e `IImageStorage`; cada equipo añade sus entidades y relaciones apuntando a
`ApplicationUser`:

```csharp
public string UserId { get; set; } = string.Empty;
public ApplicationUser User { get; set; } = null!;
```

Así, una compra, una entrada de cine, un pedido o una reseña puede pertenecer a un
usuario sin que el grupo tenga que volver a implementar registro, login, roles,
avatares o el panel de administración. La guía de extracción está en
[`docs/BASE-COMUN-GRUPOS.md`](docs/BASE-COMUN-GRUPOS.md).

## Ejecutar

Desde la raíz de este repositorio:

```bash
dotnet run
```

Al arrancar, `Program.cs` llama a `DbInitializer`, que aplica migraciones y siembra los
datos de ejemplo solo cuando el catálogo está vacío.

Usuarios incluidos:

- `admin` / `Admin123!`
- `user` / `User123!`

Tarjeta de prueba para el checkout:

- Número: `4242 4242 4242 4242`
- Caducidad: `12/30`
- CVV: `123`

## Estructura

```text
Controllers/    # Equivalente a @Controller y manejo de rutas HTTP
Data/            # ApplicationDbContext, relaciones ORM y DbInitializer
Models/          # Author, Book, Category, Review, Order, OrderItem y ApplicationUser
Repositories/    # IRepository + consultas específicas con EF Core/LINQ
Services/        # Casos de uso y reglas de negocio
ViewModels/      # DTOs de formularios y páginas
Views/           # Razor Views y layout común Bootstrap
Utilities/       # Lógica pura reutilizable: ISBN, estadísticas y precios
Data/Migrations/ # Historial versionado del esquema
docs/           # Guías docentes del modelo y del flujo de una petición
```

Piezas concretas de la base común:

```text
Models/ApplicationUser.cs
Controllers/AccountController.cs
Controllers/ProfileController.cs
Controllers/UsersController.cs
Services/UserService.cs
Services/ImageStorage.cs
ViewModels/Profile/
ViewModels/Users/
Views/Account/
Views/Profile/
Views/Users/
```

## Equivalencias que se pueden señalar en clase

```text
Controller → Service → Repository → ApplicationDbContext → SQLite
     ↓             ↓            ↓
  Razor       reglas       consultas LINQ
```

- `ApplicationDbContext` es el contexto de EF Core y configura relaciones con `OnModelCreating`.
- `IRepository<TEntity>` muestra el CRUD común que Spring Data ofrece mediante `JpaRepository`.
- Las interfaces específicas contienen consultas equivalentes a derived queries y `@Query`.
- `CartService` guarda temporalmente las cantidades en la sesión y `OrderService` las valida de nuevo y las persiste como `Order` + `OrderItem`.
- Identity sustituye a Spring Security: `[Authorize]` protege acciones y
  `[Authorize(Roles = RoleNames.Admin)]` restringe operaciones de administración.
- `BookFormViewModel` y los demás ViewModels evitan enlazar directamente entidades complejas desde formularios.
