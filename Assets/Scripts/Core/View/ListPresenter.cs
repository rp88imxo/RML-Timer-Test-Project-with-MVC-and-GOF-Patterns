using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RML.Core
{
[Serializable]
public abstract class ListPresenterBase<T> where T : MonoBehaviour
{
    private const int MAX_POOL_SIZE = 100;

    [SerializeField]
    protected T _prefab;

    [SerializeField]
    protected Transform _container;

    public Transform Container => _container;

    private List<T> _pool = new List<T>();
    protected List<T> Pool => _pool ??= new List<T>();

    public IReadOnlyList<T> ViewsBase => Pool;

    protected int ActiveElementsCount { get; private set; }

    public void HideContainer() => _container.gameObject.SetActive(false);
    public void ShowContainer() => _container.gameObject.SetActive(true);

    protected T GetObject()
    {
        var obj = TryGetFromPool(out var pooledObj)
            ? pooledObj
            : CreateObject();

        obj.gameObject.SetActive(true);
        return obj;
    }

    private void RemoveObject(T obj)
    {
        bool success = AttemptToPushToPool(obj);
        if (!success) DestroyObject(obj);
    }

    private bool AttemptToPushToPool(T obj)
    {
        if (!Pool.Contains(obj)) return false;

        bool alreadyInPool = !obj.gameObject.activeSelf;
        if (alreadyInPool) return true;

        if (obj is IPoolingResettable resettable) resettable.ResetState();

        obj.gameObject.SetActive(false);
        ActiveElementsCount--;

        return true;
    }

    private T CreateObject()
    {
        var obj = Object.Instantiate(_prefab, _container);
        obj.name = $"{_prefab.name} ({Pool.Count})";
        return obj;
    }

    private void DestroyObject(T obj) { Object.Destroy(obj.gameObject); }

    private bool TryGetFromPool(out T obj)
    {
        bool hasInactiveElements = ActiveElementsCount < Pool.Count;
        if (!hasInactiveElements) return TryAddNewToPool(out obj);

        obj = Pool[ActiveElementsCount];
        ActiveElementsCount++;

        return true;
    }

    private bool TryAddNewToPool(out T obj)
    {
        if (Pool.Count >= MAX_POOL_SIZE)
        {
            obj = default;
            return false;
        }

        obj = CreateObject();
        Pool.Add(obj);
        ActiveElementsCount++;
        return true;
    }

    public virtual void Clear()
    {
        foreach (var obj in Pool)
        {
            RemoveObject(obj);
        }
    }
}

[Serializable]
public class ListPresenter<T> : ListPresenterBase<T>
    where T : MonoBehaviour
{
    [SerializeField]
    private bool _hideContainerWhileRepaint;

    public void Repaint(int count, Action<T, int> onCreated = null)
    {
        Clear();

        if (_hideContainerWhileRepaint) HideContainer();
        CreateViews(count, onCreated);
        if (_hideContainerWhileRepaint) ShowContainer();
    }

    private void CreateViews(int count, Action<T, int> onCreated)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = GetObject();

            onCreated?.Invoke(obj, i);
        }
    }

    public async UniTask Repaint(int count,
        Func<T, int, UniTask> onCreated)
    {
        Clear();

        if (_hideContainerWhileRepaint) HideContainer();
        await CreateViews(count, onCreated);
        if (_hideContainerWhileRepaint) ShowContainer();
    }

    private async UniTask CreateViews(int count,
        Func<T, int, UniTask> onCreated)
    {
        var tasks = new List<UniTask>();

        for (int i = 0; i < count; i++)
        {
            var view = GetObject();
            var task = onCreated(view, i);
            tasks.Add(task);
        }

        await UniTask.WhenAll(tasks);
    }

    public T this[int index]
    {
        get => Pool[index];
        set => Pool[index] = value;
    }
}

[Serializable]
public class ListPresenter<TData, TView> : ListPresenterBase<TView>
    where TView : MonoBehaviour
{
    [SerializeField]
    private bool _hideContainerWhileRepaint;

    public Dictionary<TData, TView> Views
    {
        get => _views ??= new Dictionary<TData, TView>();
        private set => _views = value;
    }

    private Dictionary<TData, TView> _views;

    public void Push(TData data, Action<TData, TView> onCreated)
    {
        var view = GetObject();

        onCreated?.Invoke(data, view);

        Views.Add(data, view);
    }

    public void Repaint(IEnumerable<TData> data,
        Action<TData, TView> onCreated)
    {
        Clear();

        if (_hideContainerWhileRepaint) HideContainer();
        CreateViews(data, onCreated);
        if (_hideContainerWhileRepaint) ShowContainer();
    }

    private void CreateViews(IEnumerable<TData> data,
        Action<TData, TView> onCreated)
    {
        foreach (var item in data)
        {
            Push(item, onCreated);
        }
    }

    public async UniTask Repaint(IEnumerable<TData> data,
        Func<TData, TView, UniTask> onCreated)
    {
        Clear();

        if (_hideContainerWhileRepaint) HideContainer();
        await CreateViews(data, onCreated);
        if (_hideContainerWhileRepaint) ShowContainer();
    }

    private async UniTask CreateViews(IEnumerable<TData> data,
        Func<TData, TView, UniTask> onCreated)
    {
        var tasks = new List<UniTask>();
        foreach (var item in data)
        {
            var view = GetObject();

            var task = onCreated.Invoke(item, view);
            tasks.Add(task);

            Views.Add(item, view);
        }

        await UniTask.WhenAll(tasks);
    }

    public override void Clear()
    {
        base.Clear();
        Views.Clear();
    }

    public TView this[TData key]
    {
        get => Views[key];
        set => Views[key] = value;
    }
}
}