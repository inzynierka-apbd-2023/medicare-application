using PractitionerService.Data;
using PractitionerService.Models;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Messaging.Notifiers;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace PractitionerService.Services
{
    public interface IStaffService
    {
        Task<StaffMemberDto?> CreateStaffMemberAsync(CreateStaffRequest request, CancellationToken cancellationToken = default);
        Task<StaffMemberDto?> UpdateStaffMemberAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteStaffMemberAsync(Guid id, CancellationToken cancellationToken = default);
        Task<StaffMemberDto?> GetStaffMemberByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<StaffMemberDto>> GetAllStaffMembersAsync(StaffSearchRequest searchRequest, CancellationToken cancellationToken = default);
        Task<List<StaffMemberDto>> GetStaffMembersByRoleAsync(string role, CancellationToken cancellationToken = default);
    }

    public class StaffService : IStaffService
    {
        private readonly PractitionerDbContext _context;
        private readonly ILogger<StaffService> _logger;
        private readonly IStaffNotifier _staffNotifier;
        private readonly IRequestClient<IGetUser> _getUserClient;
        private readonly IRequestClient<IGetUsers> _getUsersClient;
        private readonly IRequestClient<ICreateUser> _createUserClient;
        private readonly IRequestClient<IUpdateUser> _updateUserClient;
        private readonly IRequestClient<IDeleteUser> _deleteUserClient;

        public StaffService(
            PractitionerDbContext context,
            ILogger<StaffService> logger,
            IStaffNotifier staffNotifier,
            IRequestClient<IGetUser> getUserClient,
            IRequestClient<IGetUsers> getUsersClient,
            IRequestClient<ICreateUser> createUserClient,
            IRequestClient<IUpdateUser> updateUserClient,
            IRequestClient<IDeleteUser> deleteUserClient)
        {
            _context = context;
            _logger = logger;
            _staffNotifier = staffNotifier;
            _getUserClient = getUserClient;
            _getUsersClient = getUsersClient;
            _createUserClient = createUserClient;
            _updateUserClient = updateUserClient;
            _deleteUserClient = deleteUserClient;
        }

        public async Task<StaffMemberDto?> CreateStaffMemberAsync(CreateStaffRequest request, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var createUserResponse = await _createUserClient.GetResponse<ICreatedUserResponse>(new
            {
                request.Profile.FirstName,
                request.Profile.LastName,
                request.Profile.Email,
                request.Profile.Phone,
                request.Profile.DateOfBirth,
                request.Profile.Gender,
                request.Profile.AddressLine1,
                request.Profile.AddressLine2,
                request.Profile.City,
                request.Profile.State,
                request.Profile.ZipCode,
                request.Profile.Country,
                request.Role,
                Password = (string?)null
            }, cancellationToken);

            var userResult = createUserResponse.Message;
            if (!userResult.Success || userResult.Id == Guid.Empty)
            {
                _logger.LogError("Failed to create user profile: {Error}", userResult.ErrorMessage);
                throw new InvalidOperationException($"Failed to create user profile: {userResult.ErrorMessage}");
            }

            var userId = userResult.Id;
            var now = DateTime.UtcNow;

            if (request.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Bio = request.Biography,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync(cancellationToken);

                if (request.Specializations?.Any() == true)
                {
                    foreach (var specializationId in request.Specializations)
                    {
                        var doctorSpecialization = new DoctorSpecialization
                        {
                            DoctorId = doctor.Id,
                            SpecializationId = specializationId
                        };
                        _context.DoctorSpecializations.Add(doctorSpecialization);
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else if (request.Role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var receptionist = new Receptionist
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Receptionists.Add(receptionist);
                await _context.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return await GetStaffMemberByUserIdAsync(userId, cancellationToken);
        }

        public async Task<StaffMemberDto?> UpdateStaffMemberAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (doctor == null && receptionist == null)
            {
                return null;
            }

            var userId = doctor?.UserId ?? receptionist?.UserId;
            if (!userId.HasValue)
            {
                return null;
            }

            if (request.Profile != null)
            {
                var updateResponse = await _updateUserClient.GetResponse<IUpdatedUserResponse>(new
                {
                    UserId = userId.Value,
                    request.Profile.FirstName,
                    request.Profile.LastName,
                    request.Profile.Email,
                    request.Profile.Phone,
                    request.Profile.DateOfBirth,
                    request.Profile.Gender,
                    request.Profile.AddressLine1,
                    request.Profile.AddressLine2,
                    request.Profile.City,
                    request.Profile.State,
                    request.Profile.ZipCode,
                    request.Profile.Country
                }, cancellationToken);

                if (!updateResponse.Message.Success)
                {
                    _logger.LogError("Failed to update user profile: {Error}", updateResponse.Message.ErrorMessage);
                    throw new InvalidOperationException($"Failed to update user profile: {updateResponse.Message.ErrorMessage}");
                }
            }

            var now = DateTime.UtcNow;

            if (doctor != null)
            {
                if (!string.IsNullOrEmpty(request.Biography))
                    doctor.Bio = request.Biography;

                doctor.UpdatedAt = now;

                if (request.Specializations != null)
                {
                    var existingSpecializations = await _context.DoctorSpecializations
                        .Where(ds => ds.DoctorId == doctor.Id)
                        .ToListAsync(cancellationToken);

                    _context.DoctorSpecializations.RemoveRange(existingSpecializations);

                    foreach (var specializationId in request.Specializations)
                    {
                        var doctorSpecialization = new DoctorSpecialization
                        {
                            DoctorId = doctor.Id,
                            SpecializationId = specializationId
                        };
                        _context.DoctorSpecializations.Add(doctorSpecialization);
                    }
                }
            }
            else if (receptionist != null)
            {
                receptionist.UpdatedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetStaffMemberByUserIdAsync(userId.Value, cancellationToken);
        }

        public async Task<bool> DeleteStaffMemberAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (doctor == null && receptionist == null)
            {
                return false;
            }

            var userId = doctor?.UserId ?? receptionist?.UserId;

            var deleteResponse = await _deleteUserClient.GetResponse<IDeletedUserResponse>(new
            {
                UserId = userId!.Value
            }, cancellationToken);

            if (!deleteResponse.Message.Success)
            {
                _logger.LogError("Failed to delete user profile: {Error}", deleteResponse.Message.ErrorMessage);
                return false;
            }

            if (doctor != null)
            {
                await _staffNotifier.NotifyDoctorArchived(doctor.Id, doctor.UserId, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return true;
        }


        public async Task<StaffMemberDto?> GetStaffMemberByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (doctor == null && receptionist == null)
            {
                return null;
            }

            var userId = doctor?.UserId ?? receptionist?.UserId;
            return await GetStaffMemberByUserIdAsync(userId!.Value, cancellationToken);
        }

        public async Task<List<StaffMemberDto>> GetAllStaffMembersAsync(StaffSearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            var staffMembers = new List<StaffMemberDto>();

            var doctors = string.IsNullOrEmpty(searchRequest.Role) || searchRequest.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase)
                ? await _context.Doctors.ToListAsync(cancellationToken)
                : new List<Doctor>();

            var receptionists = string.IsNullOrEmpty(searchRequest.Role) || searchRequest.Role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase)
                ? await _context.Receptionists.ToListAsync(cancellationToken)
                : new List<Receptionist>();

            var allUserIds = doctors.Select(d => d.UserId)
                .Concat(receptionists.Select(r => r.UserId))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (allUserIds.Count == 0)
            {
                return staffMembers;
            }

            var usersResponse = await _getUsersClient.GetResponse<IUsersResponse>(new { UserIds = allUserIds }, cancellationToken);
            var userProfiles = usersResponse.Message.Users?.ToDictionary(u => u.Id) ?? new Dictionary<Guid, IUserResponse>();

            foreach (var doctor in doctors)
            {
                if (userProfiles.TryGetValue(doctor.UserId, out var profile))
                {
                    var specializations = await _context.DoctorSpecializations
                        .Where(ds => ds.DoctorId == doctor.Id)
                        .Join(_context.Specializations, ds => ds.SpecializationId, s => s.Id, (ds, s) => s)
                        .ToListAsync(cancellationToken);

                    staffMembers.Add(MapToStaffMemberDto(doctor, null, profile, specializations));
                }
            }

            foreach (var receptionist in receptionists)
            {
                if (userProfiles.TryGetValue(receptionist.UserId, out var profile))
                {
                    staffMembers.Add(MapToStaffMemberDto(null, receptionist, profile, null));
                }
            }

            if (!string.IsNullOrEmpty(searchRequest.SearchQuery))
            {
                staffMembers = staffMembers.Where(s =>
                    s.Profile.FirstName.Contains(searchRequest.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    s.Profile.LastName.Contains(searchRequest.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    s.Profile.Email.Contains(searchRequest.SearchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (searchRequest.IsActive.HasValue)
            {
                staffMembers = staffMembers.Where(s => s.IsActive == searchRequest.IsActive.Value).ToList();
            }

            var skip = (searchRequest.Page - 1) * searchRequest.PageSize;
            return staffMembers.Skip(skip).Take(searchRequest.PageSize).ToList();
        }

        public async Task<List<StaffMemberDto>> GetStaffMembersByRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            var searchRequest = new StaffSearchRequest
            {
                Role = role,
                Page = 1,
                PageSize = 1000
            };

            return await GetAllStaffMembersAsync(searchRequest, cancellationToken);
        }

        private async Task<StaffMemberDto?> GetStaffMemberByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userResponse = await _getUserClient.GetResponse<IUserResponse>(new { UserId = userId }, cancellationToken);
            var profile = userResponse.Message;

            if (profile == null || profile.Id == Guid.Empty)
            {
                _logger.LogWarning("Failed to get user profile for userId {UserId}", userId);
                return null;
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
            var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

            if (doctor == null && receptionist == null)
            {
                return null;
            }

            List<Specialization>? specializations = null;
            if (doctor != null)
            {
                specializations = await _context.DoctorSpecializations
                    .Where(ds => ds.DoctorId == doctor.Id)
                    .Join(_context.Specializations, ds => ds.SpecializationId, s => s.Id, (ds, s) => s)
                    .ToListAsync(cancellationToken);
            }

            return MapToStaffMemberDto(doctor, receptionist, profile, specializations);
        }

        private static StaffMemberDto MapToStaffMemberDto(Doctor? doctor, Receptionist? receptionist, IUserResponse profile, List<Specialization>? specializations)
        {
            return new StaffMemberDto
            {
                Id = doctor?.Id ?? receptionist!.Id,
                Role = doctor != null ? "Doctor" : "Receptionist",
                Profile = new ProfileDto
                {
                    FirstName = profile.FirstName ?? "",
                    LastName = profile.LastName ?? "",
                    Email = profile.Email ?? "",
                    Phone = profile.Phone,
                    DateOfBirth = profile.DateOfBirth ?? DateTime.MinValue,
                    Gender = profile.Gender ?? "",
                    AddressLine1 = profile.AddressLine1 ?? "",
                    AddressLine2 = profile.AddressLine2,
                    City = profile.City ?? "",
                    State = profile.State ?? "",
                    ZipCode = profile.ZipCode ?? "",
                    Country = profile.Country ?? ""
                },
                IsActive = profile.IsActive,
                CreatedAt = doctor?.CreatedAt ?? receptionist!.CreatedAt,
                UpdatedAt = doctor?.UpdatedAt ?? receptionist!.UpdatedAt
            };
        }
    }
}
