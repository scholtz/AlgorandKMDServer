#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM scholtz2/algorand-kmd-mainnet:3.9.4-stable AS algo

FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build
WORKDIR /src
COPY ["AlgorandKMDServer.csproj", "."]
RUN dotnet restore "./AlgorandKMDServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AlgorandKMDServer.csproj" -c Release -o /app/build
RUN dotnet publish "AlgorandKMDServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy AS final
ENV ALGORAND_DATA=/app/data
USER root
ENV DEBIAN_FRONTEND noninteractive
RUN apt update && apt dist-upgrade -y && apt install -y mc wget telnet git curl iotop atop vim && apt-get clean autoclean && apt-get autoremove dotnet6 --yes && rm -rf /var/lib/{apt,dpkg,cache,log}/
WORKDIR /app
RUN mkdir /kmd
RUN mkdir /node
RUN mkdir /app/data
RUN mkdir /app/mainnet
RUN useradd -ms /bin/bash algo
RUN chown algo:algo /app -R
RUN chown algo:algo /node -R
RUN chown algo:algo /kmd -R
WORKDIR /app
COPY --from=algo /app .
COPY --from=algo /node /node
WORKDIR /kmd
COPY --from=build /app/publish /kmd
ENV PATH=/node:$PATH

RUN echo "sed -i s~aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa~\$algodtoken~g /kmd/appsettings.json\n$(cat /app/run-kmd.sh)" > /app/run-kmd.sh
RUN echo "algodtoken=\$(cat /app/data/algod.token)\n$(cat /app/run-kmd.sh)" > /app/run-kmd.sh
RUN echo "cd /kmd/ && nohup dotnet AlgorandKMDServer.dll &\n$(cat /app/run-kmd.sh)" > /app/run-kmd.sh

RUN echo "cd /kmd/ && nohup dotnet AlgorandKMDServer.dll &\n$(cat /app/run.sh)" > /app/run.sh

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:18888
EXPOSE 18888
ENTRYPOINT ["dotnet", "AlgorandKMDServer.dll"]