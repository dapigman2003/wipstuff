namespace StS2Launcher.Core;

/// <summary>
/// Step 11 local integrity helpers. Steam depot manifests expose an Adler-32
/// checksum for every decompressed chunk; this lets an interrupted .part file be
/// scanned without persisting a separate chunk journal. Full files are still
/// required to pass the manifest SHA-1 before they are considered complete.
/// </summary>
public static class SteamDepotResumeValidation
{
    private const uint AdlerMod = 65521;
    private const int AdlerBlock = 5552;

    public static uint ComputeAdler32(ReadOnlySpan<byte> data)
    {
        uint a = 1;
        uint b = 0;
        var offset = 0;

        while (offset < data.Length)
        {
            var count = Math.Min(AdlerBlock, data.Length - offset);
            var end = offset + count;
            for (; offset < end; offset++)
            {
                a += data[offset];
                b += a;
            }

            a %= AdlerMod;
            b %= AdlerMod;
        }

        return a | (b << 16);
    }

    public static async Task<uint> ComputeAdler32Async(
        Stream stream,
        int length,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        uint a = 1;
        uint b = 0;
        var buffer = new byte[Math.Min(128 * 1024, Math.Max(1, length))];
        var remaining = length;

        while (remaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var wanted = Math.Min(buffer.Length, remaining);
            var read = await stream.ReadAsync(buffer.AsMemory(0, wanted), cancellationToken)
                .ConfigureAwait(false);
            if (read <= 0)
                throw new EndOfStreamException("Resume chunk ended before its manifest length.");

            var offset = 0;
            while (offset < read)
            {
                var count = Math.Min(AdlerBlock, read - offset);
                var end = offset + count;
                for (; offset < end; offset++)
                {
                    a += buffer[offset];
                    b += a;
                }

                a %= AdlerMod;
                b %= AdlerMod;
            }

            remaining -= read;
        }

        return a | (b << 16);
    }
}
