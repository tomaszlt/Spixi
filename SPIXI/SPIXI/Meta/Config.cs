﻿using IXICore;
using System.IO;

namespace SPIXI.Meta
{
    public class Config
    {
        // Providing pre-defined values
        // Can be read from a file later, or read from the command line

        public static bool isTestNet = false;

        public static bool enablePushNotifications = true;

        public static string walletFile = "wallet.ixi";

        public static string spixiUserFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Spixi");

        public static int encryptionRetryPasswordAttempts = 3;   // How many allowed attempts in the LaunchRetry page before throwing the user back to Launch Page

        // Read-only values
        public static readonly string aboutUrl = "https://www.spixi.io";
        public static readonly string guideUrl = "https://www.spixi.io/guide.html";

        public static readonly string pushServiceUrl = "https://ipn.ixian.io/v1";

        public static readonly int packetDataSize = 102400; // 100 Kb per packet for file transfers
        public static readonly long packetRequestTimeout = 60; // Time in seconds to re-request packets

        public static readonly string version = "spixi-0.5.1"; // Spixi version

        // Default SPIXI settings
        public static bool defaultXamarinAnimations = false;

        // App-specific settings
        public static bool storeHistory = true;

        // Push notifications OneSignal AppID
        public static string oneSignalAppId = "44d96ce3-5d33-4e8b-997d-d1ad786b96a1";

        // Block height at which the current version of Spixi was generated
        // Useful for optimized block header sync
        // Note: Always round last block height to 1000 and subtract 1 (i.e. if last block height is 33234, the correct value is 32999)
        public static ulong bakedBlockHeight = 1262999;

        // Block checksum (paired with bakedBlockHeight) of bakedBlockHeight
        // Useful for optimized block header sync
        public static byte[] bakedBlockChecksum = Crypto.stringToHash("456357156126adfa5b20ee46169de8c68545a2570835a92898933ad39f1855d34de38ff76b08edf4a053fbe7");


        // Temporary variables for bh sync recovery
        // Note: Always round last block height to 1000 and subtract 1 (i.e. if last block height is 33234, the correct value is 32999)
        public static ulong bakedRecoveryBlockHeight = 999999;
        public static byte[] bakedRecoveryBlockChecksum = Crypto.stringToHash("fa9d2126ecb78648b45e8d4bc382503c27563e7815bfaf6d32ef4b95bdd4041b7a631fc559fa007fb6af7e74");

        // VoIP settings, don't change
        public static readonly int VoIP_sampleRate = 16000;
        public static readonly int VoIP_bitRate = 16;
        public static readonly int VoIP_channels = 1;
    }
}