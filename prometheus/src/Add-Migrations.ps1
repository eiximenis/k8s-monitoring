$Env:pgsql__constr="Host=localhost;Database=hellodb;Username=hellouser;Password=Passw0rd!"
docker compose up -d pgsql
Push-Location HelloMonitor
dotnet ef migrations add Initial
Pop-Location