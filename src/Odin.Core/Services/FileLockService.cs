namespace Odin.Core.Services;

/// <summary>
/// Service for locking files to prevent modification/deletion.
/// Uses FileStream with FileShare.Read to allow reading but block writing.
/// </summary>
public class FileLockService : IDisposable
{
    private FileStream? _lockedFileStream;
    private string? _lockedFilePath;
    private bool _disposed;

    /// <summary>
    /// Gets whether a file is currently locked.
    /// </summary>
    public bool IsLocked => _lockedFileStream != null;

    /// <summary>
    /// Gets the path of the currently locked file.
    /// </summary>
    public string? LockedFilePath => _lockedFilePath;

    /// <summary>
    /// Event raised when lock state changes.
    /// </summary>
    public event Action<bool>? OnLockStateChanged;

    /// <summary>
    /// Event raised when an error occurs.
    /// </summary>
    public event Action<string>? OnError;

    /// <summary>
    /// Locks the specified file.
    /// </summary>
    /// <param name="filePath">Path to the file to lock.</param>
    /// <returns>True if lock was successful.</returns>
    public bool LockFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            OnError?.Invoke("File path is empty");
            return false;
        }

        if (!File.Exists(filePath))
        {
            OnError?.Invoke($"File not found: {filePath}");
            return false;
        }

        if (IsLocked)
        {
            UnlockFile();
        }

        try
        {
            _lockedFileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            _lockedFilePath = filePath;
            OnLockStateChanged?.Invoke(true);
            return true;
        }
        catch (IOException ex)
        {
            OnError?.Invoke($"Cannot lock file (already in use): {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            OnError?.Invoke($"Access denied: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Error locking file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Unlocks the currently locked file.
    /// </summary>
    public void UnlockFile()
    {
        if (_lockedFileStream != null)
        {
            try
            {
                _lockedFileStream.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }

            _lockedFileStream = null;
            _lockedFilePath = null;
            OnLockStateChanged?.Invoke(false);
        }
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        UnlockFile();
        GC.SuppressFinalize(this);
    }
}
