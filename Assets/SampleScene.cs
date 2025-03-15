using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SampleScene : MonoBehaviour
{
    void Start()
    {
        var test = new TestClass();
        test.Main().Forget();
    }
}

public class TestClass
{
    private readonly TestAsset[] _assets = new TestAsset[5];
    
    public async UniTask Main()
    {
        await Addressables.InitializeAsync();

        for (var j = 0; j < 100; j++)
        {
            for (var i = 0; i < 100; i++)
            {
                var index = i % 5;
                _assets[index]?.Dispose();
                _assets[index] = new TestAsset();
                _assets[index].Load((int) (Random.value * 3) + 1).Forget();
            }
            await UniTask.Yield();
        }
    }
}

/// <summary>
/// TestAsset
/// 再利用は禁止だよ
/// </summary>
public class TestAsset
{
    private AsyncOperationHandle<Sprite> _handle;
    private CancellationTokenSource _canceler;

    public async UniTask Load(int id)
    {
        _canceler = new CancellationTokenSource();
        var file = $"Assets/Images/white{id:D3}.png";
        _handle = Addressables.LoadAssetAsync<Sprite>(file);
        var sprite = await _handle.WithCancellation(_canceler.Token);
        Debug.Log($"Load {file}, {_handle.DebugName}, {sprite.name}");
    }
    
    public void Dispose()
    {
        _canceler?.Cancel();
        _canceler?.Dispose();
        _canceler = null;

        if (_handle.IsValid())
            Addressables.Release(_handle);
    }
}
















