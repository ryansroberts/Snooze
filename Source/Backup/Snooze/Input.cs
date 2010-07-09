using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Snooze
{
    public class Input<T> : IInput
    {
        string _rawValue;
        T _value;
        List<string> _errorMessages = new List<string>();
        bool _conversionError;

        public Input()
        {
            IsValid = true;
        }

        TypeConverter GetTypeConverter()
        {
            return TypeDescriptor.GetConverter(typeof(T));
        }

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _rawValue = GetTypeConverter().ConvertToString(value);
                IsValid = true;
                HasValue = true;
            }
        }

        public string RawValue
        {
            get
            {
                return _rawValue;
            }
            set
            {
                _rawValue = value;
                try
                {
                    _value = (T)GetTypeConverter().ConvertFromString(value);
                    IsValid = true;
                    HasValue = true;
                    _conversionError = false;
                }
                catch
                {
                    _value = default(T);
                    IsValid = false;
                    _conversionError = true;
                }
            }
        }

        public bool HasValue { get; protected set; }

        public virtual bool IsValid { get; protected set; }

        public virtual IEnumerable<string> ErrorMessages
        {
            get
            {
                if (_conversionError)
                {
                    return Enumerable.Concat(new[] { "Invalid data." }, _errorMessages);
                }
                else
                {
                    return _errorMessages;
                }
            }
        }

        public void AddErrorMessage(string message)
        {
            _errorMessages.Add(message);
        }

        public override string ToString()
        {
            if (HasValue)
            {
                return Value.ToString();
            }
            else
            {
                return default(T).ToString();
            }
        }

        public static implicit operator T(Input<T> input)
        {
            if (input.IsValid)
            {
                return input.Value;
            }
            else
            {
                throw new InvalidOperationException("The input is not valid so cannot extract the value.");
            }
        }

        public static implicit operator Input<T>(T value)
        {
            return new Input<T> { Value = value };
        }
    }
}
