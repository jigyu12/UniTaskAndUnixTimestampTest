using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ServerAndDeviceTimeLogger : MonoBehaviour
{
    private void Start()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        
        LogTimeAsync(ct).SuppressCancellationThrow().Forget();
    }

    private async UniTask LogTimeAsync(CancellationToken ct)
    {
        try
        {
            long deviceUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long? googleUnix = await GetGoogleServerUnixTimeAsync(ct);

            if (!googleUnix.HasValue)
            {
                Debug.LogWarning("Google server time request failed.");
            
                return;
            }
        
            Debug.Log($"[Time] Device UTC (Unix s): {deviceUnix}");
            Debug.Log($"[Time] Google Server UTC (Unix s): {googleUnix.Value}");
        
            var deviceDto = DateTimeOffset.FromUnixTimeSeconds(deviceUnix);
            var googleDto = DateTimeOffset.FromUnixTimeSeconds(googleUnix.Value);
            Debug.Log($"[Time] Device UTC (ISO8601): {deviceDto.ToUniversalTime():O}");
            Debug.Log($"[Time] Device Local: {deviceDto.ToLocalTime():yyyy-MM-dd HH:mm:ss zzz}");
            Debug.Log($"[Time] Google UTC (ISO8601): {googleDto.ToUniversalTime():O}");
            Debug.Log($"[Time] Google Local: {googleDto.ToLocalTime():yyyy-MM-dd HH:mm:ss zzz}");
        }
        catch (OperationCanceledException)
        {
            // 의도된 취소
            return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            
            return;
        }
    }

    private async UniTask<long?> GetGoogleServerUnixTimeAsync(CancellationToken ct)
    {
        try
        {
            using var request = UnityWebRequest.Head("https://www.google.com");
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose   = true;
            request.disposeCertificateHandlerOnDispose = true;
            
            await request.SendWebRequest().ToUniTask(cancellationToken: ct);

            bool isRequestFailed = request.result != UnityWebRequest.Result.Success;
            if (isRequestFailed)
            {
                Debug.LogWarning($"Google Time Request failed: {request.error} (code = {request.responseCode})");
            
                return null;
            }
        
            string date = request.GetResponseHeader("Date");
            bool isInvalidDate = string.IsNullOrEmpty(date);
            if (isInvalidDate)
            {
                Debug.LogWarning("Date header missing.");
            
                return null;
            }

            if (!DateTimeOffset.TryParse(date, out var dto))
            {
                Debug.LogWarning($"Date parse failed. raw = {date}");
            
                return null;
            }
        
            return dto.ToUnixTimeSeconds();
        }
        catch (OperationCanceledException)
        {
            // 의도된 취소
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            
            return null;
        }
    }
}