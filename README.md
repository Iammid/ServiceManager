
# 🔧 DIAttributes + ServiceManager (Roslyn Source Generator)

**Automatically generate dependency injection registrations** using simple attributes like `[Scoped]`, `[Singleton]`, and `[Transient]`.

No more writing `builder.Services.AddScoped<...>()` by hand — let the compiler handle it for you. ⚡

---

## 💡 Why Use This?

- 🔥 Stop maintaining giant lists of `AddScoped<...>()`
- ⛓️ Attribute-based service registration (`[Scoped]`, `[Singleton]`, `[Transient]`)
- 🧠 Automatically detects interfaces and registers like `services.AddScoped<IMyService, MyService>()`
- ⚡ Powered by Roslyn source generators — compile-time, zero reflection
- 📦 No runtime cost or startup scanning
- ✅ Works with any .NET 8.0+ app

---

## 🧪 Requirements

- .NET 8+ for your app
- The generator project (`ServiceManager`) must target `netstandard2.0`
- Add `builder.Services.AddAutoServices();` in your app manually (required)
- Have both the (`ServiceManager`) and (`DIAttributes`) installed and in the referenced folders

---

## 📦 What Is This?

This solution includes two projects:

### `DIAttributes`
A small class library that defines these attributes:

```csharp
[Scoped]
[Singleton]
[Transient]
```

Use them to annotate your services in your application.

---

### `ServiceManager`
A **Roslyn Source Generator** that scans for those attributes and automatically generates the appropriate DI registration code at compile time.

---

## 🧰 Setup (Installation)

### 1. Reference Both Projects

In your main application `.csproj`, add:

```xml
<ItemGroup>
  <!-- This project defines the [Scoped], [Singleton], [Transient] attributes -->
  <ProjectReference Include="..\DIAttributes\DIAttributes.csproj" />

  <!-- This is the source generator that scans for those attributes and generates DI code -->
  <ProjectReference Include="..\ServiceManager\ServiceManager.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

Adjust the paths if your folders are different.

---

### 2. Annotate Your Services

In your service classes, use the attributes like this:

```csharp
using DIAttributes;

[Scoped]
public class ValidationService : IValidationService
{
    private readonly INotificationFacade _dialogFacade;
    // ...
}
```

✔️ The generator will automatically detect this class and generate:

```csharp
services.AddScoped<IValidationService, ValidationService>();
```

---

### 3. Call the Generated Registration Method

In your `Program.cs` (or wherever you're configuring services):

```csharp
builder.Services.AddAutoServices();
```

That method is generated automatically during build.

You can find the generated code under:

```plaintext
obj/GeneratedCode/ServiceManager.DIGenerator/DIRegistration.g.cs
```

---

## ✨ Example Output

If you have:

```csharp
[Singleton]
public class LoggingService { }

[Scoped]
public class ValidationService : IValidationService { }
```

The generator will produce something like:

```csharp
public static class ServiceRegistration
{
    public static void AddAutoServices(this IServiceCollection services)
    {
        services.AddSingleton<LoggingService>();
        services.AddScoped<IValidationService, ValidationService>();
    }
}
```


