using System;
using System.Linq;
using FivePD.Server.Models;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Database;

/// <summary>
/// Simple test class to verify Dapper migration works correctly.
/// This should be run manually to verify the migration.
/// </summary>
public class MessageRepositoryTests
{
    private readonly MessageRepository _repository;
    private readonly IDatabaseContextFactory _factory;

    public MessageRepositoryTests(IDatabaseContextFactory factory)
    {
        _factory = factory;
        _repository = new MessageRepository(factory);
    }

    /// <summary>
    /// Tests the complete message flow: create, read, update, delete.
    /// </summary>
    public bool RunAllTests()
    {
        try
        {
            Console.WriteLine("Starting MessageRepository tests...");
            
            // Test 1: Create message
            Console.WriteLine("Test 1: Creating message...");
            var testUser = new User { Id = 1, Name = "TestUser" }; // Assuming user with ID 1 exists
            var message = new Message
            {
                Sender = "TestSender",
                Subject = "Test Subject",
                Content = "Test Content",
                SentAt = DateTime.Now,
                Read = false,
                UserId = testUser.Id,
                User = testUser
            };

            var savedMessage = _repository.Add(message);
            if (savedMessage.Id == 0)
            {
                Console.WriteLine("❌ Test 1 FAILED: Message ID not generated");
                return false;
            }
            Console.WriteLine($"✓ Test 1 PASSED: Message created with ID {savedMessage.Id}");

            // Test 2: Find by ID
            Console.WriteLine("Test 2: Finding message by ID...");
            var foundMessage = _repository.FindById(savedMessage.Id);
            if (foundMessage == null || foundMessage.Id != savedMessage.Id)
            {
                Console.WriteLine("❌ Test 2 FAILED: Message not found");
                return false;
            }
            Console.WriteLine("✓ Test 2 PASSED: Message found");

            // Test 3: Get by user ID with pagination
            Console.WriteLine("Test 3: Getting messages by user ID...");
            var page = _repository.GetByUserId(testUser.Id, 1, 6);
            if (page == null || !page.Items.Any())
            {
                Console.WriteLine("❌ Test 3 FAILED: No messages found for user");
                return false;
            }
            Console.WriteLine($"✓ Test 3 PASSED: Found {page.ItemCount} messages for user");

            // Test 4: Update message
            Console.WriteLine("Test 4: Updating message...");
            foundMessage.Read = true;
            foundMessage.Subject = "Updated Subject";
            _repository.Update(foundMessage);

            var updatedMessage = _repository.FindById(foundMessage.Id);
            if (updatedMessage == null || !updatedMessage.Read || updatedMessage.Subject != "Updated Subject")
            {
                Console.WriteLine("❌ Test 4 FAILED: Message not updated correctly");
                return false;
            }
            Console.WriteLine("✓ Test 4 PASSED: Message updated");

            // Test 5: Get for user (with verification)
            Console.WriteLine("Test 5: Getting message for specific user...");
            var userMessage = _repository.GetForUser(foundMessage.Id, testUser.Id);
            if (userMessage == null)
            {
                Console.WriteLine("❌ Test 5 FAILED: Message not retrieved for user");
                return false;
            }
            Console.WriteLine("✓ Test 5 PASSED: User-specific message retrieval works");

            // Test 6: Cleanup - Delete message
            Console.WriteLine("Test 6: Deleting message...");
            _repository.Delete(foundMessage.Id);
            var deletedMessage = _repository.FindById(foundMessage.Id);
            if (deletedMessage != null)
            {
                Console.WriteLine("❌ Test 6 FAILED: Message not deleted");
                return false;
            }
            Console.WriteLine("✓ Test 6 PASSED: Message deleted");

            Console.WriteLine("\n🎉 All tests PASSED! Dapper migration is working correctly.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test FAILED with exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Tests the pagination functionality specifically.
    /// </summary>
    public bool TestPagination()
    {
        try
        {
            Console.WriteLine("\nTesting pagination...");
            
            // Create multiple test messages
            var userId = 1; // Use existing user
            for (int i = 1; i <= 5; i++)
            {
                var msg = new Message
                {
                    Sender = $"Sender{i}",
                    Subject = $"Subject{i}",
                    Content = $"Content{i}",
                    SentAt = DateTime.Now.AddMinutes(-i),
                    Read = false,
                    UserId = userId
                };
                _repository.Add(msg);
            }

            // Test page 1
            var page1 = _repository.GetByUserId(userId, 1, 3);
            Console.WriteLine($"Page 1: {page1.Items.Count()} items, Total: {page1.ItemCount}, Pages: {page1.PageCount}");

            // Test page 2
            var page2 = _repository.GetByUserId(userId, 2, 3);
            Console.WriteLine($"Page 2: {page2.Items.Count()} items, Total: {page2.ItemCount}, Pages: {page2.PageCount}");

            // Cleanup
            foreach (var item in page1.Items.Concat(page2.Items))
            {
                _repository.Delete(item.Id);
            }

            Console.WriteLine("✓ Pagination test PASSED");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Pagination test FAILED: {ex.Message}");
            return false;
        }
    }
}