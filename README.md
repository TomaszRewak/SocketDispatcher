# SocketDispatcher

Socket Dispatcher marries network sockets with the `System.Windows.Threading.Dispatcher`.

It enables you to consume network traffic synchronously on a thread that hosts a standard windows dispatcher. It’s worth noting that it doesn’t necessarily has to be the main GUI thread, as the `System.Windows.Threading.Dispatcher` can be spawned manually on any background thread if needed. But if you decide to use this library on the main thread of your desktop application, please make sure you know perfectly well what the maximal network load it has to handle might be - as it will have a direct impact on the responsiveness of your user interface.

### Why?

Why would you want to go with this solution instead of just handling the networking completely asynchronously? Well, sometimes it's just simpler to think about problems in a single-threaded domain. Funny enough, it can also bring some big performance improvements. Depending on the type of the data you are receiving, you might want to deserialize a newly received stream directly into preexisting objects by overriding their current state. In a multi-threaded application you might need to ensure thread safety by either locking, creating new immutable objects to hold the new state or by pooling pre-allocated structures. The first approach raises the thread contention rate, the second one puts a pressure on the GC and the third one significantly increases the code complexity. Of course putting network handling and processing on a dispatcher is not a Holy Grail, but it might bring quite few benefits in some very specialized applications.

### How to use it?

All you have to do is to derive a your own class from the base `SocketConnection` class. 

What’s important, you have to create objects of this derived class directly on the thread your network dispatcher is running on.

In the derived class you will have an option to override following methods:

```csharp
protected internal virtual void OnConnected();
protected internal virtual void OnDisconnected();
protected internal virtual void OnConnectionFailed();
protected virtual void OnAccepted(Socket socket);
protected virtual int Read(ReadOnlySpan<byte> data);
```

The first three simply inform you about socket state changes.

The `OnAccepted` method allows you to accept new connections in case your application is acting as a server.

The most interesting one is of course the `Read` method. It's invoked whenever new data has been received. As you might have noticed, it has to return an integer. Integer that indicated how many bytes have been actually consumed by the client. It might be that an incomplete message has been received (and there is no way to know about it on the pure TCP level) and in that case the method should return only the number of bytes it was actually able to process. The remaining ones will be part of the buffer next time this method is called.

Apart from that, other methods that are available through the `SocketConnection` class are:

```csharp
protected void Connect(string host, int port);
protected void Disconnect();
protected void Listen(int port, int maxPendingConnections = 100);
protected void Close();
protected Span<byte> Write(int bytes);
protected void Flush();
```

`Connect` and `Disconnect` should be quite straightforward. They allow you to connect to the remote server and end the connection afterward.

`Listen` and `Close` are equivalent of the `Connect` and `Disconnect` methods, but should be used in case you are creating a server that accepts incoming connections. When using them, don't forget to override the `OnAccepted` method (which by default simply rejects all clients).

The `Write` method is probably the one you will be using most frequently. It allows you to write to the send buffer. It has a single parameter that specifies how many bytes the client wants to send. When called, it allocates (or most likely reuses) buffer space of requested size and returns it as a mutable `Span`. Thanks to that, if your serialization algorithm is well optimized, unnecessary allocations and copying can be easily avoided. Once writing is complete simply call the `Flush` method to notify the socket about pending data being available (tip: you can also stack multiple messages together before calling the `Flush` method).

### How does it work?

The Socket Dispatcher does not "select sockets". It means that it doesn't spawn any timer on the dispatcher to continuously check if new data has arrived or is ready to be sent. Instead, it hooks directly into the native wsock32 API which provides an option to publish network events on a STA thread, which the dispatcher (by using the `ComponentDispatcher.ThreadFilterMessage` handler) can then intercept and consume.

### One more time: why?

I've created this library mostly for my private use and to experiment a bit with the lower level networking. Nevertheless, if there is a functionality that you see missing here or if you've discovered a bug worth fixing, feel free to drop a PR or create a github ticket.
