using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UniTaskTest : MonoBehaviour
{
    private CancellationTokenSource cts;
    [SerializeField] private Button cancelButton;

    private void Start()
    {
        cts = new();
        
        StartAsync(cts.Token).Forget();
        
        cancelButton.onClick.AddListener(CancelUniTask);
    }

    private async UniTaskVoid StartAsync(CancellationToken token)
    {
        while (true)
        {
            // await UniTask.Yield(); 한 프레임 대기
            // await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: true); 타임 스케일 무시하고 대기
            // await UniTask.Delay(500); 500ms 대기 (Time.timeScale의 영향 받음)
            // await UniTask.DelayFrame(3); n프레임 대기
            // await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate); 특정 PlayerLoop 타이밍에 양보
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
            Debug.Log("StartAsync");
        }
    }

    private void CancelUniTask()
    {
        cts.Cancel();
    }

    private void OnDestroy()
    {
        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

    private void RunTestInvoke()
    {
        Invoke(nameof(Test), 1f);
    }

    private IEnumerator RunTestCoroutine()
    {
        yield return new WaitForSeconds(1f);
        
        Test();
    }
    
    private async UniTask RunTestUniTask()
    {
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        
            Test();
        }
    }
    
    private void Test()
    {
        Debug.Log("Test");
    }
}