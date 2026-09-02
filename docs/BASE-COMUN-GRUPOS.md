# Base común para `g1_aspnet`, `g2_aspnet`, …

`biblioteca-aspnet` es la referencia completa. Cada repositorio de grupo puede empezar
con la infraestructura de usuarios terminada para que el trabajo se centre en su
propio dominio.

## Conservar

- `ApplicationUser`, `RoleNames` y la configuración de Identity.
- `AccountController`, `ProfileController`, `UsersController` y sus vistas/ViewModels.
- `UserService`, `ImageStorage`, `ImageFolder` y la configuración de subidas.
- La migración inicial de Identity, los roles `User` y `Admin`, la protección
  antiforgery, Bootstrap, Font Awesome y el selector de tema.

No se conserva una capa `Repositories` ni interfaces de servicio: el nuevo proyecto
puede usar `ApplicationDbContext` y `SaveChanges()` de manera explícita, igual que la
referencia.

## Relacionar una entidad con el usuario

Cuando una acción pertenece a una cuenta, añadir una clave externa y la navegación:

```csharp
public sealed class Ticket
{
    public int Id { get; set; }

    // La FK guarda el propietario de la entrada.
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
```

En el controlador, el ID debe salir de la cookie, no del formulario:

```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
ticket.UserId = userId!;
```

Ejemplos de dominio:

| Proyecto | Relaciones probables |
| --- | --- |
| Ecommerce | Usuario → Pedido, Producto → Reseña, Producto ↔ Categoría |
| Cartelera | Usuario → Entrada, Película → Reseña, Sesión → Película |
| Restaurantes | Usuario → Pedido, Restaurante → Plato, Usuario → Reseña |

## Primer slice vertical

1. Crear la entidad con sus validaciones.
2. Añadir `DbSet` y configurar la relación en `ApplicationDbContext`.
3. Crear una migración y comprobar que la base se crea.
4. Crear un ViewModel de formulario.
5. Crear el controlador MVC y las vistas `Index`, `Details`, `Create`, `Edit` y
   `Delete`.
6. Añadir unos datos demo y hacer un commit.

Una clase concreta de ayuda está justificada si encierra una operación completa, por
ejemplo `CartService` o `ImageStorage`. No crear una interfaz solo para tener una
interfaz: si solo habrá una implementación, inyectar la clase directamente deja el
flujo más fácil de seguir.

## Antes de empezar a codificar

- Renombrar la solución, namespace, título, base SQLite y marca visual.
- Eliminar las entidades y migraciones de biblioteca que no correspondan.
- Crear una migración inicial limpia para el nuevo dominio.
- Mantener los usuarios demo para que cada equipo pueda probar permisos desde el
  primer día.
- No copiar `App_Data/*.db` ni imágenes subidas: cada repositorio genera sus propios
  datos a partir de migraciones y del inicializador.
