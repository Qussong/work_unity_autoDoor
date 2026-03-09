using System;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 비디오 재생을 관리하는 싱글톤 매니저 클래스
/// </summary>
public class VideoManager : MonoSingleton<VideoManager>
{
    [Header("=== UI 설정 ===")]
    public RawImage _rawImage;              // 비디오가 표시될 RawImage

    [Header("=== 비디오 설정 ===")]
    public string _videoName;               // StreamingAssets 폴더 내 비디오 파일명
    public int _resolutionWidth;            // 렌더 텍스처 가로 해상도
    public int _resolutionHeight;           // 렌더 텍스처 세로 해상도

    private VideoPlayer _videoPlayer;       // 비디오 재생을 담당하는 컴포넌트
    private RenderTexture _renderTexture;   // 비디오 프레임을 저장하는 텍스처 버퍼

    public Action<VideoPlayer> _videoEndHandler;    // 비디오 재생 완료 시 호출되는 이벤트 핸들러

    #region 유니티 이벤트 함수 + 추사 함수 구현

    /// <summary>
    /// VideoPlayer 및 RenderTexture 설정
    /// </summary>
    protected override void OnSingletonAwake()
    {
        // RenderTexture 생성 (비디오 출력을 담을 버퍼)
        _renderTexture = new RenderTexture(_resolutionWidth, _resolutionHeight, 0);

        // VideoPlayer 컴포넌트 추가 및 설정
        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
        string videoPath = Path.Combine(Application.streamingAssetsPath, _videoName);
        _videoPlayer.url = videoPath;                   // 비디오 파일 경로 설정
        _videoPlayer.targetTexture = _renderTexture;    // 비디오 출력 대상을 RenderTexture로 지정
        _videoPlayer.playOnAwake = false;               // 시작 시 자동 재생 방지
        _videoPlayer.isLooping = false;                 // 반복 재생 방지

        // RawImage에 RenderTexture 연결 (화면에 표시)
        _rawImage.texture = _renderTexture;

        // 비디오 재생 완료 이벤트 등록 
        _videoPlayer.loopPointReached += (vp) => _videoEndHandler?.Invoke(vp);
    }

    protected override void OnSingletonApplicationQuit()
    {
        // 앱 종료 시 정리 작업 (필요 시 구현)
    }

    protected override void OnSingletonDestroy()
    {
        // 오브젝트 파괴 시 정리 작업 (필요 시 구현)
    }

    #endregion

    #region 외부 호출 함수

    /// <summary>
    /// 비디오 재생 시작
    /// </summary>
    public void PlayVideo()
    {
        _videoPlayer?.Play();
    }

    /// <summary>
    /// 현재 비디오가 재생 중인지 확인
    /// </summary>
    public bool IsVideoPlaying()
    {
        return _videoPlayer.isPlaying;
    }

    /// <summary>
    /// 비디오 정지 및 화면 초기화 (검은 화면으로)
    /// </summary>
    public void ResetVideo()
    {
        _videoPlayer.Stop();

        // RenderTexture 를 검은색으로 Clear
        ClearRenderTexture(Color.black);

        // 대기화면2 보여주기

    }

    #endregion

    #region 내부 호출 함수

    /// <summary>
    /// RenderTexture를 검은색으로 클리어
    /// 비디오 종료 후 화면 초기화에 사용
    /// </summary>
    public void ClearRenderTexture(Color clearColor)
    {
        RenderTexture.active = _renderTexture;  // 작업할 RenderTexture 지정 (활성화)
        GL.Clear(true, true, clearColor);       // 클리어
        RenderTexture.active = null;            // 작업 완료, 연결 해제
    }

    #endregion

}
