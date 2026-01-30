using UnityEngine;

/// <summary>
/// Maske 1 - Placeholder özellikler
/// </summary>
public class Mask1 : MaskBase
{
    public override string MaskName => "Maske 1";
    public override int MaskIndex => 0;

    [Header("Maske 1 Ayarları")]
    [Tooltip("Bu maskenin özel efekti için placeholder")]
    public string abilityDescription = "Özel Yetenek 1 - TODO: Implement";

    protected override void OnActivate()
    {
        // TODO: Maske 1 özel yeteneğini buraya ekle
        // Örnek: Gece görüşü, hız artışı, vb.
        Debug.Log($"[Maske 1] {abilityDescription} - AKTİF");
    }

    protected override void OnDeactivate()
    {
        // TODO: Maske 1 etkilerini kaldır
        Debug.Log($"[Maske 1] {abilityDescription} - KAPALI");
    }

    void Update()
    {
        if (IsActive)
        {
            // TODO: Sürekli efektler buraya
            // Örnek: Her frame'de gösterilen görsel efektler
        }
    }
}
