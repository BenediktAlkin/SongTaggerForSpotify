### Setup
- add Nuget-Package Microsoft.EntityFrameworkCore.Design to project where migration is required
- make sure that DbContext class has parameterless constructor
- install ef tools ``` dotnet tool install --global dotnet-ef ```
- update ef tools  ``` dotnet tool update --global dotnet-ef ```
- verify installation  ``` dotnet ef ```


### add migration
``` 
dotnet ef migrations add <MigrationName> --project Backend 
```
