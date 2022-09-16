# KMD service api

Public service with goal of securing Algorand network by generic public users.

Service is using ARC-0014 authentication. In the Authentication header the signed algorand transaction with zero fee with note field KMD is required. Signator of the transaction is authenticated user.
