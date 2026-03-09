using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class StartState : BaseState<StartState, StartView>
{
    private bool _bActiveContent = true;
    private bool _bRegisterEvent = false;
    private bool _bSomeonePassDoor = false;     // 문을 지나간 사람이 있는지 확인하는 플래그

    private bool _bDebugFlag = false;

    // Debug
    private CancellationTokenSource _timerCts;

    public StartState(StartView view) : base(view)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 안내 메세지 이미지 숨김
        _view._imgVideoInfo.GetComponent<CanvasGroup>().DeActivate();
        _view._imgDoorInfo.GetComponent<CanvasGroup>().DeActivate();

        // RenderTexture 정리
        VideoManager.Instance.ResetVideo();

        // 이벤트 리스너 등록
        if (false == _bRegisterEvent)
        {
            _bRegisterEvent = true;
            RegisterEvent();
        }

        // 디버깅) 초기화
        _view._txtContentStatus.text = "Wait";
        _view._txtContentPlayable.text = "Yes";
        _view._txtDoorStatus.text = "Close";
        _view._objDebugContainer.GetComponent<CanvasGroup>().DeActivate();
        _bDebugFlag = false;

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // test
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _bSomeonePassDoor = true;   // 문을 지나간 사람이 존재함
            // 동영상 재생
            _ = PlayVideoDelay();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (false == _bDebugFlag)
            {
                _bDebugFlag = true;
                _view._objDebugContainer.GetComponent<CanvasGroup>().Activate();
            }
            else
            {
                _bDebugFlag = false;
                _view._objDebugContainer.GetComponent<CanvasGroup>().DeActivate();
            }
        }

    }

    private async Task PlayVideoDelay()
    {
        // 디버깅) 콘텐츠 시작
        _view._txtContentStatus.text = "Play";

        // 안내 화면 노출
        _view._imgVideoInfo.GetComponent<CanvasGroup>().Activate();

        // 동영상 재생 대기
        string strDelayTime = CSVParser.Instance.GetConfigData("PlayDelayTime");
        await Task.Delay(int.Parse(strDelayTime));

        // 안내 화면 숨김
        _view._imgVideoInfo.GetComponent<CanvasGroup>().DeActivate();

        // 사람 인식 플래그 off
        _bSomeonePassDoor = false;

        // 비디오 플레이
        VideoManager.Instance.PlayVideo();
    }

    private void RegisterEvent()
    {
        SerialManager.Instance.ReceiveDataHandler += PlayVideo;
        VideoManager.Instance._videoEndHandler += EndVideo;
    }

    private void PlayVideo(Byte[] data)
    {
        if (data[1] == 0x31
            && _bActiveContent == true
            && _bSomeonePassDoor == false
            && false == VideoManager.Instance.IsVideoPlaying())
        {
            _bSomeonePassDoor = true;   // 문을 지나간 사람이 존재함
            // 동영상 재생
            _ = PlayVideoDelay();
        }
    }

    private void EndVideo(VideoPlayer vp)
    {
        // 콘탠츠 대기시작
        _bActiveContent = false;
        // 디버깅) 콘텐츠 상태
        _view._txtContentStatus.text = "End";
        // 디버깅) 콘텐츠 사용가능 여부
        _view._txtContentPlayable.text = "No";

        // RenderTexture 정리
        VideoManager.Instance.ResetVideo();

        // 1초마다 "DoorSignalLoopCnt" 횟수만큼 문열림 신호 보냄
        _ = OpenDoorSignal();
    }

    private async Task OpenDoorSignal()
    {
        // 안내 화면 노출
        _view._imgDoorInfo.GetComponent<CanvasGroup>().Activate();

        string strLoopCnt = CSVParser.Instance.GetConfigData("DoorSignalLoopCnt");
        int loopCnt = int.Parse(strLoopCnt);

        // 디버깅) 문 상태
        _view._txtDoorStatus.text = "Open";
        for (int i = 0; i < loopCnt; ++i)
        {
            // 문 열림 신호 전송
            SerialManager.Instance.SendData(new byte[] { 0x31 });

            //Debug.Log($"문열림 {i}");
            await Task.Delay(1000);
        }
        // 디버깅) 문 상태
        _view._txtDoorStatus.text = "Close";

        // 문 닫힐때까지 대기
        string strWaitTime = CSVParser.Instance.GetConfigData("WaitTime");
        await Task.Delay(int.Parse(strWaitTime));

        // 안내화면 숨김
        _view._imgDoorInfo.GetComponent<CanvasGroup>().DeActivate();

        //Debug.Log("콘텐츠 대기 해제");
        // 콘텐츠 대기 해제
        // 디버깅) 콘텐츠 상태
        _view._txtContentStatus.text = "Wait";
        _bActiveContent = true;
        // 디버깅) 콘텐츠 사용 가능 여부
        _view._txtContentPlayable.text = "Yes";
    }

}
