dotnet publish ./src/Nuages.PubSub.Starter.WebSocket/Nuages.PubSub.Starter.WebSocket.csproj --configuration Release --framework net6.0 --no-self-contained  /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64
dotnet publish ./src/Nuages.PubSub.Starter.API/Nuages.PubSub.Starter.API.csproj --configuration Release --framework net6.0 --no-self-contained  /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64
