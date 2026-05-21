# Infrastructure Layer

The **Infrastructure** layer contains **cross-cutting concerns** that support the application: configuration management, logging, persistence, cryptography, and JSON serialization.

This layer is **reusable** across Desktop, Web, and future deployment scenarios.

---

## 📋 Overview

### Purpose

The Infrastructure layer provides:
- **Configuration Management** — Application settings, database connections, feature flags
- **Logging** — File-based logging for debugging and auditing
- **Persistence** — File I/O for drawings and materials
- **Cryptography** — Encryption, hashing for security
- **Serialization** — JSON wrapper for domain entity serialization

### Design Principle

> **Infrastructure handles plumbing. Application handles business logic.**

### Layer Independence

Infrastructure is **UI-agnostic** and can serve:
- ✅ Desktop (WinForms)
- ✅ Web (ASP.NET Core)
- ✅ Services (background jobs)

---

## 🏗️ Project Structure

```
Infrastructure/
├── Config/
│   └── AppConfig.vb                   # Configuration management
├── Logging/
│   ├── ILogger.vb                     # Logging interface
│   └── FileLogger.vb                  # File-based logger implementation
├── IO/
│   ├── TakeOffFileStore.vb            # Drawing file persistence (.takeoff)
│   └── MaterialJsonStore.vb           # Material database JSON storage
├── Crypto/
│   ├── CryptoService.vb               # Encryption/decryption service
│   └── Hashing.vb                     # Password/token hashing
├── Wrappers/
│   └── JsonSerializer.vb              # JSON serialization wrapper
└── README.md
```

---

## 🔧 Core Components

### 1. AppConfig

**Purpose:** Centralized configuration management for the application.

**Key Responsibilities:**

- **Load Settings** — From config files, environment variables, database
- **Provide Access** — Typed, strongly-validated properties
- **Support Environments** — Development, Testing, Production

**Key Properties:**

```vb
Public Class AppConfig
    ''' <summary>Database connection string</summary>
    Public Property DatabaseConnectionString As String

    ''' <summary>Deployment mode: Standalone or Integrated</summary>
    Public Property DeploymentMode As String  ' "Standalone" or "Integrated"

    ''' <summary>Log file path</summary>
    Public Property LogFilePath As String     ' e.g., "./logs/takeoff.log"

    ''' <summary>Enable debug logging</summary>
    Public Property DebugMode As Boolean

    ''' <summary>Feature flags</summary>
    Public Property Features As Dictionary(Of String, Boolean)

    ''' <summary>Unit system (metric, imperial)</summary>
    Public Property DefaultUnitSystem As String

    ''' <summary>Encryption key for sensitive files</summary>
    Public Property EncryptionKey As String
End Class
```

**Key Methods:**

```vb
Public Shared Function LoadFromFile(configPath As String) As AppConfig
    ' Load from JSON/XML config file
End Function

Public Shared Function LoadFromEnvironment() As AppConfig
    ' Load from environment variables
End Function

Public Function GetConnectionString(mode As String) As String
    ' Return appropriate connection string per deployment mode
End Function

Public Function IsFeatureEnabled(featureName As String) As Boolean
    ' Check feature flag
End Function
```

**Related Use Cases:**
- UC-008: Switch between standalone and integrated mode

**Example:**

```vb
' Load configuration
Dim config = AppConfig.LoadFromEnvironment()

' Access settings
Dim connString = config.GetConnectionString(config.DeploymentMode)
Dim debugEnabled = config.DebugMode
Dim unitSystem = config.DefaultUnitSystem
```

---

### 2. ILogger & FileLogger

**Purpose:** Application logging for debugging, auditing, and error tracking.

**Interface:**

```vb
Public Interface ILogger
    Sub Log(level As LogLevel, message As String)
    Sub Log(level As LogLevel, exception As Exception)
    Sub LogDebug(message As String)
    Sub LogInfo(message As String)
    Sub LogWarn(message As String)
    Sub LogError(message As String, Optional ex As Exception = Nothing)
End Interface

Public Enum LogLevel
    Debug
    Info
    Warn
    Error
    Critical
End Enum
```

**Implementation (FileLogger):**

```vb
Public Class FileLogger
    Implements ILogger

    Private ReadOnly _filePath As String
    Private ReadOnly _lock As New Object()

    Public Sub New(filePath As String)
        _filePath = filePath
    End Sub

    Public Sub Log(level As LogLevel, message As String) Implements ILogger.Log
        ' Append to log file with timestamp
        ' Format: [2025-01-15 14:30:45.123] [INFO] Message here
    End Sub
End Class
```

**Key Points:**

- **Thread-safe** — Uses lock for concurrent access
- **Rotating logs** — New file per day (optional)
- **Levels** — Debug, Info, Warn, Error, Critical
- **Timestamped** — All entries include ISO 8601 timestamp

**Related Use Cases:**
- All use cases (implicit logging throughout)

**Example:**

```vb
Dim logger = New FileLogger("./logs/takeoff.log")

logger.LogInfo("Application started")
logger.LogDebug("Canvas initialized with 0 elements")

Try
    ' Some operation
Catch ex As Exception
    logger.LogError("Calculation failed", ex)
End Try
```

---

### 3. TakeOffFileStore

**Purpose:** File persistence for drawing files (.takeoff format).

**Key Responsibilities:**

- **Save Drawing** — Serialize CanvasLayout to file
- **Load Drawing** — Deserialize file to CanvasLayout
- **File Format** — JSON-based with optional compression/encryption

**Key Methods:**

```vb
Public Class TakeOffFileStore
    ''' <summary>Save drawing to file</summary>
    Public Async Function SaveAsync(
        layout As CanvasLayout,
        filePath As String,
        Optional encrypt As Boolean = False
    ) As Task

    ''' <summary>Load drawing from file</summary>
    Public Async Function LoadAsync(
        filePath As String,
        Optional decrypt As Boolean = False
    ) As Task(Of CanvasLayout)

    ''' <summary>Get file metadata without full load</summary>
    Public Function GetMetadata(filePath As String) As FileMetadata
        ' Name, created date, element count
    End Function
End Class

Public Class FileMetadata
    Public Property FilePath As String
    Public Property FileName As String
    Public Property CreatedDate As DateTime
    Public Property ModifiedDate As DateTime
    Public Property ElementCount As Integer
    Public Property FileSizeBytes As Long
End Class
```

**File Format (.takeoff):**

```json
{
  "version": "1.0",
  "canvas": {
    "id": "guid-here",
    "name": "Project A - Ground Floor",
    "unit": "m",
    "scaleFactor": 1.0,
    "createdAt": "2025-01-15T10:30:00Z"
  },
  "elements": [
    {
      "id": "guid-here",
      "type": "Rectangle",
      "layer": "Walls",
      "geometryJson": "{...}",
      "businessJson": "{...}"
    }
  ],
  "layers": [...]
}
```

**Related Use Cases:**
- UC-001-007 (implicit, all drawing operations save state)
- UC-008: Switch between standalone and integrated mode

**Example:**

```vb
Dim store = New TakeOffFileStore()

' Save drawing
Await store.SaveAsync(layout, "project.takeoff", encrypt:=True)

' Load drawing
Dim layout = Await store.LoadAsync("project.takeoff", decrypt:=True)

' Get metadata
Dim metadata = store.GetMetadata("project.takeoff")
Console.WriteLine($"File size: {metadata.FileSizeBytes} bytes")
```

---

### 4. MaterialJsonStore

**Purpose:** Persist material definitions and pricing to JSON files.

**Key Responsibilities:**

- **Save Materials** — Serialize material list to JSON
- **Load Materials** — Deserialize material definitions
- **Merge Prices** — Update pricing from multiple sources

**Key Methods:**

```vb
Public Class MaterialJsonStore
    ''' <summary>Load materials from JSON file</summary>
    Public Function LoadMaterials(filePath As String) As List(Of Material)

    ''' <summary>Save materials to JSON file</summary>
    Public Sub SaveMaterials(materials As List(Of Material), filePath As String)

    ''' <summary>Add or update a material</summary>
    Public Sub UpsertMaterial(material As Material, filePath As String)

    ''' <summary>Get material by ID</summary>
    Public Function GetMaterial(id As String, filePath As String) As Material
End Class

Public Class Material
    Public Property Id As String
    Public Property Name As String
    Public Property Category As String
    Public Property Unit As String         ' m, m², m³, kg, etc.
    Public Property UnitPrice As Double
    Public Property CreatedDate As DateTime
    Public Property LastUpdatedDate As DateTime
End Class
```

**Related Use Cases:**
- UC-003: Attach a Smart Tag to an object
- UC-004: Run a take-off quantity summary

**Example:**

```vb
Dim store = New MaterialJsonStore()

' Load materials
Dim materials = store.LoadMaterials("materials.json")

' Find by ID
Dim concrete = store.GetMaterial("MAT_CONCRETE", "materials.json")

' Update price
concrete.UnitPrice = 25.50
store.UpsertMaterial(concrete, "materials.json")
```

---

### 5. CryptoService

**Purpose:** Encryption and decryption for sensitive file data.

**Key Responsibilities:**

- **Encrypt Data** — AES-256 encryption
- **Decrypt Data** — Reverse encryption
- **Key Management** — Derive keys from master password

**Key Methods:**

```vb
Public Class CryptoService
    ''' <summary>Encrypt text with master key</summary>
    Public Function Encrypt(plaintext As String, masterKey As String) As String
        ' AES-256-GCM encryption
        ' Return Base64-encoded ciphertext
    End Function

    ''' <summary>Decrypt ciphertext</summary>
    Public Function Decrypt(ciphertext As String, masterKey As String) As String
        ' Reverse AES-256-GCM decryption
    End Function

    ''' <summary>Verify ciphertext integrity</summary>
    Public Function VerifyIntegrity(ciphertext As String, masterKey As String) As Boolean
        ' Check HMAC tag
    End Function
End Class
```

**Related Use Cases:**
- UC-008: Switch between standalone and integrated mode (file security)

**Example:**

```vb
Dim crypto = New CryptoService()
Dim masterKey = "my-secret-password"

' Encrypt sensitive data
Dim encrypted = crypto.Encrypt(sensitiveData, masterKey)

' Decrypt when needed
Dim decrypted = crypto.Decrypt(encrypted, masterKey)

' Verify integrity
If crypto.VerifyIntegrity(encrypted, masterKey) Then
    ' Safe to decrypt
End If
```

---

### 6. Hashing

**Purpose:** Cryptographic hashing for passwords and tokens.

**Key Responsibilities:**

- **Hash Password** — PBKDF2 with salt
- **Verify Password** — Compare against hash
- **Generate Token** — Secure random tokens

**Key Methods:**

```vb
Public Class Hashing
    ''' <summary>Hash password with salt</summary>
    Public Shared Function HashPassword(password As String) As String
        ' PBKDF2-SHA256, 10000 iterations, random salt
        ' Return hashed+salted password
    End Function

    ''' <summary>Verify password against hash</summary>
    Public Shared Function VerifyPassword(password As String, hash As String) As Boolean
        ' Compare password against stored hash
    End Function

    ''' <summary>Generate secure random token</summary>
    Public Shared Function GenerateToken(lengthBytes As Integer) As String
        ' Cryptographically secure random bytes
        ' Return Base64-encoded token
    End Function
End Class
```

**Related Use Cases:**
- Application security (future authentication scenarios)

**Example:**

```vb
' Hash password on user creation
Dim passwordHash = Hashing.HashPassword("MySecurePassword123")
' Store passwordHash in database

' Verify password on login
If Hashing.VerifyPassword("MySecurePassword123", passwordHash) Then
    ' Login success
End If

' Generate secure token
Dim token = Hashing.GenerateToken(32)  ' 32 random bytes
```

---

### 7. JsonSerializer Wrapper

**Purpose:** Centralized JSON serialization using System.Text.Json.

**Key Responsibilities:**

- **Serialize Objects** — Object → JSON string
- **Deserialize Objects** — JSON string → Object
- **Settings Management** — Consistent JSON settings across app

**Key Methods:**

```vb
Public Class JsonSerializer
    Private Shared ReadOnly _options As JsonSerializerOptions = New JsonSerializerOptions With {
        .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        .WriteIndented = True
    }

    Public Shared Function Serialize(Of T)(obj As T) As String
        ' Serialize object to indented JSON
    End Function

    Public Shared Function Deserialize(Of T)(json As String) As T
        ' Deserialize JSON to object
    End Function

    Public Shared Function SerializeIndented(Of T)(obj As T) As String
        ' Pretty-printed JSON
    End Function
End Class
```

**Related Use Cases:**
- All persistence operations (implicit)

**Example:**

```vb
' Serialize
Dim json = JsonSerializer.Serialize(layout)

' Deserialize
Dim loadedLayout = JsonSerializer.Deserialize(Of CanvasLayout)(json)

' Pretty print
Dim prettyJson = JsonSerializer.SerializeIndented(layout)
```

---

## 🔄 Data Flow

### File Save Pipeline

```
CanvasLayout (in memory)
    ↓
JsonSerializer.Serialize() → JSON string
    ↓
(Optional) CryptoService.Encrypt() → Encrypted bytes
    ↓
TakeOffFileStore.SaveAsync() → Write to disk
    ↓
Log event (FileLogger)
    ↓
File saved: project.takeoff
```

### File Load Pipeline

```
File: project.takeoff (on disk)
    ↓
TakeOffFileStore.LoadAsync() → Read from disk
    ↓
(Optional) CryptoService.Decrypt() → JSON string
    ↓
JsonSerializer.Deserialize(Of CanvasLayout)() → Object
    ↓
Log event (FileLogger)
    ↓
CanvasLayout (in memory)
```

---

## 🏗️ Dependency Injection

### Typical DI Setup

```vb
' In CompositionRoot or DI container
Dim config = AppConfig.LoadFromEnvironment()
Dim logger = New FileLogger(config.LogFilePath)

Dim fileStore = New TakeOffFileStore()
Dim materialStore = New MaterialJsonStore()
Dim cryptoService = New CryptoService()

' Inject into services
Dim takeOffService = New TakeOffService(
    materialService:=New MaterialService(materialStore, logger),
    logger:=logger
)
```

---

## 🧪 Testing Considerations

### Unit Tests

- **AppConfig** — Test configuration loading from different sources
- **FileLogger** — Test log file creation and formatting
- **JsonSerializer** — Test round-trip serialization for domain entities
- **Hashing** — Test password verification, token generation

### Integration Tests

- **TakeOffFileStore** — Test save/load with actual files
- **CryptoService** — Test encrypt/decrypt round-trip
- **Material JSON Store** — Test persistence with material data

### Mock Dependencies

```vb
' Create in-memory logger for tests
Public Class MockLogger
    Implements ILogger
    Public Property Messages As List(Of String)

    Public Sub Log(level As LogLevel, message As String) Implements ILogger.Log
        Messages.Add($"[{level}] {message}")
    End Sub
End Class
```

---

## 🔒 Security Considerations

### Password Storage

✅ **Do:**
- Use PBKDF2 or Bcrypt for password hashing
- Store only hashes, never plaintext passwords
- Use strong salt (16+ bytes)

❌ **Don't:**
- Use MD5 or SHA-1 (weak)
- Store passwords in config files
- Log passwords or tokens

### File Encryption

✅ **Do:**
- Use AES-256 for file encryption
- Use authenticated encryption (GCM mode)
- Protect encryption keys

❌ **Don't:**
- Use weak ciphers (DES, RC4)
- Embed keys in source code
- Log encrypted/decrypted data

### Configuration Secrets

✅ **Do:**
- Use environment variables for secrets
- Use OS credential storage (Windows DPAPI)
- Rotate secrets regularly

❌ **Don't:**
- Commit secrets to Git
- Hardcode connection strings
- Log sensitive config values

---

## 📝 Conventions

### Naming

- Service classes use **Service suffix** (CryptoService, FileLogger)
- Interfaces use **IService prefix** (ILogger, IFileStore)
- Configuration classes use **Config suffix** (AppConfig)
- Methods use **Async suffix** for async operations (SaveAsync, LoadAsync)

### Logging

- **Info**: Major operations (start, save, load)
- **Debug**: Detailed execution steps
- **Warn**: Recoverable issues (file not found, retry)
- **Error**: Unexpected failures (exception + stack trace)

```vb
_logger.LogInfo("Loading materials from file")
_logger.LogDebug($"Material count: {count}")
_logger.LogWarn($"Material not found: {id}, using default")
_logger.LogError("Failed to decrypt file", ex)
```

---

## ⚠️ Important Notes

### No Business Logic

❌ Do NOT add:
- Domain entity logic
- Calculation logic
- Use case orchestration

✅ Keep Infrastructure layer:
- Pure plumbing
- Framework abstractions
- Cross-cutting concerns

### Testability

- All major components should have **interfaces** for mocking
- Avoid **static** methods (use dependencies instead)
- Keep **file paths configurable** (don't hardcode)

---

## 🚀 Quick Reference

### Load Configuration

```vb
Dim config = AppConfig.LoadFromEnvironment()
Dim connString = config.GetConnectionString(config.DeploymentMode)
```

### Setup Logging

```vb
Dim logger = New FileLogger(AppConfig.LoadFromEnvironment().LogFilePath)
logger.LogInfo("Application started")
```

### Save/Load Drawing

```vb
Dim fileStore = New TakeOffFileStore()
Await fileStore.SaveAsync(layout, "project.takeoff", encrypt:=True)
Dim loaded = Await fileStore.LoadAsync("project.takeoff", decrypt:=True)
```

### Encrypt Sensitive Data

```vb
Dim crypto = New CryptoService()
Dim encrypted = crypto.Encrypt(plaintext, masterKey)
Dim decrypted = crypto.Decrypt(encrypted, masterKey)
```

---

**Last Updated:** 19 April 2026  
**Layer Responsibility:** Cross-Cutting Concerns & Persistence  
**Maintainer:** Development Team
