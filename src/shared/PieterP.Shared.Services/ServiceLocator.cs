using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace PieterP.Shared.Services {
    public static class ServiceLocator {
        static ServiceLocator() {
            _container = new UnityContainer();
        }

        public static TProxyType Resolve<TProxyType>() {
            if (_container == null)
                throw new ObjectDisposedException(nameof(ServiceLocator));
            return _container.Resolve<TProxyType>();
        }

        public static IUnityContainer RegisterType<TFrom, TTo>(params InjectionMember[] injectionMembers) where TTo : TFrom {
            if (_container == null)
                throw new ObjectDisposedException(nameof(ServiceLocator));
            return _container.RegisterType<TFrom, TTo>(injectionMembers);
        }

        public static IUnityContainer RegisterType<TFrom, TTo>(ITypeLifetimeManager lifetimeManager, params InjectionMember[] injectionMembers) where TTo : TFrom {
            if (_container == null)
                throw new ObjectDisposedException(nameof(ServiceLocator));
            return _container.RegisterType<TFrom, TTo>(lifetimeManager, injectionMembers);
        }

        public static IUnityContainer RegisterInstance<TInterface>(TInterface instance) {
            if (_container == null)
                throw new ObjectDisposedException(nameof(ServiceLocator));
            return _container.RegisterInstance<TInterface>(instance);
        }

        public static IUnityContainer RegisterInstance<TInterface>(TInterface instance, IInstanceLifetimeManager lifetimeManager) {
            if (_container == null)
                throw new ObjectDisposedException(nameof(ServiceLocator));
            return _container.RegisterInstance<TInterface>(instance, lifetimeManager);
        }

        private static IUnityContainer _container;
    }
}
