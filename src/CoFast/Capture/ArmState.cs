namespace CoFast.Capture;

/// <summary>
/// PrtScn'e basıldıktan sonra belirli bir süre "kurulu" kalan durum.
/// Sadece kurulu durumdayken panoya gelen görüntüler işlenir.
/// </summary>
internal sealed class ArmState
{
    private DateTime? _armedUntilUtc;

    public ArmState(TimeSpan timeout)
    {
        Timeout = timeout;
    }

    public TimeSpan Timeout { get; set; }

    public void Arm(DateTime nowUtc) => _armedUntilUtc = nowUtc + Timeout;

    public void Disarm() => _armedUntilUtc = null;

    public bool IsArmed(DateTime nowUtc)
    {
        if (_armedUntilUtc is null)
        {
            return false;
        }

        if (nowUtc > _armedUntilUtc.Value)
        {
            _armedUntilUtc = null;
            return false;
        }

        return true;
    }
}
