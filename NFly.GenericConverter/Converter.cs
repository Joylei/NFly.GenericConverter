/**
 * Copyright (c) 2015, wowin (leingliu@126.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NFly.GenericConverter
{
    /// <summary>
    /// convert type from one to another;
    /// user registered converter has the highest priority.
    /// you can register a custom converter by:
    ///     GenericConverter.Register(typeof(YourType), yourConverter)
    /// 
    /// how to use:
    ///     var time = DateTime.Now;
    ///     var text = Converter.Convert<string>(time);
    /// 
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// user registered converters
        /// </summary>
        private static readonly Dictionary<Type, TypeConverter> CustomConverters =
            new Dictionary<Type, TypeConverter>();

        /// <summary>
        /// register custom converters to change type;
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="converter"></param>
        public static void Register(Type targetType, TypeConverter converter)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            CustomConverters[targetType] = converter;
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TTarget Convert<TTarget>(object source)
        {
            return Convert<TTarget>(source, null);
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TryConvert<TTarget>(object source, out TTarget target)
        {
            return TryConvert(source, null, out target);
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="provider"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TryConvert<TTarget>(object source, IFormatProvider provider, out TTarget target)
        {
            try
            {
                target = Convert<TTarget>(source, provider);
                return true;
            }
            catch (Exception ex)
            {
                //ignore errors
                Debug.Write(ex, "trace");
            }
            target = default(TTarget);
            return false;
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static TTarget Convert<TTarget>(object source, IFormatProvider provider)
        {
            return (TTarget)Convert(source, typeof(TTarget), provider);
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object Convert(object source, Type targetType)
        {
            return Convert(source, targetType, null);
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TryConvert(object source, Type targetType, out object target)
        {
            return TryConvert(source, targetType, null, out target);
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="provider"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TryConvert(object source, Type targetType, IFormatProvider provider, out object target)
        {
            try
            {
                target = Convert(source, targetType, provider);
                return true;
            }
            catch (Exception ex)
            {
                //ignore errors
                Debug.Write(ex, "trace");
            }

            target = null;
            return false;
        }

        /// <summary>
        /// convert object to the specified type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static object Convert(object source, Type targetType, IFormatProvider provider)
        {
            //validate paramters
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (targetType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("targetType is a generic type definition:" + targetType.FullName, "targetType");
            }

            //check nulls
            if (source == null || System.Convert.IsDBNull(source))
            {
                return CheckNulls(source, targetType);
            }

            var typeSource = source.GetType();

            if (targetType.IsAssignableFrom(typeSource))
            {
                return CheckNulls(source, targetType);
            }

            //apply different converters
            object target = CastByConverter(source, targetType, typeSource)//check converters
                ?? ChangeType(source, targetType, provider)
                ?? CastByOperator(source, targetType, typeSource)//check operators
                ?? ParseEnum(source, targetType);

            if (target != null)
            {
                return target;
            }

            //failed after try every ways
            throw new InvalidCastException(string.Format("cannot cast from {0} to {1}",
                typeSource.FullName, targetType.FullName));

        }

        /// <summary>
        /// try to parse the object as enum type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static object ParseEnum(object source, Type targetType)
        {
            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, source.ToString(), true);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex, "trace");
                }
            }
            return null;
        }

        /// <summary>
        /// try to change type by System.Convert.ChangeType
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static object ChangeType(object source, Type targetType, IFormatProvider provider)
        {
            try
            {
                return System.Convert.ChangeType(source, targetType, provider);
            }
            catch (Exception ex)
            {
                Debug.Write(ex, "trace");
            }
            return null;
        }


        /// <summary>
        /// try to convert object to targetType by the implicit or explicit operator of sourceType and sourceType
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="typeSource"></param>
        /// <returns></returns>
        private static object CastByOperator(object source, Type targetType, Type typeSource)
        {
            //TODO: improve the performance
            const string implicitMethod = "op_Implicit";
            const string explicitMethod = "op_Explicit";

            var parameters = new object[] { source };
            var method = GetOperator(targetType, implicitMethod, targetType, typeSource);
            if (method == null)
            {
                method = GetOperator(targetType, explicitMethod, targetType, typeSource);
            }
            if (method == null)
            {
                method = GetOperator(typeSource, implicitMethod, targetType, typeSource);
            }
            if (method == null)
            {
                method = GetOperator(typeSource, explicitMethod, targetType, typeSource);
            }

            if (method != null)
            {
                try
                {
                    return method.Invoke(null, parameters);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex, "trace");
                }

            }
            return null;
        }


        private static MethodInfo GetOperator(Type fromType, string opName, Type targetType, Type typeSource)
        {
            var method = fromType.GetMethod(opName, new Type[] { typeSource });
            if (method != null && targetType.IsAssignableFrom(method.ReturnType))
            {
                return method;
            }
            return null;
        }

        /// <summary>
        /// try to convert object to targetType by custom converters and default system converters
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="typeSource"></param>
        /// <returns></returns>
        private static object CastByConverter(object source, Type targetType, Type typeSource)
        {
            object target = null;
            TypeConverter converter = null;
            //convert by custom converter of target type
            if (CustomConverters.TryGetValue(targetType, out converter))
            {
                try
                {
                    target = converter.ConvertFrom(source);
                    if (target != null)
                    {
                        return target;
                    }
                }
                catch (Exception)
                {
                    //ignore errors
                }

            }

            //convert by custom converter of source type
            if (CustomConverters.TryGetValue(typeSource, out converter))
            {
                try
                {
                    target = converter.ConvertTo(null, null, source, targetType);
                    if (target != null)
                    {
                        return target;
                    }
                }
                catch (Exception)
                {
                    //ignore errors
                }
            }

            //convert by converter of target type
            converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeSource))
            {
                try
                {
                    target = converter.ConvertFrom(source);
                    if (target != null)
                    {
                        return target;
                    }
                }
                catch (Exception)
                {
                    //ignore errors
                }
            }

            //convert by converter of source type
            converter = TypeDescriptor.GetConverter(typeSource);
            if (converter.CanConvertTo(targetType))
            {
                try
                {
                    target = converter.ConvertTo(null, null, source, targetType);
                    if (target != null)
                    {
                        return target;
                    }
                }
                catch (Exception)
                {
                    //ignore errors
                }
            }
            return null;
        }

        /// <summary>
        /// throws if try convert null to value type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="typeTarget"></param>
        /// <returns></returns>
        private static object CheckNulls(object source, Type typeTarget)
        {
            if (source == null && !IsNullable(typeTarget) && typeTarget.IsValueType)
            {
                throw new InvalidCastException("cannot cast null to value type " + typeTarget.FullName);
            }
            return source;
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();
        }

    }
}
