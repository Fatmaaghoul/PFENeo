using Docvision.Models;
using Docvision.Persistance;
using Docvision.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Docvision.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        public async Task<ResponseModel> AddUserAsync(string username, string email, string phonenumber, string password, List<string> roles)
        {
            var user = new ApplicationUser { UserName = username, Email = email , PhoneNumber = phonenumber };
            var result = await _userRepository.CreateUserAsync(user, password);

            if (!result.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            var roleResult = await _userRepository.AddRolesToUserAsync(user, roles);
            if (!roleResult.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", roleResult.Errors.Select(e => e.Description)) };

            return new ResponseModel { Success = true, Message = "Utilisateur ajouté avec succès." };
        }

        public async Task<ResponseModel> EditUserAsync(string userId, string username, string email, string phonenumber,  List<string> roles)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };

            user.UserName = username;
            user.Email = email;

            user.PhoneNumber = phonenumber;

            var updateResult = await _userRepository.UpdateUserAsync(user);
            if (!updateResult.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };

            var currentRoles = await _userRepository.GetUserRolesAsync(user);
            var removeResult = await _userRepository.RemoveRolesFromUserAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", removeResult.Errors.Select(e => e.Description)) };

            var addResult = await _userRepository.AddRolesToUserAsync(user, roles);
            if (!addResult.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", addResult.Errors.Select(e => e.Description)) };

            return new ResponseModel { Success = true, Message = "Utilisateur modifié avec succès." };
        }

        public async Task<ResponseModel> DeleteUserAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };

            var result = await _userRepository.DeleteUserAsync(user);
            if (!result.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ResponseModel { Success = true, Message = "Utilisateur supprimé avec succès." };
        }

        public async Task<List<UserWithRolesResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userResponses = new List<UserWithRolesResponse>();

            foreach (var user in users)
            {
                var roles = await _userRepository.GetUserRolesAsync(user);
                userResponses.Add(new UserWithRolesResponse
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                    Roles = roles
                });
            }

            return userResponses;
        }
    }
}