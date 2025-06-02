using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyChatApp.ApiService.Controllers;
using MyChatApp.ApiService.Models;
using MyChatApp.Web.Data;
using NSubstitute;
using System.Security.Claims;

namespace MyChatApp.Tests;

[TestFixture]
public class ContactsControllerTests
{
    private ContactsController _controller;
    private UserManager<ApplicationUser> _userManager;
    private ApplicationDbContext _context;
    private ILogger<ContactsController> _logger;
    private ApplicationUser _testUser;
    private ApplicationUser _targetUser;

    [SetUp]
    public void Setup()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);
        
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        
        _logger = Substitute.For<ILogger<ContactsController>>();
        _controller = new ContactsController(_userManager, _context, _logger);

        _testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        _targetUser = new ApplicationUser
        {
            Id = "target-user-id",
            UserName = "targetuser",
            Email = "target@example.com",
            DisplayName = "Target User"
        };

        // Setup controller context with user claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUser.Id)
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        // Setup UserManager mocks
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(_testUser);
        _userManager.FindByIdAsync(_targetUser.Id).Returns(_targetUser);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _userManager?.Dispose();
    }

    [Test]
    public async Task SearchUsersAsync_ReturnsSearchResults_WhenUsersExist()
    {
        // Arrange
        var searchRequest = new ContactSearchRequest { Query = "target" };
        
        var users = new List<ApplicationUser> { _targetUser }.AsQueryable();
        _userManager.Users.Returns(users);

        // Act
        var result = await _controller.SearchUsersAsync(searchRequest);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        
        var searchResults = okResult.Value as IEnumerable<UserSearchResultDto>;
        Assert.That(searchResults, Is.Not.Null);
        
        var resultList = searchResults.ToList();
        Assert.That(resultList, Has.Count.EqualTo(1));
        Assert.That(resultList[0].UserId, Is.EqualTo(_targetUser.Id));
        Assert.That(resultList[0].DisplayName, Is.EqualTo(_targetUser.DisplayName));
        Assert.That(resultList[0].ContactStatus, Is.Null); // No existing contact
    }

    [Test]
    public async Task SendContactRequestAsync_CreatesContactRequest_WhenValid()
    {
        // Arrange
        var request = new SendContactRequestDto { ReceiverId = _targetUser.Id };

        // Act
        var result = await _controller.SendContactRequestAsync(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        
        var contactRequest = createdResult.Value as ContactRequestDto;
        Assert.That(contactRequest, Is.Not.Null);
        Assert.That(contactRequest.RequesterId, Is.EqualTo(_testUser.Id));
        Assert.That(contactRequest.ReceiverId, Is.EqualTo(_targetUser.Id));
        Assert.That(contactRequest.Status, Is.EqualTo(ContactStatusDto.Pending));

        // Verify contact was created in database
        var contact = await _context.Contacts.FirstOrDefaultAsync();
        Assert.That(contact, Is.Not.Null);
        Assert.That(contact.RequesterId, Is.EqualTo(_testUser.Id));
        Assert.That(contact.ReceiverId, Is.EqualTo(_targetUser.Id));
        Assert.That(contact.Status, Is.EqualTo(ContactStatus.Pending));
    }

    [Test]
    public async Task SendContactRequestAsync_ReturnsConflict_WhenContactAlreadyExists()
    {
        // Arrange
        var existingContact = new Contact
        {
            RequesterId = _testUser.Id,
            ReceiverId = _targetUser.Id,
            Status = ContactStatus.Pending
        };
        _context.Contacts.Add(existingContact);
        await _context.SaveChangesAsync();

        var request = new SendContactRequestDto { ReceiverId = _targetUser.Id };

        // Act
        var result = await _controller.SendContactRequestAsync(request);

        // Assert
        var conflictResult = result.Result as ConflictObjectResult;
        Assert.That(conflictResult, Is.Not.Null);
        Assert.That(conflictResult.Value, Is.EqualTo("Contact relationship already exists"));
    }

    [Test]
    public async Task UpdateContactAsync_AcceptsContactRequest_WhenReceiverAccepts()
    {
        // Arrange
        var contact = new Contact
        {
            RequesterId = _targetUser.Id, // Target user sent the request
            ReceiverId = _testUser.Id,    // Test user receives it
            Status = ContactStatus.Pending
        };
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateContactRequestDto { Status = ContactStatusDto.Accepted };

        // Act
        var result = await _controller.UpdateContactAsync(contact.Id, updateRequest);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        
        var updatedContact = okResult.Value as ContactRequestDto;
        Assert.That(updatedContact, Is.Not.Null);
        Assert.That(updatedContact.Status, Is.EqualTo(ContactStatusDto.Accepted));

        // Verify status was updated in database
        var dbContact = await _context.Contacts.FindAsync(contact.Id);
        Assert.That(dbContact, Is.Not.Null);
        Assert.That(dbContact.Status, Is.EqualTo(ContactStatus.Accepted));
    }

    [Test]
    public async Task DeleteContactAsync_RemovesContact_WhenUserIsAuthorized()
    {
        // Arrange
        var contact = new Contact
        {
            RequesterId = _testUser.Id,
            ReceiverId = _targetUser.Id,
            Status = ContactStatus.Accepted
        };
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteContactAsync(contact.Id);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);

        // Verify contact was removed from database
        var dbContact = await _context.Contacts.FindAsync(contact.Id);
        Assert.That(dbContact, Is.Null);
    }

    [Test]
    public async Task GetContactsAsync_ReturnsUserContacts_WhenContactsExist()
    {
        // Arrange
        var contact1 = new Contact
        {
            RequesterId = _testUser.Id,
            ReceiverId = _targetUser.Id,
            Status = ContactStatus.Accepted,
            Requester = _testUser,
            Receiver = _targetUser
        };

        var contact2 = new Contact
        {
            RequesterId = _targetUser.Id,
            ReceiverId = _testUser.Id,
            Status = ContactStatus.Pending,
            Requester = _targetUser,
            Receiver = _testUser
        };

        _context.Contacts.AddRange(contact1, contact2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetContactsAsync();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        
        var contacts = okResult.Value as IEnumerable<ContactDto>;
        Assert.That(contacts, Is.Not.Null);
        
        var contactList = contacts.ToList();
        Assert.That(contactList, Has.Count.EqualTo(2));
        
        var acceptedContact = contactList.FirstOrDefault(c => c.Status == ContactStatusDto.Accepted);
        Assert.That(acceptedContact, Is.Not.Null);
        Assert.That(acceptedContact.IsRequester, Is.True);
        
        var pendingContact = contactList.FirstOrDefault(c => c.Status == ContactStatusDto.Pending);
        Assert.That(pendingContact, Is.Not.Null);
        Assert.That(pendingContact.IsRequester, Is.False);
    }
}