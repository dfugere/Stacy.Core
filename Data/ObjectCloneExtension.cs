﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace Stacy.Core.Data
{
	public static class ObjectCloneExtension
	{
		public static T Clone<T>(this T obj) where T : new()
		{
			return ObjectExtCache<T>.Clone(obj);
		}
		static class ObjectExtCache<T> where T : new()
		{
			private static readonly Func<T, T> cloner;
			static ObjectExtCache()
			{
				var param = Expression.Parameter(typeof(T), "in");

				var bindings = typeof(T).GetProperties()
					.Where(prop => prop.CanRead && prop.CanWrite)
					.Select(
						prop => new { prop, column = Attribute.GetCustomAttribute(prop, typeof(ColumnAttribute)) as ColumnAttribute })
					.Where(p => p.column == null || !p.column.IsPrimaryKey)
					.Select(p => (MemberBinding)Expression.Bind(p.prop, Expression.Property(param, p.prop)));

				cloner = Expression.Lambda<Func<T, T>>(
					Expression.MemberInit(
						Expression.New(typeof(T)), bindings), param).Compile();
			}
			public static T Clone(T obj)
			{
				return cloner(obj);
			}
		}
	}
}
