#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG ALGO_BASE
FROM scholtz2/algorand-participation-mainnet-extended:$ALGO_BASE

# /app/run-participation-voitest.sh
RUN echo '#!/bin/bash\nalgodtoken='"\$(cat /app/data/algod.token)"'\nsed -i s~aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa~$algodtoken~g /kmd/appsettings.json\nsed -i '"'"'s~"server": "http://localhost:8080"~"server": "http://localhost:28081"~g'"'"' /kmd/appsettings.json\nsed -i '"'"'s~"networkGenesisId": "mainnet-v1.0"~"networkGenesisId": "voitest-v1"~g'"'"' /kmd/appsettings.json\nsed -i '"'"'s~"networkGenesisHash": "wGHE2Pwdvd7S12BL5FaOP20EGYesN73ktiC1qzkkit8="~"networkGenesisHash": "IXnoWtviVVJW5LGivNFc0Dq14V3kqaXuK2u5OQrdVZo="~g'"'"' /kmd/appsettings.json\ncd /kmd/ && nohup dotnet AlgorandKMDServer.dll &\n'"$(cat /app/run-participation-voitest.sh)" > /app/run-participation-voitest.sh

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:18888
EXPOSE 18888
ADD docker/voitest/version.json version.json
ENTRYPOINT ["/bin/bash", "/app/run-participation-voitest.sh"]
