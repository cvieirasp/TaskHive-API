using TaskHive.Domain.Repositories;

namespace TaskHive.Application.UseCases.Users
{
    public class DeleteUserUseCase(IUserRepository userRepository)
    {
        public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Delete user
            await userRepository.DeleteAsync(id, cancellationToken);
        }
    }
}
