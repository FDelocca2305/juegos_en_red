using System.Collections;
using UnityEngine;

public static class ServiceLocatorUtil
{
    public static IEnumerator WaitFor<T>(System.Action<T> onReady) where T : class
    {
        T svc;
        while (!ServiceLocator.TryResolve(out svc))
            yield return null;

        onReady?.Invoke(svc);
    }
}