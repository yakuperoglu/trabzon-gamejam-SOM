using UnityEngine;

/// <summary>
/// Maske Base Class - Tüm maskeler bu sınıftan türetilir
/// </summary>
public abstract class MaskBase : MonoBehaviour
{
    /// <summary>
    /// Maskenin görünen adı
    /// </summary>
    public abstract string MaskName { get; }
    
    /// <summary>
    /// Maske index'i (0, 1, 2)
    /// </summary>
    public abstract int MaskIndex { get; }
    
    /// <summary>
    /// Maske aktif mi?
    /// </summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Maskeyi aktifleştir
    /// </summary>
    public virtual void Activate()
    {
        IsActive = true;
        Debug.Log($"{MaskName} aktifleştirildi!");
        OnActivate();
    }

    /// <summary>
    /// Maskeyi deaktif et
    /// </summary>
    public virtual void Deactivate()
    {
        IsActive = false;
        Debug.Log($"{MaskName} deaktif edildi!");
        OnDeactivate();
    }

    /// <summary>
    /// Alt sınıflar tarafından override edilecek - Aktivasyon efektleri
    /// </summary>
    protected abstract void OnActivate();

    /// <summary>
    /// Alt sınıflar tarafından override edilecek - Deaktivasyon efektleri
    /// </summary>
    protected abstract void OnDeactivate();
}
