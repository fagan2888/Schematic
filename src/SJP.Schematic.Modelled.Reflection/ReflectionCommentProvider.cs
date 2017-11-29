﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    /// <summary>
    /// Retrieves and provides access to comments associated with a reflected assembly.
    /// </summary>
    public class ReflectionCommentProvider : IReflectionCommentProvider
    {
        /// <summary>
        /// Creates an instance from a reflected assembly.
        /// </summary>
        /// <param name="assembly">A reflected asseembly that may have documentation available for it.</param>
        /// <remarks>When a documentation file is not present for the assembly, all comments returned from this instance will be <c>null</c>.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <c>null</c>.</exception>
        public ReflectionCommentProvider(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var docLocation = Path.ChangeExtension(assembly.Location, "xml");
            if (File.Exists(docLocation))
            {
                using (var reader = File.OpenRead(docLocation))
                    Document = XDocument.Load(reader);
            }
            else
            {
                Document = XDocument.Parse(EmptyDoc);
            }
        }

        /// <summary>
        /// Creates an instance where the documentation file already known and provided.
        /// </summary>
        /// <param name="document">A document that described a .NET assembly's content. This should be in the same format as one created by the .NET compilers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is <c>null</c>.</exception>
        public ReflectionCommentProvider(XDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        /// <summary>
        /// A document which contains comments and documentation for an assembly.
        /// </summary>
        protected XDocument Document { get; }

        /// <summary>
        /// Retrieves comments associated with a given constructor.
        /// </summary>
        /// <param name="constructorInfo">A constructor.</param>
        /// <returns>A comment for <paramref name="constructorInfo"/> if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public string GetComment(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
                throw new ArgumentNullException(nameof(constructorInfo));

            var identifier = GetCommentIdentifier(constructorInfo);
            return GetCommentByIdentifier(identifier);
        }

        /// <summary>
        /// Retrieves comments associated with a given event.
        /// </summary>
        /// <param name="eventInfo">An event.</param>
        /// <returns>A comment for <paramref name="eventInfo"/> if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventInfo"/> is <c>null</c>.</exception>
        public string GetComment(EventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException(nameof(eventInfo));

            var identifier = GetCommentIdentifier(eventInfo);
            return GetCommentByIdentifier(identifier);
        }

        /// <summary>
        /// Retrieves comments associated with a given field.
        /// </summary>
        /// <param name="fieldInfo">A field.</param>
        /// <returns>A comment for <paramref name="fieldInfo"/> if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fieldInfo"/> is <c>null</c>.</exception>
        public string GetComment(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            var identifier = GetCommentIdentifier(fieldInfo);
            return GetCommentByIdentifier(identifier);
        }

        /// <summary>
        /// Retrieves comments associated with a given method.
        /// </summary>
        /// <param name="methodInfo">A method.</param>
        /// <returns>A comment for <paramref name="methodInfo"/> if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="methodInfo"/> is <c>null</c>.</exception>
        public string GetComment(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var identifier = GetCommentIdentifier(methodInfo);
            return GetCommentByIdentifier(identifier);
        }

        /// <summary>
        /// Retrieves comments associated with a given property.
        /// </summary>
        /// <param name="propertyInfo">A property.</param>
        /// <returns>A comment for <paramref name="propertyInfo"/> if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyInfo"/> is <c>null</c>.</exception>
        public string GetComment(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var identifier = GetCommentIdentifier(propertyInfo);
            return GetCommentByIdentifier(identifier);
        }

        /// <summary>
        /// Retrieves comments associated with a given type.
        /// </summary>
        /// <param name="type">A type.</param>
        /// <returns>A comment for <paramref name="type"/> if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public string GetComment(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var identifier = GetCommentIdentifier(type);
            return GetCommentByIdentifier(identifier);
        }

        /// <summary>
        /// Given a reflected metadata object identifier, returns the object assocatied with it from the documentation file.
        /// </summary>
        /// <param name="identifier">A reflected metadata object identifier</param>
        /// <returns>A comment if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <c>null</c>, empty or whitespace.</exception>
        protected string GetCommentByIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            var memberNode = Document
                .Descendants("member")
                .FirstOrDefault(member => member.Attribute("name")?.Value == identifier);

            if (memberNode == null)
                return null;

            var summaryNode = memberNode.Descendants("summary").FirstOrDefault();
            return summaryNode?.Value;
        }

        /// <summary>
        /// Given a reflected constructor object, returns the identifier used to retrieve a comment for it.
        /// </summary>
        /// <param name="constructorInfo">A reflected constructor object.</param>
        /// <returns>An identifier for <paramref name="constructorInfo"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="constructorInfo"/> is <c>null</c>.</exception>
        protected static string GetCommentIdentifier(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
                throw new ArgumentNullException(nameof(constructorInfo));

            var builder = new StringBuilder("M:");
            AppendFullTypeName(builder, constructorInfo.DeclaringType);
            builder.Append(".");
            AppendMethodName(builder, constructorInfo);

            return builder.ToString();
        }

        /// <summary>
        /// Given a reflected event object, returns the identifier used to retrieve a comment for it.
        /// </summary>
        /// <param name="eventInfo">A reflected event object.</param>
        /// <returns>An identifier for <paramref name="eventInfo"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventInfo"/> is <c>null</c>.</exception>
        protected static string GetCommentIdentifier(EventInfo eventInfo)
        {
            if (eventInfo == null)
                throw new ArgumentNullException(nameof(eventInfo));

            var builder = new StringBuilder("E:");
            AppendFullTypeName(builder, eventInfo.DeclaringType);
            builder.Append(".");
            builder.Append(eventInfo.Name);

            return builder.ToString();
        }

        /// <summary>
        /// Given a reflected field object, returns the identifier used to retrieve a comment for it.
        /// </summary>
        /// <param name="fieldInfo">A reflected field object.</param>
        /// <returns>An identifier for <paramref name="fieldInfo"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fieldInfo"/> is <c>null</c>.</exception>
        protected static string GetCommentIdentifier(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            var builder = new StringBuilder("F:");
            AppendFullTypeName(builder, fieldInfo.DeclaringType);
            builder.Append(".");
            builder.Append(fieldInfo.Name);

            return builder.ToString();
        }

        /// <summary>
        /// Given a reflected method object, returns the identifier used to retrieve a comment for it.
        /// </summary>
        /// <param name="methodInfo">A reflected method object.</param>
        /// <returns>An identifier for <paramref name="methodInfo"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="methodInfo"/> is <c>null</c>.</exception>
        protected static string GetCommentIdentifier(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var builder = new StringBuilder("M:");
            AppendFullTypeName(builder, methodInfo.DeclaringType);
            builder.Append(".");
            AppendMethodName(builder, methodInfo);

            if (IsCastingOperator(methodInfo))
            {
                builder.Append("~");
                AppendFullTypeName(builder, methodInfo.ReturnType, true);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Given a reflected property object, returns the identifier used to retrieve a comment for it.
        /// </summary>
        /// <param name="propertyInfo">A reflected property object.</param>
        /// <returns>An identifier for <paramref name="propertyInfo"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyInfo"/> is <c>null</c>.</exception>
        protected static string GetCommentIdentifier(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var builder = new StringBuilder("P:");
            AppendFullTypeName(builder, propertyInfo.DeclaringType);
            builder.Append(".");
            builder.Append(propertyInfo.Name);

            // handle indexers
            var propertyArgs = propertyInfo.GetIndexParameters();
            if (propertyArgs.Length > 0)
                AppendParameterNames(builder, propertyArgs);

            return builder.ToString();
        }

        /// <summary>
        /// Given a reflected object type, returns the identifier used to retrieve a comment for it.
        /// </summary>
        /// <param name="type">A reflected object type.</param>
        /// <returns>An identifier for <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        protected static string GetCommentIdentifier(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var builder = new StringBuilder("T:");
            AppendFullTypeName(builder, type, expandGenericArgs: false);

            return builder.ToString();
        }

        private static void AppendFullTypeName(StringBuilder builder, Type type, bool expandGenericArgs = false)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.Namespace != null)
            {
                builder.Append(type.Namespace);
                builder.Append(".");
            }
            AppendTypeName(builder, type, expandGenericArgs);
        }

        private static void AppendTypeName(StringBuilder builder, Type type, bool expandGenericArgs)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsNested)
            {
                AppendTypeName(builder, type.DeclaringType, false);
                builder.Append(".");
                builder.Append(type.Name);
            }
            else if (type.IsArray)
            {
                var currentType = type;
                var arrayNameStart = currentType.Name.IndexOf('[');
                var typeName = currentType.Name.Substring(0, arrayNameStart);
                builder.Append(typeName);

                var segments = new List<string>();
                while (currentType.IsArray)
                {
                    var rank = currentType.GetArrayRank();
                    if (rank > 1)
                    {
                        var arrayBuilder = new StringBuilder("[");

                        for (var i = 0; i < rank; i++)
                        {
                            arrayBuilder.Append("0:");
                            arrayBuilder.Append(",");
                        }

                        arrayBuilder.Replace(",", "]", arrayBuilder.Length - 1, 1);
                        segments.Add(arrayBuilder.ToString());
                    }
                    else
                    {
                        segments.Add("[]");
                    }

                    currentType = currentType.GetElementType();
                }

                // must reverse as arrays work out -> in
                segments.Reverse();
                builder.Append(segments.Join(string.Empty));
            }
            else if (type.IsByRef)
            {
                var typeName = type.Name.Replace("&", "@");
                builder.Append(typeName);
            }
            else
            {
                builder.Append(type.Name);
            }

            if (expandGenericArgs)
                ExpandGenericArgsIfAny(builder, type);
        }

        private static void ExpandGenericArgsIfAny(StringBuilder builder, Type type)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.GetTypeInfo().IsGenericType)
            {
                var genericArgsBuilder = new StringBuilder("{");

                var genericArgs = type.GetGenericArguments();
                foreach (var argType in genericArgs)
                {
                    AppendFullTypeName(genericArgsBuilder, argType, true);
                    genericArgsBuilder.Append(",");
                }
                genericArgsBuilder.Replace(",", "}", genericArgsBuilder.Length - 1, 1);

                builder.Replace("`" + genericArgs.Length.ToString(), genericArgsBuilder.ToString());
            }
            else if (type.IsArray)
            {
                ExpandGenericArgsIfAny(builder, type.GetElementType());
            }
        }

        private static void AppendMethodName(StringBuilder builder, MethodInfo methodInfo)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            builder.Append(methodInfo.Name);

            if (methodInfo.IsGenericMethod)
            {
                builder.Append("``");
                var genericArgs = methodInfo.GetGenericArguments();
                builder.Append(genericArgs.Length);

                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                    return;

                AppendParameterNames(builder, parameters, genericArgs);
            }
            else
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                    return;

                AppendParameterNames(builder, parameters);
            }
        }

        private static void AppendMethodName(StringBuilder builder, ConstructorInfo constructorInfo)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (constructorInfo == null)
                throw new ArgumentNullException(nameof(constructorInfo));

            builder.Append(constructorInfo.Name.Replace(".", "#"));

            var parameters = constructorInfo.GetParameters();
            if (parameters.Length == 0)
                return;

            AppendParameterNames(builder, parameters);
        }

        private static void AppendParameterNames(StringBuilder builder, IEnumerable<ParameterInfo> parameters)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            builder.Append("(");
            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                if (paramType.IsGenericParameter)
                {
                    var parentTypeArgs = paramType.DeclaringType.GetGenericArguments().ToList();
                    var paramIndex = parentTypeArgs.IndexOf(paramType);
                    builder.Append("`");
                    builder.Append(paramIndex);
                }
                else
                {
                    AppendFullTypeName(builder, paramType, true);
                }

                builder.Append(",");
            }
            builder.Replace(",", ")", builder.Length - 1, 1);
        }

        private static void AppendParameterNames(StringBuilder builder, IEnumerable<ParameterInfo> parameters, IList<Type> genericArgs)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            builder.Append("(");
            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                var paramIndex = genericArgs.IndexOf(paramType);
                if (paramIndex >= 0)
                {
                    builder.Append("``");
                    builder.Append(paramIndex);
                }
                else
                {
                    AppendFullTypeName(builder, paramType, true);
                }
                builder.Append(",");
            }
            builder.Replace(",", ")", builder.Length - 1, 1);
        }

        private static bool IsCastingOperator(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            return methodInfo.IsSpecialName
                && (methodInfo.Name.StartsWith("op_Explicit") || methodInfo.Name.StartsWith("op_Implicit"));
        }

        private const string EmptyDoc = "<doc></doc>";
    }
}