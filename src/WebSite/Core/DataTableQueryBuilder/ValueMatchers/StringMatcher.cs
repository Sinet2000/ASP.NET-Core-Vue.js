﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace DataTableQueryBuilder.ValueMatchers
{
    public class StringMatcher : ValueMatcher
    {
        protected ValueMatchMethod MatchMethod { get; set; }

        public StringMatcher(Expression property, string valueToMatch, ValueMatchMethod matchMethod) : base(property, valueToMatch)
        {
            MatchMethod = matchMethod;
        }

        public override Expression Match()
        {
            if (MatchMethod == ValueMatchMethod.StringSQLServerContainsPhrase || MatchMethod == ValueMatchMethod.StringSQLServerFreeText)
                return GenerateSQLServerFullTextSearchMatchExp();

            return GenerateStringMatchExp();
        }

        private Expression GenerateStringMatchExp()
        {
            var methodName = MatchMethod == ValueMatchMethod.StringStartsWith ? "StartsWith" : (MatchMethod == ValueMatchMethod.StringEndsWith ? "EndsWith" : "Contains");

            var propertyAsStringExp = Property.Type == typeof(string) ? (Expression)Expression.Coalesce(Property, Expression.Constant(string.Empty)) : Expression.Call(Property, Property.Type.GetMethod("ToString", Type.EmptyTypes));

            var propertyToLowerExp = Expression.Call(propertyAsStringExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            var propertyMatchExp = Expression.Call(propertyToLowerExp, typeof(string).GetMethod(methodName, new[] { typeof(string) }), Expression.Constant(ValueToMatch.ToLower()));

            return propertyMatchExp;
        }

        private Expression GenerateSQLServerFullTextSearchMatchExp()
        {
            var sqlServerMethodName = MatchMethod == ValueMatchMethod.StringSQLServerContainsPhrase ? "Contains" : "FreeText";
            var valueToMatch = MatchMethod == ValueMatchMethod.StringSQLServerContainsPhrase ? $"\"{ValueToMatch}*\"" : ValueToMatch;

            var methodInfo = typeof(SqlServerDbFunctionsExtensions).GetMethod(sqlServerMethodName, BindingFlags.Static | BindingFlags.Public, null, new[] { EF.Functions.GetType(), typeof(string), typeof(string) }, null);

            return Expression.Call(methodInfo, Expression.Constant(EF.Functions), Property, Expression.Constant(valueToMatch));
        }
    }
}