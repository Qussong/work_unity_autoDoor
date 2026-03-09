using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 
/// 현재 csv 테이블 형태 : 
///     헤더1, 헤더2
///     데이터 이름1, 데이터1
///     데이터 이름2, 데이터2
///     ....
///     
/// </summary>

public class CSVParser
    : MonoSingleton<CSVParser>
{
    // 테스트용 컨테이너
    //private Dictionary<string, string> dataContainer = new Dictionary<string, string>();
    private Dictionary<string, string> _configDataContainer = new Dictionary<string, string>();

    #region 추상 함수 구현 + 유니티 이벤트 함수

    protected override void OnSingletonAwake()
    {
        //TestCode();
        ReadCSVFile("Config.csv", _configDataContainer);
    }

    /*private void TestCode()
    {
        bool result = ReadCSVFile("Test.csv", dataContainer);

        if (result)
        {
            // ValueTuple 활용
            int idx = 0;
            foreach ((string k, string v) in dataContainer)
            {
                Log($"[{idx}] {k} - {v}");
                ++idx;
            }
        }
    }*/

    protected override void OnSingletonApplicationQuit()
    {

    }

    protected override void OnSingletonDestroy()
    {

    }

    #endregion


    #region 내부 호출 함수

    private bool ReadFile(string path, Dictionary<string, string> container)
    {
        // 파일 존재여부 확인
        if (!File.Exists(path))
            return false;

        try
        {
            // 지정한 파일(path)을 열고, 그 파일을 읽기 위한 StreamReader를 생성
            using (StreamReader sr = File.OpenText(path))
            {
                // 파일 전체 내용을 한 번에 문자열로 읽어온다.
                string openText = sr.ReadToEnd();
                // \r\n, \n 모두 처리
                string[] textArray = openText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int idx = 0; idx < textArray.Length; idx++)
                {
                    // 헤더 스킵 (0 번 줄)
                    if (idx < 1)
                        continue;

                    string line = textArray[idx];

                    // 빈 줄 스킵 = 공백만 있는 문자열 제거 ex) "      "
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] dataArray = line.Split(',');

                    // 데이터 유효성 체크 (CSV 파일 형태 : 데이터 이름, 데이터)
                    if (dataArray.Length < 2)
                        continue;

                    // Trim()으로 앞뒤 공백 제거
                    string key = dataArray[0].Trim();
                    string value = dataArray[1].Trim();

                    // 중복 키 처리
                    if (!container.ContainsKey(key))
                        container.Add(key, value);
                }
            }
            return true;
        }
        catch
        {
            return false;
        }

    }

    #endregion


    #region 외부 호출 함수

    public bool ReadCSVFile(string fileName, Dictionary<string, string> dataContainer)
    {
        // csv 파일 경로 생성
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        // csv 파일 데이터 읽기 및 저장
        bool result = ReadFile(path, dataContainer);

        return result;
    }

    #endregion

    public string GetConfigData(string key)
    {
        if (_configDataContainer.TryGetValue(key, out var data))
        {
            return data;
        }
        return null;
    }

}
