﻿using Abp.Authorization;
using Abp.Localization;
using Abp.Notifications;
using Yootek.Authorization;

namespace Yootek.Notifications
{
    public class AppNotificationProvider : NotificationProvider
    {
        public override void SetNotifications(INotificationDefinitionContext context)
        {
            context.Manager.Add(
                new NotificationDefinition(
                    AppNotificationNames.NewUserRegistered,
                    displayName: L("NewUserRegisteredNotificationDefinition"),
                    permissionDependency: new SimplePermissionDependency(IOCPermissionNames.Pages_Administration_Users)
                    )
                );

            context.Manager.Add(
                new NotificationDefinition(
                    AppNotificationNames.NewTenantRegistered,
                    displayName: L("NewTenantRegisteredNotificationDefinition"),
                    permissionDependency: new SimplePermissionDependency(IOCPermissionNames.Pages_Tenants)
                    )
                );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, YootekConsts.LocalizationSourceName);
        }
    }
}
