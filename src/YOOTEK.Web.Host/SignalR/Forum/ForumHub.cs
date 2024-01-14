using Abp;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Dependency;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using Castle.Windsor;
using Yootek.Chat;
using Yootek.GroupChats;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Chat
{
    public class ForumHub : OnlineClientHubBase, ITransientDependency
    {


        private readonly ILocalizationManager _localizationManager;
        private readonly IWindsorContainer _windsorContainer;
        private readonly ITenantCache _tenantCache;
        private bool _isCallByRelease;

      
        public ForumHub(
            ILocalizationManager localizationManager,
            IOnlineClientManager onlineClientManager,
             ITenantCache tenantCache,
             IWindsorContainer windsorContainer,
            IOnlineClientInfoProvider clientInfoProvider) : base(onlineClientManager, clientInfoProvider)
        {
            _localizationManager = localizationManager;
            _windsorContainer = windsorContainer;
            _tenantCache = tenantCache;
            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }



    }
}
