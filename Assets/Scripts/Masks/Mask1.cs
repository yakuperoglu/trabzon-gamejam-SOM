using UnityEngine;
using System;

/// <summary>
/// Maske 1 - Gizli Nesneleri Görme Yeteneği
/// Aktifken normalde görünmez nesneleri gösterir
/// </summary>
public class Mask1 : MaskBase
{
    public override string MaskName => "Gizli Görüş Maskesi";
    public override int MaskIndex => 0;

    // Static event - tüm RevealableObject'ler bunu dinler
    public static event Action<bool> OnRevealStateChanged;
    public static bool IsRevealActive { get; private set; }

    [Header("Görsel Efektler")]
    [Tooltip("Maske aktifken ekrana hafif renk efekti")]
    public bool useScreenTint = true;
    
    [Tooltip("Ekran renk efekti")]
    public Color screenTintColor = new Color(0.3f, 0.5f, 1f, 0.1f);

    protected override void OnActivate()
    {
        IsRevealActive = true;
        
        // Tüm gizli objelere haber ver
        OnRevealStateChanged?.Invoke(true);
    }

    protected override void OnDeactivate()
    {
        IsRevealActive = false;
        
        // Tüm gizli objelere haber ver
        OnRevealStateChanged?.Invoke(false);
    }

    void OnDestroy()
    {
        // Temizlik
        if (IsRevealActive)
        {
            IsRevealActive = false;
            OnRevealStateChanged?.Invoke(false);
        }
    }
}

