# Distributed PubSub with Akka.NET

This is a simple example of how to use Akka.NET's Distributed PubSub to send and receive messages between actors in different processes.

One app is the 'follower' and the other is the 'leader'.
This is only because we've used the leader's port for the seed node address.

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
  * in the first terminal window run the follower `.\Akka.Mix.Follower\bin\Debug\net9.0\Akka.Mix.Follower.exe`
    * wait for it to start, and start complaining about the leader not being available.
  * in the second terminal window run the leader `.\Akka.Mix.Leader\bin\Debug\net9.0\Akka.Mix.Leader.exe`   
* Observe that the cluster is now formed and both the ping and count report messages are being received successfully by the leader

Now for the interesting part.

* stop the leader app using `ctrl-c`
* start the leader app again `.\Akka.Mix.Leader\bin\Debug\net9.0\Akka.Mix.Leader.exe`
* Observe that the cluster is re-formed and the leader app is now able to receive the ping messages again. but the count report messages are not received, not even after waiting for multiple minutes.

By contrast, if you do it the other way around, i.e. stop the follower app and start it again, the count report messages are received just fine after the cluster is re-formed.





