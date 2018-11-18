using System.Threading;

#pragma warning disable IDE1006 // Naming Styles
// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedMember.Global
public static class awooo
#pragma warning restore IDE1006 // Naming Styles
{
    public static bool IsRunning { get; set; }
    private static SynchronizationContext _context;

    public static SynchronizationContext Context
    {
        get { return _context; }
        set
        {
            if (_context != null) return;
            _context = value;
        }
    }

    public static bool IsCellNumber(this string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.StartsWith("0"))
        {
            if (s.Length != 11)
                return false;
            var n = 0L;
            if (!long.TryParse(s, out n))
                return false;
            if (n.ToString().Length != 10)
                return false;
            return true;
        }
        else
        {
            var n = (s.StartsWith("+")) ? s.Substring(1) : s;
            var nn = 0L;
            if (!long.TryParse(n, out nn))
                return false;
            if (nn.ToString().Length != n.Length)
                return false;
            return true;
        }
        //return false;
    }
}
