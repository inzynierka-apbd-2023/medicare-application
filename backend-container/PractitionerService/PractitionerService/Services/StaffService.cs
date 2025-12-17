using PractitionerService.Data;
using PractitionerService.Models;
using PractitionerService.Features.StaffManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;

namespace PractitionerService.Services
{
    public interface IStaffService
    {
        Task<StaffMemberDto?> CreateStaffMemberAsync(CreateStaffRequest request, CancellationToken cancellationToken = default);
        Task<StaffMemberDto?> UpdateStaffMemberAsync(string id, UpdateStaffRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteStaffMemberAsync(string id, CancellationToken cancellationToken = default);
        Task<StaffMemberDto?> GetStaffMemberByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<List<StaffMemberDto>> GetAllStaffMembersAsync(StaffSearchRequest searchRequest, CancellationToken cancellationToken = default);
        Task<List<StaffMemberDto>> GetStaffMembersByRoleAsync(string role, CancellationToken cancellationToken = default);
    }

    public class StaffService : IStaffService
    {
        private readonly PractitionerDbContext _context;
        private readonly HttpClient _userServiceClient;
        private readonly ILogger<StaffService> _logger;

        public StaffService(
            PractitionerDbContext context, 
            HttpClient userServiceClient,
            ILogger<StaffService> logger)
        {
            _context = context;
            _userServiceClient = userServiceClient;
            _logger = logger;
        }

        public async Task<StaffMemberDto?> CreateStaffMemberAsync(CreateStaffRequest request, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Step 1: Create user profile via UserService
                var userProfileRequest = new
                {
                    firstName = request.Profile.FirstName,
                    lastName = request.Profile.LastName,
                    email = request.Profile.Email,
                    phone = request.Profile.Phone,
                    dateOfBirth = request.Profile.DateOfBirth,
                    gender = request.Profile.Gender,
                    addressLine1 = request.Profile.AddressLine1,
                    addressLine2 = request.Profile.AddressLine2,
                    city = request.Profile.City,
                    state = request.Profile.State,
                    zipCode = request.Profile.ZipCode,
                    country = request.Profile.Country,
                    role = request.Role
                };

                var userResponse = await _userServiceClient.PostAsJsonAsync("/api/users", userProfileRequest, cancellationToken);
                
                if (!userResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to create user profile: {StatusCode} - {Content}", userResponse.StatusCode, errorContent);
                    throw new InvalidOperationException($"Failed to create user profile: {userResponse.StatusCode}");
                }

                var userResponseContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
                var userResult = JsonSerializer.Deserialize<dynamic>(userResponseContent);
                var userId = userResult?.GetProperty("id").GetString();

                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException("Failed to get user ID from UserService response");
                }

                // Step 2: Create staff member in PractitionerService
                var now = DateTime.UtcNow;
                
                if (request.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
                {
                    var doctor = new Doctor
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Bio = request.Biography,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Add specializations if provided
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
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    _context.Receptionists.Add(receptionist);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                // Return the created staff member
                return await GetStaffMemberByUserIdAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to create staff member");
                throw;
            }
        }

        public async Task<StaffMemberDto?> UpdateStaffMemberAsync(string id, UpdateStaffRequest request, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Find the staff member
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
                var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (doctor == null && receptionist == null)
                {
                    return null;
                }

                var userId = doctor?.UserId ?? receptionist?.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                // Update user profile if provided
                if (request.Profile != null)
                {
                    var userUpdateRequest = new
                    {
                        firstName = request.Profile.FirstName,
                        lastName = request.Profile.LastName,
                        email = request.Profile.Email,
                        phone = request.Profile.Phone,
                        dateOfBirth = request.Profile.DateOfBirth,
                        gender = request.Profile.Gender,
                        addressLine1 = request.Profile.AddressLine1,
                        addressLine2 = request.Profile.AddressLine2,
                        city = request.Profile.City,
                        state = request.Profile.State,
                        zipCode = request.Profile.ZipCode,
                        country = request.Profile.Country
                    };

                    var userResponse = await _userServiceClient.PutAsJsonAsync($"/api/users/{userId}", userUpdateRequest, cancellationToken);
                    
                    if (!userResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogError("Failed to update user profile: {StatusCode} - {Content}", userResponse.StatusCode, errorContent);
                        throw new InvalidOperationException($"Failed to update user profile: {userResponse.StatusCode}");
                    }
                }

                // Update practitioner-specific data
                var now = DateTime.UtcNow;

                if (doctor != null)
                {
                    if (!string.IsNullOrEmpty(request.Biography))
                        doctor.Bio = request.Biography;
                    
                    doctor.UpdatedAt = now;

                    // Update specializations if provided
                    if (request.Specializations != null)
                    {
                        // Remove existing specializations
                        var existingSpecializations = await _context.DoctorSpecializations
                            .Where(ds => ds.DoctorId == doctor.Id)
                            .ToListAsync(cancellationToken);
                        
                        _context.DoctorSpecializations.RemoveRange(existingSpecializations);

                        // Add new specializations
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

                return await GetStaffMemberByUserIdAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to update staff member with ID {StaffId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteStaffMemberAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
                var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (doctor == null && receptionist == null)
                {
                    return false;
                }

                var userId = doctor?.UserId ?? receptionist?.UserId;

                // Soft delete via UserService
                var response = await _userServiceClient.DeleteAsync($"/api/users/{userId}", cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to delete user profile: {StatusCode}", response.StatusCode);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete staff member with ID {StaffId}", id);
                return false;
            }
        }

        public async Task<StaffMemberDto?> GetStaffMemberByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (doctor == null && receptionist == null)
            {
                return null;
            }

            var userId = doctor?.UserId ?? receptionist?.UserId;
            return await GetStaffMemberByUserIdAsync(userId!, cancellationToken);
        }

        public async Task<List<StaffMemberDto>> GetAllStaffMembersAsync(StaffSearchRequest searchRequest, CancellationToken cancellationToken = default)
        {
            var staffMembers = new List<StaffMemberDto>();

            // Get doctors if role filter allows
            if (string.IsNullOrEmpty(searchRequest.Role) || searchRequest.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctorsQuery = _context.Doctors.AsQueryable();
                
                var doctors = await doctorsQuery.ToListAsync(cancellationToken);
                
                foreach (var doctor in doctors)
                {
                    if (doctor.UserId != null)
                    {
                        var staffMember = await GetStaffMemberByUserIdAsync(doctor.UserId, cancellationToken);
                        if (staffMember != null)
                        {
                            staffMembers.Add(staffMember);
                        }
                    }
                }
            }

            // Get receptionists if role filter allows
            if (string.IsNullOrEmpty(searchRequest.Role) || searchRequest.Role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var receptionistsQuery = _context.Receptionists.AsQueryable();
                
                var receptionists = await receptionistsQuery.ToListAsync(cancellationToken);
                
                foreach (var receptionist in receptionists)
                {
                    var staffMember = await GetStaffMemberByUserIdAsync(receptionist.UserId, cancellationToken);
                    if (staffMember != null)
                    {
                        staffMembers.Add(staffMember);
                    }
                }
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchRequest.SearchQuery))
            {
                staffMembers = staffMembers.Where(s =>
                    s.Profile.FirstName.Contains(searchRequest.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    s.Profile.LastName.Contains(searchRequest.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    s.Profile.Email.Contains(searchRequest.SearchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Apply active filter
            if (searchRequest.IsActive.HasValue)
            {
                staffMembers = staffMembers.Where(s => s.IsActive == searchRequest.IsActive.Value).ToList();
            }

            // Apply pagination
            var skip = (searchRequest.Page - 1) * searchRequest.PageSize;
            return staffMembers.Skip(skip).Take(searchRequest.PageSize).ToList();
        }

        public async Task<List<StaffMemberDto>> GetStaffMembersByRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            var searchRequest = new StaffSearchRequest
            {
                Role = role,
                Page = 1,
                PageSize = 1000 // Get all for role-based queries
            };

            return await GetAllStaffMembersAsync(searchRequest, cancellationToken);
        }

        private async Task<StaffMemberDto?> GetStaffMemberByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get user profile from UserService
                var userResponse = await _userServiceClient.GetAsync($"/api/users/{userId}", cancellationToken);
                
                if (!userResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get user profile for userId {UserId}: {StatusCode}", userId, userResponse.StatusCode);
                    return null;
                }

                var userContent = await userResponse.Content.ReadAsStringAsync(cancellationToken);
                var userProfile = JsonSerializer.Deserialize<dynamic>(userContent);

                // Get practitioner-specific data
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
                var receptionist = await _context.Receptionists.FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);

                if (doctor == null && receptionist == null)
                {
                    return null;
                }

                // Get specializations for doctors
                List<SpecializationDto> specializations = new();
                if (doctor != null)
                {
                    var doctorSpecializations = await _context.DoctorSpecializations
                        .Where(ds => ds.DoctorId == doctor.Id)
                        .Join(_context.Specializations, ds => ds.SpecializationId, s => s.Id, (ds, s) => s)
                        .ToListAsync(cancellationToken);

                    specializations = doctorSpecializations.Select(s => new SpecializationDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        IsPrimary = true // You might want to add this logic
                    }).ToList();
                }

                var staffMember = new StaffMemberDto
                {
                    Id = doctor?.Id ?? receptionist!.Id,
                    Role = doctor != null ? "Doctor" : "Receptionist",
                    Profile = new ProfileDto
                    {
                        FirstName = userProfile?.GetProperty("firstName").GetString() ?? "",
                        LastName = userProfile?.GetProperty("lastName").GetString() ?? "",
                        Email = userProfile?.GetProperty("email").GetString() ?? "",
                        Phone = userProfile?.GetProperty("phone").GetString(),
                        DateOfBirth = userProfile?.GetProperty("dateOfBirth").GetDateTime() ?? DateTime.MinValue,
                        Gender = userProfile?.GetProperty("gender").GetString() ?? "",
                        AddressLine1 = userProfile?.GetProperty("addressLine1").GetString() ?? "",
                        AddressLine2 = userProfile?.GetProperty("addressLine2").GetString(),
                        City = userProfile?.GetProperty("city").GetString() ?? "",
                        State = userProfile?.GetProperty("state").GetString() ?? "",
                        ZipCode = userProfile?.GetProperty("zipCode").GetString() ?? "",
                        Country = userProfile?.GetProperty("country").GetString() ?? ""
                    },
                    IsActive = userProfile?.GetProperty("isActive").GetBoolean() ?? true,
                    CreatedAt = doctor?.CreatedAt ?? receptionist!.CreatedAt,
                    UpdatedAt = doctor?.UpdatedAt ?? receptionist!.UpdatedAt
                };

                return staffMember;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get staff member by userId {UserId}", userId);
                return null;
            }
        }
    }
}
