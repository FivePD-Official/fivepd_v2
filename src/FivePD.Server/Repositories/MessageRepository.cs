using System;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for Message CRUD operations.
/// </summary>
public class MessageRepository : BaseRepository<Message, MessageUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public MessageRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Messages)
    {
    }

    /// <inheritdoc />
    public override Message Add(Message message)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                message.Sender,
                message.Subject,
                message.Content,
                message.SentAt,
                message.HasRead,
                message.UserId,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        message.Id = id;
        return message;
    }

    /// <inheritdoc />
    public override void Update(Message message, Action<MessageUpdate> updateAction)
    {
        var update = new MessageUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Sender), update.Sender);
        builder.Add(nameof(update.Subject), update.Subject);
        builder.Add(nameof(update.Content), update.Content);
        builder.Add(nameof(update.SentAt), update.SentAt);
        builder.Add(nameof(update.HasRead), update.HasRead);
        builder.Add(nameof(update.UserId), update.UserId);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, message.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }
}