# Distributed PubSub with Akka.NET

This is a simple example of how to use Akka.NET's Distributed PubSub to send and receive messages between different actors in different .NET versions.

The .NET 4.8 version is the 'follower' and the .NET 6.0 version is the 'leader'.
This is only because we've used the .NET 6.0 version's port as the seed node address.

# Examples
## Ping / Pong

This is a simple example of how to send and receive messages between actors by using well known actor paths/addresses.

The follower sends periodic ping messages to the leader.
The leader responds with a pong message.    


# Count Report

This is a simple example of how to use Akka.NET's Distributed PubSub to send and receive messages between actors.

The follower starts a few count reporter actors, which periodically publish a count report.
The leader starts a count report processor, which subscribes to the count report topic and receives the count reports.
The processor sends acknowledgements back to the reporter. It will reply with a report rejection message back to the reporter if a duplicate sequence number is detected.

# The Distributed PubSub Problem

## Reproduction

* build the project from scratch `dotnet build .\Akka.Mix.sln --no-incremental`
* open 2 terminal windows
  * in the first terminal window run the .NET 4.8 version `.\Akka.Mix.Net48\bin\Debug\net481\Akka.Mix.Net48.exe`
  * in the second terminal window run the .NET 6.0 version `.\Akka.Mix.Net60\bin\Debug\net6.0\Akka.Mix.Net60.exe`   
* Observe that the cluster is formed and both the ping and count report messages are received by the .NET 6.0 actors

Now for the interesting part.

* stop the .NET 6.0 app using `ctrl-c`
* start the .NET 6.0 app again `.\Akka.Mix.Net60\bin\Debug\net6.0\Akka.Mix.Net60.exe`
* Observe that the cluster is re-formed and the .NET 6.0 app is now able to receive the ping messages again. but the count report messages are not received, not even after waiting for multiple minutes.

if you do it the other way around, i.e. stop the .NET 4.8 app and start it again, the count report messages are received just fine.




