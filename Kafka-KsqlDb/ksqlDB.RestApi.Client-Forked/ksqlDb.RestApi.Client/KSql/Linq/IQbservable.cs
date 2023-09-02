﻿using System.Linq.Expressions;

namespace ksqlDB.RestApi.Client.KSql.Linq;

public interface IQbservable
{
  /// <summary>
  /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of IQbservable is executed.
  /// </summary>
  Type ElementType { get; }

  /// <summary>
  /// Gets the expression tree that is associated with the instance of IQbservable.
  /// </summary>
  Expression Expression { get; }

  /// <summary>
  /// Gets the query provider that is associated with this data source.
  /// </summary>
  IKSqlQbservableProvider Provider { get; }
}

public interface IQbservable<out T> : IQbservable
{
  IDisposable Subscribe(IObserver<T> observer);

  Task<Subscription> SubscribeAsync(IObserver<T> observer, CancellationToken cancellationToken = default);
}