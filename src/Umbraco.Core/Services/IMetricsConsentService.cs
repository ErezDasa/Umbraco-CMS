﻿using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface IMetricsConsentService
    {
        ConsentLevel GetConsentLevel();

        void SetConsentLevel(ConsentLevel consentLevel);
    }
}
