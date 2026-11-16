using UnityEngine;
using TMPro;

public class HeightManager : MonoBehaviour
{
    // プレイヤー
    public Transform player;

    // 高さ表示用テキスト
    public TextMeshProUGUI heightText;

    // 最高記録表示用テキスト
    public TextMeshProUGUI maxHeightText;

    // スタート位置のY座標
    private float startY;

    // 最高到達高度
    private float maxHeight = 0f;

    void Start()
    {
        // ゲーム開始時のY座標を記録
        startY = player.position.y;
    }

    void Update()
    {
        // 現在の高さ
        float currentHeight = player.position.y - startY;

        // マイナスにならないようにする
        if (currentHeight < 0)
        {
            currentHeight = 0;
        }

        // 最高記録更新
        if (currentHeight > maxHeight)
        {
            maxHeight = currentHeight;
        }

        // 表示更新
        heightText.text = "高さ : " + currentHeight.ToString("F1") + " m";

        maxHeightText.text = "最高記録 : " + maxHeight.ToString("F1") + " m";
    }
}