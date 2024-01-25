using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Data.AppConfiguration;
using Azure.Core;

class Program
{
    static async Task Main(string[] args)
    {
        string devEndpoint = "https://dev-appconfig.azconfig.io";
        string prodEndpoint = "https://prod-appconfig.azconfig.io";

        TokenCredential credential = new DefaultAzureCredential();

        ConfigurationClientOptions options = new ConfigurationClientOptions
        {
            // Add any additional
        };

        ConfigurationClient devClient = new ConfigurationClient(new Uri(devEndpoint), credential, options);
        ConfigurationClient prodClient = new ConfigurationClient(new Uri(prodEndpoint), credential, options);

        Dictionary<string, string> devKeys = await FetchKeysAsync(devClient, "development");
        Dictionary<string, string> prodKeys = await FetchKeysAsync(prodClient, "production");

        CompareAndDisplayKeys(devKeys, prodKeys);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task<Dictionary<string, string>> FetchKeysAsync(ConfigurationClient client, string environment)
    {
        Dictionary<string, string> keys = new Dictionary<string, string>();

        await foreach (ConfigurationSetting setting in client.GetConfigurationSettingsAsync(new SettingSelector()))
        {
            // Check if the key has a label matching the environment
            if (setting.Label == environment)
            {
                keys.Add(setting.Key, setting.Value);
            }
        }

        return keys;
    }

    static void CompareAndDisplayKeys(Dictionary<string, string> devKeys, Dictionary<string, string> prodKeys)
    {
        foreach (var devKey in devKeys)
        {
            string key = devKey.Key;
            string devValue = devKey.Value;
            string prodValue;

            if (prodKeys.TryGetValue(key, out prodValue))
            {
                if (devValue == prodValue)
                {
                    Console.WriteLine($"Key: {key} - Environment: Both environments, Value: {devValue}");
                }
                else
                {
                    Console.WriteLine($"Key: {key} - Environment: Different values - Development: {devValue}, Production: {prodValue}");
                }
            }
            else
            {
                Console.WriteLine($"Key: {key} - Environment: Development only, Value: {devValue}");
            }
        }

        foreach (var prodKey in prodKeys)
        {
            string key = prodKey.Key;

            if (!devKeys.ContainsKey(key))
            {
                Console.WriteLine($"Key: {key} - Environment: Production only, Value: {prodKey.Value}");
            }
        }
    }
}
