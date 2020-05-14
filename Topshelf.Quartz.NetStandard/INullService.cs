namespace Topshelf.Quartz.NetStandard
{
    public interface INullService
    {
        void Start();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "This should be named as is")]
        void Stop();
    }
}
