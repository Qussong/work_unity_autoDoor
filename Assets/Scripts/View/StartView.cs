using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartView : BaseView
{
    [Header("=== Image ===")]
    //public Image _imgBackground;
    public Image _imgVideoInfo;
    public Image _imgDoorInfo;

    [Header("=== Container ===")]
    public GameObject _objDebugContainer;

    [Header("=== Text ===")]
    public TMP_Text _txtContentStatus;      // 콘텐츠 상태
    public TMP_Text _txtContentPlayable;    // 콘텐츠 플레이 가능여부
    public TMP_Text _txtDoorStatus;         // 문 상태


}
