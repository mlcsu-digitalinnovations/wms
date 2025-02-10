using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace WmsHub.Business.Tests.Services
{
  [Obsolete("Use InMemory testing and not mocking DbContext")]
  public static class DbContextMock
  {
    public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList)
      where T : class
    {
      var queryable = sourceList.AsQueryable();
      var dbSet = new Mock<DbSet<T>>();
      dbSet.As<IQueryable<T>>().Setup(m => m.Provider)
        .Returns(queryable.Provider);
      dbSet.As<IQueryable<T>>().Setup(m => m.Expression)
        .Returns(queryable.Expression);
      dbSet.As<IQueryable<T>>().Setup(m => m.ElementType)
        .Returns(queryable.ElementType);
      dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator())
        .Returns(() => queryable.GetEnumerator());
      dbSet.As<IAsyncEnumerable<T>>()
        .Setup(m => m.GetAsyncEnumerator(CancellationToken.None))
        .Returns(new AsyncEnumerator<T>(sourceList.GetEnumerator()));
      dbSet.Setup(d => d.Add(It.IsAny<T>()))
        .Callback<T>((s) => sourceList.Add(s));
      return dbSet.Object;
    }

    internal class AsyncEnumerable<T> :
      EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
    public AsyncEnumerable(Expression expression) : base(expression) { }

      public IAsyncEnumerator<T> GetEnumerator() =>
        new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

      public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken()) =>
        new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    internal class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
      private readonly IEnumerator<T> enumerator;

      public AsyncEnumerator(IEnumerator<T> enumerator)
      {
        this.enumerator = enumerator;
      }

      public T Current => this.enumerator.Current;

      public ValueTask DisposeAsync()
      {
        return new ValueTask(Task.Run(() => this.enumerator.Dispose()));
      }

      public ValueTask<bool> MoveNextAsync()
      {
        return new ValueTask<bool>(this.enumerator.MoveNext());
      }
    }
  }
}