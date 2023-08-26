# Algorand participation server api

Public service with goal of securing Algorand network by generic public users.

Service is using ARC-0014 authentication. In the Authentication header the signed algorand transaction with zero fee with note field KMD is required. Signator of the transaction is authenticated user.

Please use the participation api endpoints. KMD endpoints are deprecated and will be removed soon.

## Sample configuration

```
  "algod": {
    "server": "http://localhost:8080",
    "token": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
    "networkGenesisId": "mainnet-v1.0",
    "networkGenesisHash": "wGHE2Pwdvd7S12BL5FaOP20EGYesN73ktiC1qzkkit8=",
    "header": "X-Algo-API-Token",
    "realm": "KMD",
    "checkExpiration": "True",
    "debug": "True"
  },
  "ParticipationServer": {
    "LockTime": 120,
    "MaximumRounds": 2000000
  }
```

## Sample deplyment

https://github.com/scholtz/AlgorandNodes/tree/main/kubernetes/algod-participation/mainnet-participation

## Public algod participation server registration

Please register your public participation server to:

https://github.com/scholtz/AlgorandPublicData/tree/main/participation