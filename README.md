[![NuGet](https://img.shields.io/nuget/v/RxRedis.svg)](https://www.nuget.org/packages/RxRedis)

# RxRedis

RxRedis is a .Net library that implements [ReactiveX](http://reactivex.io/) 
patterns using [Redis](http://redis.io/) as a transport layer.

Features
=====

* Ability to use composable event streams and operators from Rx.Net between processes
* Uses Redis as a broker for message passing
* New subjects / observables can be used with Rx.Net operators seamlessly
* Support for different subject types behaviors over the wire.  


Installation
============
RxRedis can be installed via the nuget UI, or via the nuget package manager console:
```
PM> Install-Package RxRedis
```

RxRedis depends on [Rx.NET](https://github.com/Reactive-Extensions/Rx.NET), [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) and [Jil](https://github.com/kevin-montrose/Jil). 

Usage
=====

Subjects
--------

One way of generating streams of events in ReactiveX are implementations of `ISubject<T>`.
They can be exposed to observers through `IObservable<T>` interface, i.e.:

```CSharp
private readonly Subject<EventData> eventsSubject;
public IObservable<EventData> Events => eventsSubject.AsObservable();

public Processor() 
{
    eventsSubject = new Subject<EventData>();
}

private void Process() 
{
    eventsSubject.onNext(new EventData { ... });    
}


[...]
{
    processor.Events.Subscribe( ev => { ... }, error => { ... });
}
    
``` 

Note, that changing type of `Subject<T>` can change behavior of the published observable stream,
i.e. using `AsyncSubject<T>` will publish only the last event and only after the stream is marked completed.

### RxRedis

RxRedis offers drop-in replacements for subjects. To use RxRedis's subjects just change constructor call:

```CSharp
var redis = ConnectionMultiplexer.Connect("localhost");
var subject = new RedisSubject<EventData>("subjectName", redis);
```

`RedisSubject<T>` requires two additional parameters: `subjectName: string` and `redisConnection: IConnectionMultiplexer`. 

Name is used to uniquely identify each subject. 
It can be later used to connect to a subject from different processes. 
Note, no 2 subjects should share the same name, otherwise there can be conflicts; this is even more true for different types of subjects like `AsyncSubject<T>`.

Redis connection is an interface that will be used by a subject to communicate with redis.

Observables
---

Once created, subject can be used as an observable inside a process, however, all emitted events will go through Redis cluster, instead of being passed in memory.

To observe those events from any other processes you can create an instance of the second biggest building block of RxRedis - `RedisObservable<T>`.

```CSharp
IObservable<EventData> observable = RedisObservable.Create<EventData>("subjectName", redis);
var sub = observable.Subscribe(...);
```

Once created, observable can be composed using Rx.NET operators.

Observables respect publishing subject's type, so ie. if publisher is of type `RedisSubject<T>`, old events won't be observed, however, if publisher is `RedisAsyncSubject<T>`,
all connected observables will see only last event and only if subject is marked completed. You can consult [ReactiveX docs](http://reactivex.io/documentation/subject.html) to check the behavior.


Serialization
-------------

Internally, all events are serialized to JSON format using [Jil](https://github.com/kevin-montrose/Jil).
Because of this, objects do not have to be marked with special attributes, however, if they are can't be serialized with Jil (ie. because of circular references) - error will be raised in `OnNext` method.

Error handling
--------------

When subject goes into faulted state (most commonly because of calling `OnError` method) error is published to Redis, so all observables are notified of this properly. 
However, currently the type of an exception is lost on the wire, so exception that is observed contains original message (`Exception.Message`) but its type is always the most generic: `Exception`.

Policy regarding how other errors (like network errors) are handled is still under consideration (hence this is still early version).


