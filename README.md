# MoveITMVC

> [!IMPORTANT]
> This repository is soon going to be the main backend API for the MoveIT UI:
> https://github.com/MGN3/project-frontend-developer-js#readme

This is one of the most advanced projects I have done so far. Following the Model-View-Controller design pattern, I created the data models using Entity Framework Core. 

The available endpoints that are accessed by the UI client apply the logic of the object-methods developed in the classes that represent the Database tables.

## SQL Server Database
The database consist of a series of typical e-commerce relational tables with users, addresses, orders, shoppingcart, products...

Designing an Entity-relational database is allways challenging, Entity Framework made it harder because it was a new syntax/system to me. However, the benefits of using an ORM like Entity Framework are:

- Speeds up the development.
- Security: an extra abstraction layer.

## Stack
- ASP.NET Core 8.0
- C#
- MVC
- Entity Framework Core
- SQL Server

## Conclusions
Many pieces of the web and backend development puzzle are starting to make more sense. Following one of the most famous design patters made me understand the importance of a good structure, making it easier to maintain, refactor, debug...
