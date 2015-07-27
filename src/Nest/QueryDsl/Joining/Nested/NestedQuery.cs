﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nest.Resolvers.Converters;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Newtonsoft.Json.Converters;

namespace Nest
{
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	[JsonConverter(typeof(ReadAsTypeConverter<NestedQueryDescriptor<object>>))]
	public interface INestedQuery : IQuery
	{
		[JsonProperty("score_mode"), JsonConverter(typeof (StringEnumConverter))]
		NestedScore? Score { get; set; }

		[JsonProperty("filter")]
		IQueryContainer Filter { get; set; }

		[JsonProperty("query")]
		IQueryContainer Query { get; set; }

		[JsonProperty("path")]
		FieldName Path { get; set; }

		[JsonProperty("inner_hits")]
		[JsonConverter(typeof(ReadAsTypeConverter<InnerHits>))]
		IInnerHits InnerHits { get; set; }

	}

	public class NestedQuery : QueryBase, INestedQuery
	{
		bool IQuery.Conditionless => IsConditionless(this);
		public NestedScore? Score { get; set; }
		public IQueryContainer Filter { get; set; }
		public IQueryContainer Query { get; set; }
		public FieldName Path { get; set; }
		public IInnerHits InnerHits { get; set; }

		protected override void WrapInContainer(IQueryContainer c) => c.Nested = this;
		internal static bool IsConditionless(INestedQuery q)
		{
			return (q.Query == null || q.Query.IsConditionless)
				&& (q.Filter == null || q.Filter.IsConditionless);
		}
	}

	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public class NestedQueryDescriptor<T> 
		: QueryDescriptorBase<NestedQueryDescriptor<T>, INestedQuery>
		, INestedQuery where T : class
	{
		private INestedQuery Self => this;
		bool IQuery.Conditionless => NestedQuery.IsConditionless(this);
		NestedScore? INestedQuery.Score { get; set; }
		IQueryContainer INestedQuery.Filter { get; set; }
		IQueryContainer INestedQuery.Query { get; set; }
		FieldName INestedQuery.Path { get; set; }
		IInnerHits INestedQuery.InnerHits { get; set; }

		public NestedQueryDescriptor<T> Filter(Func<QueryContainerDescriptor<T>, QueryContainer> filterSelector)
		{
			var q = new QueryContainerDescriptor<T>();
			Self.Filter = filterSelector(q);
			return this;
		}

		public NestedQueryDescriptor<T> Query(Func<QueryContainerDescriptor<T>, QueryContainer> querySelector)
		{
			var q = new QueryContainerDescriptor<T>();
			Self.Query = querySelector(q);
			return this;
		}

		public NestedQueryDescriptor<T> Score(NestedScore score)
		{
			Self.Score = score;
			return this;
		}
		public NestedQueryDescriptor<T> Path(string path)
		{
			Self.Path = path;
			return this;
		}

		public NestedQueryDescriptor<T> Path(Expression<Func<T, object>> objectPath)
		{
			Self.Path = objectPath;
			return this;
		}

		public NestedQueryDescriptor<T> InnerHits()
		{
			Self.InnerHits = new InnerHits();
			return this;
		}

		public NestedQueryDescriptor<T> InnerHits(Func<InnerHitsDescriptor<T>, IInnerHits> innerHitsSelector)
		{
			if (innerHitsSelector == null) return this;
			Self.InnerHits = innerHitsSelector(new InnerHitsDescriptor<T>());
			return this;
		}
	}
}