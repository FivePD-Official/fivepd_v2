using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Database;

#pragma warning disable

/// <summary>
/// Dapper-based repository for Message CRUD operations.
/// Replaces EF Core's DbSet<Message> functionality.
/// </summary>
public class MessageRepository
{
    private readonly IDapperDatabaseConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database context factory.</param>
    public MessageRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    /// <summary>
    /// Adds a new message to the database.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <returns>The added message with generated ID.</returns>
    public Message Add(Message message)
    {
        using var scope = this._dbConnectionFactory.CreateScope();
        var connection = scope.Connection;

        const string sql = @"
            INSERT INTO fivepd_messages (Sender, Subject, Content, SentAt, Read, UserId)
            VALUES (@Sender, @Subject, @Content, @SentAt, @Read, @UserId);
            SELECT LAST_INSERT_ID();";

        var id = connection.ExecuteScalar<int>(sql, new
        {
            message.Sender,
            message.Subject,
            message.Content,
            message.SentAt,
            message.Read,
            // message.UserId
        });

        message.Id = id;
        return message;
    }

    /// <summary>
    /// Finds a message by its ID.
    /// </summary>
    /// <param name="id">The message ID.</param>
    /// <returns>The message if found, null otherwise.</returns>
    public Message FindById(int id)
    {
        using var scope = this._dbConnectionFactory.CreateScope();
        var connection = scope.Connection;

        const string sql = @"
            SELECT m.*, u.*
            FROM fivepd_messages m
            LEFT JOIN fivepd_users u ON m.UserId = u.Id
            WHERE m.Id = @Id";

        var result = connection.Query<Message, User, Message>(
            sql,
            (message, user) =>
            {
                message.User = user;
                return message;
            },
            new { Id = id },
            splitOn: "Id"
        ).FirstOrDefault();

        return result;
    }

    /// <summary>
    /// Gets all messages for a specific user with pagination.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filters">Optional filters.</param>
    /// <returns>A page of messages for the user.</returns>
    public Page<Message> GetByUserId(int userId, int page, int pageSize = 6, Filters<Message> filters = null)
    {
        using var scope = this._dbConnectionFactory.CreateScope();
        var connection = scope.Connection;

        // Base query
        var baseQuery = "SELECT * FROM fivepd_messages WHERE UserId = @UserId";
        var countQuery = "SELECT COUNT(*) FROM fivepd_messages WHERE UserId = @UserId";

        // Apply filters if provided
        if (filters != null && filters.IsValid())
        {
            // For Dapper, we'd need to build dynamic SQL. For simplicity, we'll handle basic filtering
            // The MessageService filters are applied in-memory after fetching
        }

        // Get total count
        var totalCount = connection.ExecuteScalar<int>(countQuery, new { UserId = userId });

        // Calculate pagination
        var offset = (page - 1) * pageSize;
        
        // Get paginated results
        var sql = $@"
            SELECT m.*, u.*
            FROM fivepd_messages m
            LEFT JOIN fivepd_users u ON m.UserId = u.Id
            WHERE m.UserId = @UserId
            ORDER BY m.SentAt DESC
            LIMIT @Limit OFFSET @Offset";

        var messages = connection.Query<Message, User, Message>(
            sql,
            (message, user) =>
            {
                message.User = user;
                return message;
            },
            new { UserId = userId, Limit = pageSize, Offset = offset },
            splitOn: "Id"
        ).ToList();

        // Apply in-memory filters if needed
        if (filters != null && filters.IsValid())
        {
            var predicate = filters.Get().First().Expression.Compile();
            messages = messages.Where(predicate).ToList();
        }

        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
        if (pageCount == 0) pageCount = 1;

        return new Page<Message>
        {
            CurrentPage = page,
            PageSize = pageSize,
            ItemCount = totalCount,
            PageCount = pageCount,
            Items = messages
        };
    }

    /// <summary>
    /// Updates a message in the database.
    /// </summary>
    /// <param name="message">The message to update.</param>
    public void Update(Message message)
    {
        using var scope = this._dbConnectionFactory.CreateScope();
        var connection = scope.Connection;

        const string sql = @"
            UPDATE fivepd_messages
            SET Sender = @Sender, 
                Subject = @Subject, 
                Content = @Content, 
                SentAt = @SentAt, 
                Read = @Read, 
                UserId = @UserId
            WHERE Id = @Id";

        connection.Execute(sql, new
        {
            message.Id,
            message.Sender,
            message.Subject,
            message.Content,
            message.SentAt,
            message.Read,
            // message.UserId
        });
    }

    /// <summary>
    /// Deletes a message by ID.
    /// </summary>
    /// <param name="id">The message ID.</param>
    public void Delete(int id)
    {
        using var scope = this._dbConnectionFactory.CreateScope();
        var connection = scope.Connection;

        const string sql = "DELETE FROM fivepd_messages WHERE Id = @Id";
        connection.Execute(sql, new { Id = id });
    }

    /// <summary>
    /// Gets a single message with user data, ensuring the message belongs to the specified user.
    /// </summary>
    /// <param name="messageId">The message ID.</param>
    /// <param name="userId">The user ID to verify ownership.</param>
    /// <returns>The message if found and owned by user, null otherwise.</returns>
    public Message GetForUser(int messageId, int userId)
    {
        using var scope = this._dbConnectionFactory.CreateScope();
        var connection = scope.Connection;

        const string sql = @"
            SELECT m.*, u.*
            FROM fivepd_messages m
            LEFT JOIN fivepd_users u ON m.UserId = u.Id
            WHERE m.Id = @MessageId AND m.UserId = @UserId";

        return connection.Query<Message, User, Message>(
            sql,
            (message, user) =>
            {
                message.User = user;
                return message;
            },
            new { MessageId = messageId, UserId = userId },
            splitOn: "Id"
        ).FirstOrDefault();
    }
}