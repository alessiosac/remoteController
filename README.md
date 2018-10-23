# Remote Controller

This project allow the admininstrator to control a remote desktop. 
It consists of 2 programs:
1. Server, installed on the target machine. It collects the information about running processes, and send them to the Client.
2. Client, desktop application with UI, the system manager can see the list of programs on the target machine in a graphical way, 
and can send commands to them.

The Server is a C++ program, while the Client is deployed in C#.
