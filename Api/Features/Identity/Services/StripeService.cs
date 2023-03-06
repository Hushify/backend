using Hushify.Api.Exceptions;
using Hushify.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Hushify.Api.Features.Identity.Services;

public interface IStripeService
{
    Task CreateCustomerAsync(Guid id, CancellationToken cancellationToken);
}

public class StripeService : IStripeService
{
    private readonly AppDbContext _context;
    private readonly IStripeClient _stripeClient;

    public StripeService(IStripeClient stripeClient, AppDbContext context)
    {
        _stripeClient = stripeClient;
        _context = context;
    }

    public async Task CreateCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
        if (user is null)
        {
            throw new AppException("User not found.");
        }

        var customerService = new CustomerService(_stripeClient);
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email
        }, cancellationToken: cancellationToken);

        user.StripeCustomerId = customer.Id;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class NoOpStripeService : IStripeService
{
    public Task CreateCustomerAsync(Guid id, CancellationToken cancellationToken) => Task.CompletedTask;
}