protoc -I=. --csharp_out=internal_access:. stealth-api-v2.1.proto

copy StealthApiV21.cs temp.cs
echo.#pragma warning disable CA1825, IDE0041, IDE0044, IDE0060, IDE0079, IDE0090>StealthApiV21.cs
echo.#nullable disable>>StealthApiV21.cs
type temp.cs >>StealthApiV21.cs
del temp.cs

PAUSE