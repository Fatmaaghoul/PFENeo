using AutoMapper;
using Docvision.Dtos;
using Docvision.Models;
using Docvision.Repositories;

namespace Docvision.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IMapper _mapper;

        public ProfileService(IProfileRepository profileRepository, IMapper mapper)
        {
            _profileRepository = profileRepository;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetProfileAsync(string userId)
        {
            var user = await _profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };
            }

            var profile = _mapper.Map<ProfileResponse>(user);
            return new ResponseModel { Success = true, Message = "Profil récupéré avec succès.", Data = profile };
        }

        public async Task<ResponseModel> EditProfileAsync(string userId, EditProfileRequest request)
        {
            var user = await _profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };
            }

            // Mise à jour des informations de base
            user.UserName = request.UserName ?? user.UserName;
            user.Email = request.Email ?? user.Email;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

            // Changement de mot de passe (si fourni)
            if (!string.IsNullOrEmpty(request.CurrentPassword) && !string.IsNullOrEmpty(request.NewPassword))
            {
                var passwordCheck = await _profileRepository.CheckPasswordAsync(user, request.CurrentPassword);
                if (!passwordCheck)
                {
                    return new ResponseModel { Success = false, Message = "Mot de passe actuel incorrect." };
                }

                var result = await _profileRepository.ResetPasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };
                }
            }

            // Mise à jour de l'utilisateur dans la base de données
            var updateResult = await _profileRepository.UpdateUserAsync(user);
            if (!updateResult.Succeeded)
            {
                return new ResponseModel { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
            }

            return new ResponseModel { Success = true, Message = "Profil mis à jour avec succès." };
        }
    }
}
