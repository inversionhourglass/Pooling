FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src
COPY . .

ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install -g Cli4Fody
RUN fody-cli DockerSample.sln --addin Rougamo -pv 4.0.4 --addin Pooling -pv 0.1.0 -n Items:Item -a "stateless=Rougamo.IMo+" -a "inspect=method(* RougamoPoolingConsoleApp.BenchmarkTest.M(..))"

RUN dotnet restore

RUN dotnet publish "./RougamoPoolingConsoleApp/RougamoPoolingConsoleApp.csproj" -c Release -o /src/bin/publish

WORKDIR /src/bin/publish
ENTRYPOINT ["dotnet", "RougamoPoolingConsoleApp.dll"]
