namespace ProberInterfaces
{
    using System.Threading;

    public delegate void CancellationDelegate(object sender, CancellationTokenSource cs);
}
