namespace ApacheMinaSSHD.NET.Wrapper.Abstractions.Models
{
    /// <summary>
    /// Provides safe session metadata for authentication, event, and file operation callbacks.
    /// </summary>
    public interface ISshSession
    {
        /// <summary>Gets the remote client address.</summary>
        string RemoteAddress { get; }
        /// <summary>Gets the unique session identifier assigned by the wrapper.</summary>
        Guid SessionId { get; }
    }
}
