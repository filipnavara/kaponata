# The motivation for the `KubernetesClient` class

Although the Kubernetes project provides an official .NET client for Kubernetes: [KubernetesClient][1], Kaponata has its own
`KubernetesClient` class.

It's a thin wrapper around the `IKubernetes` interface which is part of the "official" [KubernetesClient][1], and attempts to make
interacting with the Kubernets APIs in a .NET project a bit more enjoyable.

It tries to achieve a couple of goals, which are outlined in this document.

## Support mocking

Not all of the APIs which the [KubernetesClient][1] exposes are methods on the `IKubernetes` interface. For example, some of them
are extension methods. This makes mocking the Kubernetes API during unit testing cumbersome.

For this reason:

> All Kubernetes APIs which are used by the Kaponata project should be accessible as a `virtual` method on the `KubernetesClient` interface.

## Simple watch algorithms

The Kaponata operators observe the Kubernetes state and want to run a reconciliation loop whenever the state changes. This involves
watching Kubernetes objects.

[KubernetesClient][1] supports watching objects, via the `Watch<T>` object. Let's say you want to wait asynchronously wait for a
pod to be deleted, with support for cancellation and timeouts. You may end up with code like this:

```csharp
private async Task WaitForPodDeleted(V1Pod pod, TimeSpan timeout, CancellationToken cancellationToken)
{
    TaskCompletionSource tcs = new TaskCompletionSource();
    cancellationToken.Register(tcs.SetCanceled);

    using (var response =
        await this.protocol.ListNamespacedPodWithHttpMessagesAsync(
            pod.Metadata.NamespaceProperty,
            fieldSelector: $"metadata.name={pod.Metadata.Name}",
            watch: true,
            cancellationToken: cancellationToken).ConfigureAwait(false))
    using (var watcher = response.Watch<V1Pod, V1PodList>(
        onEvent: (eventType, pod) =>
        {
            if (eventType == WatchEventType.Deleted)
            {
                tcs.SetResult();
            }
        },
        onError: (ex) =>
        {
            tcs.SetException(ex);
        },
        onClosed: () =>
        {
            tcs.SetCanceled();
        }))
    {
        if (await Task.WhenAny(tcs.Task, Task.Delay(timeout)).ConfigureAwait(false) != tcs.Task)
        {
            throw new TimeoutException();
        }
    }
}
```

That's a lot of code for a simple operation.

At the core, the `Watcher<T>` class has an inner task which reads objects off a HTTP stream and invokes a callback. But
that task is not exposed to the consumers of the `Watcher<T>` class. This means you'll need to use a `TaskCompletionSource`
to reconstruct a task-like API. At the end, there are _two_ tasks running instead of one, and that feels like overhead.

The `Watcher<T>` class _does not_ support mocking: there is no interface you can mock nor are the methods it exposes
virtual.

Finally, the `Watcher<T>` class also suffers from some issues - for example, it [does not attempt to reconnect when the
connection with the Kubernetes server times out][2].

For this reason:

> The `KubernetesClient` class implements its own API for watching. It returns a `Task` which will complete when the watch
> operation has completed, and notifies the caller of the reason for completion. It takes a single `onEvent` callback, which
> the caller can use to instruct the watcher to continue watching for events or to exit:
> ```csharp
> Task<WatchExitReason> WatchPodAsync(
>            V1Pod pod,
>            Func<WatchEventType, V1Pod, Task<WatchResult>> onEvent,
>            CancellationToken cancellationToken);
> ```

## Friendly exceptions

By default, the [KubernetesClient][1] will throw a `HttpOperationException` when something goes wrong. This exception will have
a generic error message, like `Microsoft.Rest.HttpOperationException: 'Operation returned an invalid status code '422'`.

The Kubernetes API server actually returns more detailed 

[This was closed as by design][3], but having to dig into an exception property to get the actual error message is not acceptable to the Kaponata project. Therefore:

> The methods on the `KubernetesClient` will intercept `HttpOperationException` values and, where possible:
> 1. Extract the `V1Status` object from the API server response.
> 2. Throw an exception which includes the `V1Status.Message` in the exception message.
> 3. Include the `V1Status` object in a property of the exception.

## `TryRead*` methods

Code may want to check for the existence of a specific Kubernetes object. By default, the `Read*`/`Get*` methods in the 
[KubernetesClient][1] will throw an exception when the object was not found which you need to catch.

Alternatively, you can do a `List*` operation and check whether the list is empty or not. This results in overhead.

For this reason:

> The `KubernetesClient` will exposes a `V1* TryRead*(string namespace, string name, CancellationToken cancellationToken)` 
> method which returns the object if found, or `null` if the object does not exist.W

## Overall API shape

The current API shape for an individual object looks like this:

```csharp
Task<V1Pod> CreatePodAsync(V1Pod value, CancellationToken cancellationToken);
Task<V1Pod> TryReadPodAsync(string @namespace, string name, CancellationToken cancellationToken);
Task DeletePodAsync(V1Pod value, TimeSpan timeout, CancellationToken cancellationToken);
```

[1]: https://github.com/kubernetes-client/csharp
[2]: https://github.com/kubernetes-client/csharp/issues/533
[3]: https://github.com/kubernetes-client/csharp/issues/58