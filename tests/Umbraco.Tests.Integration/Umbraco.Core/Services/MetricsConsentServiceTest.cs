﻿using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MetricsConsentServiceTest : UmbracoIntegrationTest
    {
        private IMetricsConsentService MetricsConsentService => GetRequiredService<IMetricsConsentService>();

        private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

        [Test]
        [TestCase(ConsentLevel.Minimal)]
        [TestCase(ConsentLevel.Basic)]
        [TestCase(ConsentLevel.Detailed)]
        public void Can_Store_Consent(ConsentLevel level)
        {
            MetricsConsentService.SetConsentLevel(level);

            var actual = MetricsConsentService.GetConsentLevel();
            Assert.IsNotNull(actual);
            Assert.AreEqual(level, actual);
        }

        [Test]
        public void Enum_Stored_as_string()
        {
            MetricsConsentService.SetConsentLevel(ConsentLevel.Detailed);

            var stringValue = KeyValueService.GetValue(Cms.Core.Services.MetricsConsentService.Key);

            Assert.AreEqual("Detailed", stringValue);
        }
    }
}
