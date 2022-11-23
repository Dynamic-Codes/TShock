﻿using System;
using Microsoft.EntityFrameworkCore;
using TShockAPI.DB;

namespace TShockAPI.Services.Scoping;

/// <summary>
/// Service providing scoped <see cref="EntityContext{TEntity}"/> instances
/// </summary>
/// <typeparam name="TEntity">Type of entity being operated on by the <see cref="EntityContext{TEntity}"/></typeparam>
public abstract class DatabaseService<TEntity> where TEntity : class
{
	private readonly IDbContextFactory<EntityContext<TEntity>> _contextFactory;

	/// <summary>
	/// Constructs a new DatabaseService with the given <see cref="IDbContextFactory{TContext}"/>
	/// </summary>
	/// <param name="contextFactory">Context factory that will be used to create database contexts</param>
	protected DatabaseService(IDbContextFactory<EntityContext<TEntity>> contextFactory)
	{
		_contextFactory = contextFactory;
	}

	/// <summary>
	///	Creates a new database context, optionally allowing the context to save changes when it is disposed.
	/// <para/>
	/// Contexts should always be disposed after use. Where possible, consider using contexts in a
	/// <c>using</c> block
	/// </summary>
	/// <returns>An <see cref="EntityContext{TEntity}"/></returns>
	public virtual EntityContext<TEntity> GetContext(bool saveOnDispose = false)
	{
		EntityContext<TEntity> context = _contextFactory.CreateDbContext();
		context.SaveOnDispose = saveOnDispose;
		return context;
	}

	/// <summary>
	/// Executes a function with an <see cref="EntityContext{TEntity}"/>, returning the output of the function.
	/// If the given <paramref name="context"/> is null, <see cref="GetContext"/> is called to provide the context
	/// that is passed to the function.
	/// </summary>
	/// <param name="function">Function to be executed</param>
	/// <param name="context">Context the function will use. May be null</param>
	/// <param name="saveOnDispose">Whether or not to save changes to a context generated by <see cref="GetContext"/>,
	/// if required</param>
	/// <typeparam name="TOut">Output type of the function</typeparam>
	/// <returns>The output of the given function</returns>
	protected virtual TOut ContextSafeFunc<TOut>(
		Func<EntityContext<TEntity>, TOut> function,
		EntityContext<TEntity>? context,
		bool saveOnDispose = true)
	{
		if (context != null)
		{
			return function(context);
		}

		using EntityContext<TEntity> singleUseContext = GetContext(saveOnDispose: saveOnDispose);
		return function(singleUseContext);
	}
}
