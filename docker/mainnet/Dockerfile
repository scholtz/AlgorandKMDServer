#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG ALGO_BASE
FROM scholtz2/algorand-kmd-mainnet:$ALGO_BASE AS algo

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src
COPY ["AlgorandKMDServer.csproj", "."]
RUN dotnet restore "./AlgorandKMDServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AlgorandKMDServer.csproj" -c Release -o /app/build
RUN dotnet publish "AlgorandKMDServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS final
ENV ALGORAND_DATA=/app/data
USER root
ENV DEBIAN_FRONTEND noninteractive
RUN apt update && apt dist-upgrade -y && apt install -y bc mc wget telnet git curl net-tools iotop atop vim dnsutils jq iproute2 && apt-get autoremove --yes && rm -rf /var/lib/{apt,dpkg,cache,log}/
RUN useradd -ms /bin/bash algo -u 1000 -d /app
WORKDIR /app
RUN mkdir /kmd && mkdir /node && mkdir /app/data && mkdir /app/mainnet
RUN chown algo:algo /app -R &&  chown algo:algo /node -R && chown algo:algo /kmd -R

WORKDIR /app
COPY --from=algo /app .
COPY --from=algo /node /node
WORKDIR /kmd
COPY --from=build /app/publish /kmd
ENV PATH=/node:$PATH

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:18888
EXPOSE 18888
ADD docker/mainnet/version.json version.json
ENTRYPOINT ["/bin/bash", "/app/run-participation-mainnet-with-appsettings.sh"]
