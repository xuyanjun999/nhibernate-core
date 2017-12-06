﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Context;
using NHibernate.Dialect.Function;
using NHibernate.Engine;
using NHibernate.Engine.Query;
using NHibernate.Engine.Query.Sql;
using NHibernate.Event;
using NHibernate.Exceptions;
using NHibernate.Hql;
using NHibernate.Id;
using NHibernate.Mapping;
using NHibernate.Metadata;
using NHibernate.Persister;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Stat;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transaction;
using NHibernate.Type;
using NHibernate.Util;
using Environment = NHibernate.Cfg.Environment;
using HibernateDialect = NHibernate.Dialect.Dialect;
using IQueryable = NHibernate.Persister.Entity.IQueryable;

namespace NHibernate.Impl
{
	using System.Threading.Tasks;
	using System.Threading;
	public sealed partial class SessionFactoryImpl : ISessionFactoryImplementor, IObjectReference
	{
		#region ISessionFactoryImplementor Members

		/// <summary>
		/// Closes the session factory, releasing all held resources.
		/// <list>
		/// <item>cleans up used cache regions and "stops" the cache provider.</item>
		/// <item>close the ADO.NET connection</item>
		/// </list>
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public async Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			log.Info("Closing");

			isClosed = true;

			foreach (IEntityPersister p in entityPersisters.Values)
			{
				if (p.HasCache)
				{
					p.Cache.Destroy();
				}
			}

			foreach (ICollectionPersister p in collectionPersisters.Values)
			{
				if (p.HasCache)
				{
					p.Cache.Destroy();
				}
			}

			if (settings.IsQueryCacheEnabled)
			{
				queryCache.Destroy();

				foreach (IQueryCache cache in queryCaches.Values)
				{
					cache.Destroy();
				}

				updateTimestampsCache.Destroy();
			}

			settings.CacheProvider.Stop();

			try
			{
				settings.ConnectionProvider.Dispose();
			}
			finally
			{
				SessionFactoryObjectFactory.RemoveInstance(uuid, name, properties);
			}

			if (settings.IsAutoDropSchema)
			{
				await (schemaExport.DropAsync(false, true, cancellationToken)).ConfigureAwait(false);
			}

			eventListeners.DestroyListeners();
		}

		public Task EvictAsync(System.Type persistentClass, object id, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				IEntityPersister p = GetEntityPersister(persistentClass.FullName);
				if (p.HasCache)
				{
					if (log.IsDebugEnabled())
					{
						log.Debug("evicting second-level cache: {0}", MessageHelper.InfoString(p, id));
					}
					CacheKey ck = GenerateCacheKeyForEvict(id, p.IdentifierType, p.RootEntityName);
					return p.Cache.RemoveAsync(ck, cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task EvictAsync(System.Type persistentClass, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				IEntityPersister p = GetEntityPersister(persistentClass.FullName);
				if (p.HasCache)
				{
					if (log.IsDebugEnabled())
					{
						log.Debug("evicting second-level cache: {0}", p.EntityName);
					}
					return p.Cache.ClearAsync(cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task EvictEntityAsync(string entityName, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				IEntityPersister p = GetEntityPersister(entityName);
				if (p.HasCache)
				{
					if (log.IsDebugEnabled())
					{
						log.Debug("evicting second-level cache: {0}", p.EntityName);
					}
					return p.Cache.ClearAsync(cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task EvictEntityAsync(string entityName, object id, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				IEntityPersister p = GetEntityPersister(entityName);
				if (p.HasCache)
				{
					if (log.IsDebugEnabled())
					{
						log.Debug("evicting second-level cache: {0}", MessageHelper.InfoString(p, id, this));
					}
					CacheKey cacheKey = GenerateCacheKeyForEvict(id, p.IdentifierType, p.RootEntityName);
					return p.Cache.RemoveAsync(cacheKey, cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task EvictCollectionAsync(string roleName, object id, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				ICollectionPersister p = GetCollectionPersister(roleName);
				if (p.HasCache)
				{
					if (log.IsDebugEnabled())
					{
						log.Debug("evicting second-level cache: {0}", MessageHelper.CollectionInfoString(p, id));
					}
					CacheKey ck = GenerateCacheKeyForEvict(id, p.KeyType, p.Role);
					return p.Cache.RemoveAsync(ck, cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task EvictCollectionAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				ICollectionPersister p = GetCollectionPersister(roleName);
				if (p.HasCache)
				{
					if (log.IsDebugEnabled())
					{
						log.Debug("evicting second-level cache: {0}", p.Role);
					}
					return p.Cache.ClearAsync(cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public async Task EvictQueriesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			// NH Different implementation
			if (queryCache != null)
			{
				await (queryCache.ClearAsync(cancellationToken)).ConfigureAwait(false);
				if (queryCaches.Count == 0)
				{
					await (updateTimestampsCache.ClearAsync(cancellationToken)).ConfigureAwait(false);
				}
			}
		}

		public Task EvictQueriesAsync(string cacheRegion, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				if (string.IsNullOrEmpty(cacheRegion))
				{
					return Task.FromException<object>(new ArgumentNullException("cacheRegion", "use the zero-argument form to evict the default query cache"));
				}
				else
				{
					if (settings.IsQueryCacheEnabled)
					{
						IQueryCache currentQueryCache;
						if (queryCaches.TryGetValue(cacheRegion, out currentQueryCache))
						{
							return currentQueryCache.ClearAsync(cancellationToken);
						}
					}
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion
	}
}
