# work_unity_autoDoor

> 인천 달동네 전시 설치물 — 관람객이 문을 통과하면 영상을 재생하고 자동문을 개방하는 인터랙티브 키오스크 시스템

- **엔진**: Unity (C#)
- **플랫폼**: Windows (Standalone)
- **핵심 기능**: 시리얼 센서 감지 → 영상 재생 → 자동문 개방 신호 전송

---

## 아키텍처

**FSM(Finite State Machine) + MonoSingleton** 구조를 기반으로 한다.

| 레이어 | 역할 |
|---|---|
| **FSM** | 앱 상태 전환 관리 (`StateMachine`, `IState`, `BaseState`) |
| **View** | 각 상태에 대응하는 UI 표현 (`BaseView` 상속) |
| **Manager** | 도메인별 독립 기능 (시리얼, 영상, 설정, 유휴 감지) |
| **Util** | 범용 유틸리티 (`MonoSingleton`, `CanvasGroupExtensions`) |

---

## 디렉토리 구조

```
Assets/
└── Scripts/
    ├── FSM/
    │   ├── IState.cs               # 상태 인터페이스 (Enter/Update/Exit)
    │   ├── BaseState.cs            # 제네릭 상태 베이스 (TState, TView)
    │   ├── StateMachine.cs         # 상태 관리 엔진
    │   └── States/
    │       ├── StartState.cs       # 핵심 로직 (센서 → 영상 → 문 개방)
    │       ├── ContentState.cs     # (미사용, 예비)
    │       └── ResultState.cs      # (미사용, 예비)
    ├── View/
    │   ├── BaseView.cs             # Show/Hide/Toggle 베이스
    │   ├── StartView.cs            # 영상 안내, 문 안내, 디버그 UI
    │   ├── ContentView.cs          # (미사용, 예비)
    │   └── ResultView.cs           # (미사용, 예비)
    ├── Util/
    │   ├── MonoSingleton.cs        # 스레드세이프 싱글턴 베이스
    │   └── CanvasGroupExtensions.cs# FadeIn/FadeOut/Activate/DeActivate
    ├── NavigationManager.cs        # FSM 진입점, 상태 등록
    ├── IdleManager.cs              # 무입력 타임아웃 관리
    ├── SerialManager.cs            # RS232 시리얼 통신
    ├── VideoManager.cs             # 영상 재생 (VideoPlayer + RenderTexture)
    └── CSVParser.cs                # StreamingAssets CSV 설정 파싱
```

---

## 주요 클래스

| 클래스 | 타입 | 역할 |
|---|---|---|
| `StateMachine` | MonoBehaviour | Dictionary 기반 FSM, `OnStateChanged` 이벤트 제공 |
| `StartState` | BaseState | 센서 수신 → 영상 재생 → 문 개방 신호 전체 흐름 |
| `BaseView` | MonoBehaviour | 루트 패널 Show/Hide, `OnShow`/`OnHide` 이벤트 |
| `StartView` | BaseView | 영상 안내 이미지, 문 안내 이미지, 디버그 텍스트 UI |
| `NavigationManager` | MonoSingleton | StateMachine 생성 및 상태 등록, 상태 전환 래핑 |
| `IdleManager` | MonoSingleton | 입력 감지, 60초 무입력 시 `StartState`로 복귀 |
| `SerialManager` | MonoSingleton | 시리얼 포트 송수신, 메인스레드 큐 처리 |
| `VideoManager` | MonoSingleton | VideoPlayer + RenderTexture → RawImage 출력 |
| `CSVParser` | MonoSingleton | `StreamingAssets/Config.csv` 키-값 파싱 |
| `MonoSingleton<T>` | MonoBehaviour | 스레드세이프, 앱 종료 안전, DontDestroyOnLoad |
| `CanvasGroupExtensions` | static | CanvasGroup Fade/Activate/DeActivate 확장 메서드 |

---

## 화면 / 기능 흐름

```
[앱 시작]
    │
    ▼
[StartState] ─── 대기 화면 표시 (영상 없음)
    │
    │  시리얼 수신 data[1] == 0x31  (또는 Space 키 디버그)
    ▼
[PlayDelayTime ms 대기] + 영상 안내 이미지 표시
    │
    ▼
[VideoManager.PlayVideo()] ─── 영상 재생
    │
    │  영상 종료 (loopPointReached)
    ▼
[OpenDoorSignal]
    ├─ 문 안내 이미지 표시
    ├─ SendData(0x31) × DoorSignalLoopCnt 회 (1초 간격)
    └─ WaitTime ms 대기
    │
    ▼
[대기 상태 복귀] ─── 다음 관람객 대기
    │
    │  60초 무입력 (IdleManager)
    ▼
[NavigationManager.GoTo<StartState>()]
```

---

## 데이터 흐름

### 시리얼 수신 흐름

```
SerialPort (백그라운드 코루틴)
    │  BytesToRead > 0 → Read()
    ▼
_mainThreadQueue.Enqueue()   ← lock(_lock) 보호
    │
    ▼  (Update() 메인스레드)
ReceiveDataHandler?.Invoke(data)
    │
    ▼
StartState.PlayVideo(data)   ← data[1] == 0x31 검증
```

### 설정 로드 흐름

```
StreamingAssets/Config.csv
    │  CSVParser.ReadCSVFile()
    ▼
_configDataContainer (Dictionary<string, string>)
    │  GetConfigData(key)
    ▼
SerialManager  → PortName
StartState     → PlayDelayTime, DoorSignalLoopCnt, WaitTime
```

---

## 설정 파일 (Config.csv)

`StreamingAssets/Config.csv` 에 위치. 빌드 재배포 없이 현장 수정 가능.

```
키,값
PortName,COM3
PlayDelayTime,3000
DoorSignalLoopCnt,5
WaitTime,2000
```

| 키 | 설명 | 단위 |
|---|---|---|
| `PortName` | 시리얼 포트명 | 문자열 (예: COM3) |
| `PlayDelayTime` | 센서 감지 후 영상 재생까지 대기 시간 | ms |
| `DoorSignalLoopCnt` | 문 개방 신호 전송 횟수 | 회 |
| `WaitTime` | 문 신호 완료 후 복귀 전 대기 시간 | ms |

---

## 디버그 조작 (에디터/개발 빌드)

| 키 | 동작 |
|---|---|
| `Space` | 센서 신호 수신 시뮬레이션 (영상 재생 트리거) |
| `D` | 디버그 상태 패널 토글 (Content/Door 상태 텍스트) |

---

## 외부 플러그인

| 플러그인 | 용도 |
|---|---|
| Better Hierarchy | 유니티 하이어라키 창 가시성 향상 (에디터 전용) |
| Gaskellgames GgCore | 커스텀 인스펙터 어트리뷰트 (`ShowIf`, `ReadOnly` 등) |

---

## 변경 이력

| 날짜 | 변경 내용 |
|---|---|
| 2026-03-10 | README 최초 작성 |
