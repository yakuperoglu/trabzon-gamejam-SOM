using UnityEngine;

/// <summary>
/// Maske 3 - Placeholder özellikler
/// </summary>
public class Mask3 : MaskBase
{
    public override string MaskName => "Maske 3";
    public override int MaskIndex => 2;

    [Header("Maske 3 Ayarları")]
    [Tooltip("Bu maskenin özel efekti için placeholder")]
    public string abilityDescription = "Özel Yetenek 3 - TODO: Implement";

    protected override void OnActivate()
    {
        // TODO: Maske 3 özel yeteneğini buraya ekle
        // Örnek: Görünmezlik, vb.
        Debug.Log($"[Maske 3] {abilityDescription} - AKTİF");
    }

    protected override void OnDeactivate()
    {
        // TODO: Maske 3 etkilerini kaldır
        Debug.Log($"[Maske 3] {abilityDescription} - KAPALI");
    }

    void Update()
    {
        if (IsActive)
        {
            // TODO: Sürekli efektler buraya
        }
    }
}
