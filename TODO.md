## Controllers
Automatically created the controllers for all the models except:
1. Order
2. OrderProduct

## Intermediate tables
I dont't know how is going to behave the controllers for intermediate tables, since the Guid is a mix of two different Guid
Is it going to fit in the data type of the sql server?

## Constructor
Creating constructors with no parameters and fixed property values can be usefull to fill data the user has not been required to fill yet.
Also, to avoid problems of interconexion between tables, where values are needed.

## En lo relativo a propiedades de navegacion
He a�adido ? a las propiedades de navegaci�n de User, para evitar
que al querer ingresar los datos de la tabla User en la base de datos
de error y no lo permita.
Aunque ahora puedo a�adir estos nuevos usuarios a la base de datos, sigue saliendo
un error en el navegador/console.
Para intentar evitarlo, he a�adido al dbcontext de users:
- .IsRequired(false)

Queda hacer la migraci�n para actualizar los constraints de la base de datos/EF.

### Tuve que actualizar: dotnet tool update --global dotnet-ef


## Error incluso agregando adecuadamente usuarios:
if (xhr.status === 200) { 
Esa linea en el cliente me mostraba error por consola, pero porque el codigo que envia el metodo del controlador con el codigo de abajo era 201, no 200.
 return CreatedAtAction("GetUser", new { id = user.UserId }, user);


 ## Ultimos apuntes 22/12
 Problemas en la comunicaci�n entre cliente y servidor para pasar los datos email y password. Uso de clase personalizada? envio por url encoded? probar otros sistemas.
 Posible problema posterior con la generaci�n del token, sin confirmar porque lno estoy seguro de como est� implementado el env�o y recibo de informaci�n y sus formatos.

_configuration es adecuado y contiene la informaci�n de la key de jwt ? revisar JwtGenerator.