# UdpPeerToPeerWithStun

This repository contains the source code for two projects:

- A simple STUN server that sends each peer's temporarily public IP address and port number to another peer based on the `groupName`.
- A simple peer messenger that communicates with the STUN server, punches an UDP hole inside the other peer's NAT, and starts messaging.
