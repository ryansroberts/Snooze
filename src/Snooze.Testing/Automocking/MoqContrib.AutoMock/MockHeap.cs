using System;
using System.Collections.Generic;
using Moq;

namespace Snooze.AutoMock.Castle.MoqContrib.AutoMock
{
    /// <summary>
    /// Like a container, but keeps track of mocks instead of objects.
    /// </summary>
    internal class MockHeap : MoqContrib.AutoMock.IMockHeap
    {
        private class Data
        {
            public Mock Mock;
            public Type Type;
        }

        private Dictionary<string, Data> _mocks = new Dictionary<string, Data>();
        #region IMockHeap Members

        public virtual Mock this[Type type]
        {
            get
            {
                return this[type, type.FullName];
            }
            set
            {
                this[type, type.FullName] = value;
            }
        }

        public virtual Mock this[Type type, string key]
        {
            get
            {
                try
                {
                    var ret = _mocks[key];
                    if (ret.Type != type)
                        throw new ArgumentException(string.Format("'{0}' was not of type {1}", key, type));
                    return ret.Mock;
                }
                catch (KeyNotFoundException)
                {
                    throw new IndexOutOfRangeException();
                }
            }
            set
            {
                _mocks[key] = new Data { Mock = value, Type = type };
            }
        }

        public virtual bool ContainsType(Type type)
        {
            return ContainsType(type, type.FullName);
        }

        public virtual bool ContainsType(Type type, string key)
        {
            return _mocks.ContainsKey(key);
        }

        #endregion
    }
}
