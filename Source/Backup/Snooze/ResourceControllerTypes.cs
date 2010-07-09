using System;
using System.Collections.Generic;
using System.Linq;

namespace Snooze
{
    static class ResourceControllerTypes
    {
        static Dictionary<Type, Type> _controllerTypes = new Dictionary<Type, Type>();

        public static Type FindTypeForUrl<TUrl>() where TUrl : Url
        {
            if (_controllerTypes.ContainsKey(typeof(TUrl)))
            {
                return _controllerTypes[typeof(TUrl)];
            }
            else
            {
                lock (_controllerTypes)
                {
                    if (_controllerTypes.ContainsKey(typeof(TUrl)))
                    {
                        return _controllerTypes[typeof(TUrl)];
                    }

                    var urlControllerTypes =
                        from type in typeof(TUrl).Assembly.GetTypes()
                        where typeof(ResourceController).IsAssignableFrom(type) && !type.IsAbstract
                        from method in type.GetMethods()
                        let parameters = method.GetParameters()
                        where parameters.Length > 0 && typeof(Url).IsAssignableFrom(parameters[0].ParameterType)
                        select new { urlType = parameters[0].ParameterType, controllerType = type };

                    var oneControllerPerUrlType =
                        from t in urlControllerTypes
                        group t by t.urlType into urlType
                        where !urlType.Key.IsAbstract
                        select new { urlType = urlType.Key, controllerType = urlType.First().controllerType };

                    foreach (var item in oneControllerPerUrlType)
                    {
                        // NICE / SH - Wrap following line to catch "Key already exists" exception
                        // This really means "Cannot find publically accessible Action defined in a Controller.
                        // Due to a logic error in this code it manifests as an ArgumentException
                        
                        // Original code
                        //_controllerTypes.Add(item.urlType, item.controllerType);
                        try
                        {
                            _controllerTypes.Add(item.urlType, item.controllerType);
                        }
                        catch (Exception exception)
                        {
                            if (exception.GetType() == typeof(ArgumentException))
                                throw new ApplicationException("Cannot find action for Route - ensure configured routes have publically accessible Actions defined");
                            throw;
                        }
                        // NICE / SH
                    }
                }

                if (!_controllerTypes.ContainsKey(typeof(TUrl)))
                {
                    _controllerTypes[typeof(TUrl)] = null;
                }

                return _controllerTypes[typeof(TUrl)];
            }
        }

    }

}
