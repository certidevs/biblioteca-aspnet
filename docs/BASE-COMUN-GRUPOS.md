# Base común para `g1_aspnet`, `g2_aspnet`, …

La aplicación `biblioteca-aspnet` sirve como referencia completa y contiene también
la base transversal que no debería repetirse en cada grupo.

## Qué se conserva en todos los repositorios

- `Models/ApplicationUser.cs`: usuario de Identity con nombre visible, estado, fecha de alta y avatar.
- Configuración de Identity en `Program.cs` y `Data/ApplicationDbContext.cs`.
- `Controllers/AccountController.cs`: registro, login por usuario/email y logout.
- `Controllers/ProfileController.cs`: consulta y edición de perfil, avatar y cambio de contraseña.
- `Controllers/UsersController.cs`: panel de administración de usuarios.
- `Services/UserService.cs` e `IUserService`: casos de uso de cuenta y administración.
- `Services/ImageStorage.cs`, `IImageStorage.cs` y `ImageFolder.cs`: almacenamiento común de imágenes.
- `wwwroot/lib/fontawesome` y el selector `data-bs-theme` del layout: iconos y tema reutilizables.
- ViewModels y vistas de `Account`, `Profile` y `Users`.
- Roles `User` y `Admin`, datos de demo y protección antiforgery.

## Qué añade cada grupo

Cada temática tiene sus propias entidades y asociaciones. Cuando una entidad necesita
saber quién realiza una acción, se relaciona con `ApplicationUser` mediante una FK:

```csharp
public sealed class Order
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
```

Ejemplos:

| Proyecto | Entidad propia | Relación con usuario |
| --- | --- | --- |
| Ecommerce | `Order`, `ProductReview`, `Address` | un usuario hace pedidos, reseñas y guarda direcciones |
| Cartelera | `Ticket`, `MovieReview` | un usuario compra entradas y escribe reseñas |
| Restaurantes | `Order`, `Review`, `Favorite` | un usuario pide, reseña y marca favoritos |

La regla pedagógica es que cada alumno complete un slice vertical de su entidad:

```text
Entidad + validación
        ↓
Relación EF Core y migración
        ↓
Repositorio con consultas
        ↓
Servicio con reglas de negocio
        ↓
Controlador MVC
        ↓
ViewModel + vistas Razor + UX Bootstrap
```

## Cómo preparar un repositorio nuevo

1. Copiar la base de este repositorio en el nuevo repositorio.
2. Conservar primero Identity, cuenta, perfil, usuarios e imágenes.
3. Sustituir el dominio de biblioteca (`Book`, `Author`, `Category`, `Review` y los pedidos) por las entidades de la temática.
4. Mantener `ApplicationUser` y añadir las FKs a usuario en las entidades que lo necesiten.
5. Revisar `ApplicationDbContext`, eliminar los `DbSet` que ya no correspondan y crear una migración inicial limpia para el nuevo repositorio.
6. Cambiar el nombre de la base SQLite, la marca visual y los datos de demo.
7. Crear el primer slice completo antes de añadir la siguiente entidad.

No se copian `App_Data/*.db` ni imágenes subidas: están excluidos por `.gitignore`.
Cada repositorio genera su base local aplicando sus propias migraciones.

## Contrato común que los alumnos pueden asumir

Al empezar su dominio, los alumnos ya pueden dar por disponibles:

- `UserId` y `User` para relaciones `N:1`.
- `User.IsActive` para no permitir acciones a cuentas desactivadas.
- `User.IsInRole("Admin")` o `[Authorize(Roles = RoleNames.Admin)]` para administración.
- `IImageStorage` para subir imágenes de productos, películas, restaurantes, platos, etc.
- Font Awesome para mantener los iconos del proyecto sin emojis y con etiquetas accesibles.
- `Profile` como lugar donde el usuario gestiona sus datos y avatar.

La lógica específica de compra, pedido, entrada o reseña sigue perteneciendo a cada
proyecto y no se oculta dentro de la base común.
