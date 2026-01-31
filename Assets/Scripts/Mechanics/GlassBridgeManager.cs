using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Cam Köprü Yöneticisi - Squid Game benzeri mekanik
/// 8 sıra, her sırada 3 panel, sahne her reset edildiğinde doğru bloklar rastgele değişir
/// </summary>
public class GlassBridgeManager : MonoBehaviour
{
    [Header("Köprü Ayarları")]
    [Tooltip("Toplam sıra sayısı")]
    public int rowCount = 8;
    
    [Tooltip("Her sıradaki panel sayısı")]
    public int panelsPerRow = 3;

    [Header("Panel Referansları")]
    [Tooltip("Tüm panelleri sırayla ekle (Sıra1-Panel1, Sıra1-Panel2, Sıra1-Panel3, Sıra2-Panel1...)")]
    public List<GlassPanel> allPanels = new List<GlassPanel>();

    [Header("Events")]
    public UnityEvent OnBridgeComplete;
    public UnityEvent OnPlayerFell;

    // Private
    private int[] correctPanelIndices; // Her sıra için doğru panel indexi (0, 1 veya 2)
    private int currentRow = 0;
    private bool bridgeActive = true;

    void Start()
    {
        InitializeBridge();
    }

    /// <summary>
    /// Köprüyü başlat ve doğru panelleri rastgele seç
    /// </summary>
    public void InitializeBridge()
    {
        correctPanelIndices = new int[rowCount];
        currentRow = 0;
        bridgeActive = true;

        // Her sıra için rastgele bir doğru panel seç
        for (int row = 0; row < rowCount; row++)
        {
            correctPanelIndices[row] = Random.Range(0, panelsPerRow);
        }

        // Panelleri ayarla
        SetupPanels();
    }

    void SetupPanels()
    {
        if (allPanels.Count != rowCount * panelsPerRow)
        {
            Debug.LogError($"GlassBridgeManager: Panel sayısı yanlış! Beklenen: {rowCount * panelsPerRow}, Mevcut: {allPanels.Count}");
            return;
        }

        for (int row = 0; row < rowCount; row++)
        {
            for (int panel = 0; panel < panelsPerRow; panel++)
            {
                int index = row * panelsPerRow + panel;
                GlassPanel glassPanelScript = allPanels[index];
                
                if (glassPanelScript != null)
                {
                    glassPanelScript.SetBridgeManager(this);
                    
                    // Bu panel doğru mu?
                    bool isSafe = (panel == correctPanelIndices[row]);
                    glassPanelScript.SetSafe(isSafe);
                }
            }
        }
    }

    /// <summary>
    /// Panel kırıldığında çağrılır
    /// </summary>
    public void OnPanelBroken(GlassPanel panel)
    {
        if (!bridgeActive) return;

        bridgeActive = false;
        OnPlayerFell?.Invoke();
    }

    /// <summary>
    /// Oyuncu köprüyü tamamladığında çağrılır (trigger ile kontrol edilebilir)
    /// </summary>
    public void CompleteBridge()
    {
        if (!bridgeActive) return;
        
        bridgeActive = false;
        OnBridgeComplete?.Invoke();
    }

    /// <summary>
    /// Köprüyü sıfırla (sahne reload olmadan)
    /// </summary>
    public void ResetBridge()
    {
        // Tüm panelleri resetle
        foreach (var panel in allPanels)
        {
            if (panel != null)
            {
                panel.ResetPanel();
            }
        }

        // Yeni rastgele düzen oluştur
        InitializeBridge();
    }

    /// <summary>
    /// Debug için doğru yolu göster
    /// </summary>
    public void DebugShowCorrectPath()
    {
        string path = "Doğru yol: ";
        for (int i = 0; i < rowCount; i++)
        {
            path += $"Sıra{i + 1}:Panel{correctPanelIndices[i] + 1} ";
        }
        Debug.Log(path);
    }

    void OnDrawGizmos()
    {
        // Editor'da panelleri numaralandır
        if (allPanels == null) return;

        for (int i = 0; i < allPanels.Count; i++)
        {
            if (allPanels[i] != null)
            {
                int row = i / panelsPerRow;
                int panel = i % panelsPerRow;
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    allPanels[i].transform.position + Vector3.up * 0.5f,
                    $"R{row + 1}P{panel + 1}"
                );
                #endif
            }
        }
    }
}
