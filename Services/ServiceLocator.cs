using System;
using System.Collections.Generic;
using Code.Custom;

namespace Code.Services
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, Func<object>> ServiceRegistry = new Dictionary<Type, Func<object>>();

        public static void InitialiseTesting()
        {
            InitialiseServices(true);
        }

        public static void Initialise()
        {
            InitialiseServices(false);
        }

        private static void InitialiseServices(bool testing)
        {
            if (testing)
            {
                Register<IMaterialService>(() => new TestMaterialService());
            }
            else
            {
                Register<IMaterialService>(() => new SqlMaterialService());
            }

            Register<MaterialRecordUpdater>(() => new MaterialRecordUpdater( /*IMaterialService*/));
        }

        public static void Register<T>(Func<T> serviceRegistryFunc)
        {
            ServiceRegistry[typeof (T)] = () => serviceRegistryFunc();
        }

        public static T GetService<T>()
        {
            Func<object> serviceFunc;
            if (ServiceRegistry.TryGetValue(typeof (T), out serviceFunc))
            {
                return (T) serviceFunc();
            }
            throw new ApplicationException("Unable to find service: " + typeof (T).FullName);
        }
    }
}