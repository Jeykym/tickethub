# TicketHub

Malé API, které vyvíjím pro procvičení .NET frameworku. API představuje jednoduchý backend pro
aplikaci poskytovatele vstupenek na koncerty.

## MVP
API je vyvíjeno metodikou MVP.

### Podporuje
- základní vytváření objektů koncertů a objednávek
- koncerty mají maximální kapacitu vstupenek, kterou nelze překročit
- objednávky umožňují specifikovat množství objednáváných vstupenek
- objednávky umožňují identifikovat zákazníka podle jeho emailu

### Nepodporuje
- Autentizaci a autorizaci uživatelů pro skutečně bezpečné objednání

## Stack
- .NET
- SQLite

## Spuštění lokálně

```bash
dotnet ef database update
dotnet run
```

## Spuštění testů

```bash
dotnet test
```
 