# RxRedis

RxRedis is a library that implements [ReactiveX](http://reactivex.io/) 
patterns with [Redis](http://redis.io/) as a transport layer on .Net.

Goals
=====

The main goal is to allow event streams to send data between processes 
using Redis as a broker while making sure that the behavior is as close
as possible to already known concepts.
Secondly, RxRedis should leverage composability and implement only basic blocks
while making sure that existing operators will work as expected.

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

Note, that changing type of `Subject<T>` can change behavior of published observable stream,
i.e. using `AsyncSubject<T>` will publish only last event and only after stream is marked completed.

### RxRedis

RxRedis offers drop-in replacements for subjects. To use 
