using Microsoft.Practices.Unity;
using Quang.Auth.Api.BusinessLogic;
using System;
using System.Web.Http;
using Unity.WebApi;

namespace Quang.Auth.Api
{
    public static class UnityConfig
    {
        private static readonly Lazy<IUnityContainer> container = new Lazy<IUnityContainer>((Func<IUnityContainer>)(() =>
        {
            UnityContainer unityContainer = new UnityContainer();
            UnityConfig.RegisterTypes((IUnityContainer)unityContainer);
            return (IUnityContainer)unityContainer;
        }));

        public static IUnityContainer GetConfiguredContainer()
        {
            return UnityConfig.container.Value;
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            UnityContainerExtensions.RegisterType<IUserBll, UserBll>(container);
            UnityContainerExtensions.RegisterType<IGroupBll, GroupBll>(container);
            UnityContainerExtensions.RegisterType<IPermissionBll, PermissionBll>(container);
            UnityContainerExtensions.RegisterType<ITermBll, TermBll>(container);
            UnityContainerExtensions.RegisterType<IDeviceBll, DeviceBll>(container);
            UnityContainerExtensions.RegisterType<ILoginHistoryBll, LoginHistoryBll>(container);
            UnityContainerExtensions.RegisterType<IRefreshTokenBll, RefreshTokenBll>(container);
        }
    }
}