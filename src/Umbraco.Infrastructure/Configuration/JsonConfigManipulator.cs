﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Configuration
{
    public class JsonConfigManipulator : IConfigManipulator
    {
        private readonly IConfiguration _configuration;

        public JsonConfigManipulator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string UmbracoConnectionPath { get; } = $"ConnectionStrings:{ Constants.System.UmbracoConnectionName}";
        public void RemoveConnectionString()
        {
            var provider = GetJsonConfigurationProvider(UmbracoConnectionPath);

            var json = GetJson(provider);

            RemoveJsonKey(json, UmbracoConnectionPath);

            SaveJson(provider, json);
        }

        public void SaveConnectionString(string connectionString, string providerName)
        {
            var provider = GetJsonConfigurationProvider();

            var json = GetJson(provider);

            var item = GetConnectionItem(connectionString, providerName);

            json.Merge(item, new JsonMergeSettings());

            SaveJson(provider, json);
        }


        private JToken GetConnectionItem(string connectionString, string providerName)
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartObject();
            writer.WritePropertyName("ConnectionStrings");
            writer.WriteStartObject();
            writer.WritePropertyName(Constants.System.UmbracoConnectionName);
            writer.WriteValue(connectionString);
            writer.WriteEndObject();
            writer.WriteEndObject();

            return writer.Token;
        }

        private static void RemoveJsonKey(JObject json, string key)
        {
            JToken token = json;
            foreach (var propertyName in key.Split(new[] { ':' }))
            {
                token = CaseSelectPropertyValues(token, propertyName);
            }

            token?.Parent?.Remove();
        }

        private static void SaveJson(JsonConfigurationProvider provider, JObject json)
        {
            if (provider.Source.FileProvider is PhysicalFileProvider physicalFileProvider)
            {
                var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path);

                using (var sw = new StreamWriter(jsonFilePath, false))
                using (var jsonTextWriter = new JsonTextWriter(sw)
                {
                    Formatting = Formatting.Indented,
                })
                {
                    json?.WriteTo(jsonTextWriter);
                }
            }

        }

        private static JObject GetJson(JsonConfigurationProvider provider)
        {
            if (provider.Source.FileProvider is PhysicalFileProvider physicalFileProvider)
            {
                var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path);

                var serializer = new JsonSerializer();
                using (var sr = new StreamReader(jsonFilePath))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return serializer.Deserialize<JObject>(jsonTextReader);
                }
            }

            return null;
        }




        private JsonConfigurationProvider GetJsonConfigurationProvider(string requiredKey = null)
        {
            if (_configuration is IConfigurationRoot configurationRoot)
            {
                foreach (var provider in configurationRoot.Providers)
                {
                    if(provider is JsonConfigurationProvider jsonConfigurationProvider)
                    {
                        if (requiredKey is null || provider.TryGet(requiredKey, out _))
                        {
                            return jsonConfigurationProvider;
                        }
                    }
                }
            }
            throw new InvalidOperationException("Could not find a writable json config source");
        }

        /// <summary>
        /// Returns the property value when case insensative
        /// </summary>
        /// <remarks>
        /// This method is required because keys are case insensative in IConfiguration.
        /// JObject[..] do not support case insensative and JObject.Property(...) do not return a new JObject.
        /// </remarks>
        private static JToken CaseSelectPropertyValues(JToken token, string name)
        {
            if (token is JObject obj)
            {

                foreach (var property in obj.Properties())
                {
                    if (name is null)
                        return property.Value;
                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                        return property.Value;
                }
            }
            return null;
        }

    }
}
