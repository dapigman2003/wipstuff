using System.Text;
using Foundation;
using Security;
using StS2Launcher.Core;

namespace StS2Launcher.Step05.iOS.Platform;

/// <summary>
/// Stores app-private generic-password records in the iOS Keychain.
///
/// The Step 05 final verification stores only harmless test sentinels and
/// deletes them before the check completes. Real Steam credentials are not
/// introduced in this step.
/// </summary>
public sealed class KeychainCredentialStore : ICredentialStore
{
    private const string ServiceName =
        "com.community.sts2launcher.step04.keychain-probe";

    public void Set(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        // For this small, single-process probe we use deterministic replace
        // semantics: remove the previous record if present, then add the new
        // value. This avoids silently accumulating duplicate generic-password
        // records and keeps the test easy to reason about.
        using (var query = CreateQuery(key))
        {
            using var existing =
                SecKeyChain.QueryAsData(query, false, out var queryStatus);

            if (queryStatus == SecStatusCode.Success)
            {
                var removeStatus = SecKeyChain.Remove(query);
                if (removeStatus != SecStatusCode.Success)
                {
                    throw new InvalidOperationException(
                        $"Keychain remove-before-set failed: " +
                        $"{Describe(removeStatus)}");
                }
            }
            else if (queryStatus != SecStatusCode.ItemNotFound)
            {
                throw new InvalidOperationException(
                    $"Keychain pre-set query failed: {Describe(queryStatus)}");
            }
        }

        using var valueData =
            NSData.FromArray(Encoding.UTF8.GetBytes(value));

        using var record = new SecRecord(SecKind.GenericPassword)
        {
            Service = ServiceName,
            Account = key,
            ValueData = valueData,

            // Device-bound and unavailable until the first unlock after boot.
            // This is a safe probe policy; final Steam credential policy will
            // be chosen later as an explicit architecture decision.
            Accessible = SecAccessible.AfterFirstUnlockThisDeviceOnly
        };

        var addStatus = SecKeyChain.Add(record);
        if (addStatus != SecStatusCode.Success)
        {
            throw new InvalidOperationException(
                $"Keychain add failed: {Describe(addStatus)}");
        }
    }

    public string? Get(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var query = CreateQuery(key);
        using var data =
            SecKeyChain.QueryAsData(query, false, out var status);

        if (status == SecStatusCode.ItemNotFound)
            return null;

        if (status != SecStatusCode.Success)
        {
            throw new InvalidOperationException(
                $"Keychain query failed: {Describe(status)}");
        }

        if (data is null)
        {
            throw new InvalidOperationException(
                "Keychain reported success but returned no data.");
        }

        return Encoding.UTF8.GetString(data.ToArray());
    }

    public bool Delete(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        using var query = CreateQuery(key);
        var status = SecKeyChain.Remove(query);

        if (status == SecStatusCode.Success)
            return true;

        if (status == SecStatusCode.ItemNotFound)
            return false;

        throw new InvalidOperationException(
            $"Keychain delete failed: {Describe(status)}");
    }

    private static SecRecord CreateQuery(string key)
    {
        return new SecRecord(SecKind.GenericPassword)
        {
            Service = ServiceName,
            Account = key
        };
    }

    private static string Describe(SecStatusCode status)
    {
        return $"{status} ({(int)status}): " +
               $"{status.GetStatusDescription() ?? "no description"}";
    }
}
