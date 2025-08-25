using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();
    
    public static void Register<TService>(TService instance) where TService : class
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance), $"Tried to register null for {typeof(TService).Name}");

        _services[typeof(TService)] = instance;
    }
    
    public static void Deregister<TService>(TService instance) where TService : class
    {
        var key = typeof(TService);
        if (_services.TryGetValue(key, out var current) && ReferenceEquals(current, instance))
            _services.Remove(key);
    }
    
    public static TService Resolve<TService>() where TService : class
    {
        if (_services.TryGetValue(typeof(TService), out var instance))
            return (TService)instance;

        throw new InvalidOperationException($"Service not found: {typeof(TService).Name}. Did you register it in ServiceInstaller?");
    }
    
    public static bool TryResolve<TService>(out TService service) where TService : class
    {
        if (_services.TryGetValue(typeof(TService), out var instance))
        {
            service = (TService)instance;
            return true;
        }
        service = null;
        return false;
    }
    
    public static void Reset() => _services.Clear();
}