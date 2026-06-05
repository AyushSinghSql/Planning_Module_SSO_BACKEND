using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;
using PlanningAPI.Models;
using System;
using WebApi.Entities;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/user-projects")]
    public class UserProjectsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public UserProjectsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ---------------------------
        // Assign User to Project
        // ---------------------------
        [HttpPost("assign")]
        public async Task<IActionResult> AssignUserToProject(
            [FromQuery] int userId,
            [FromQuery] string projId)
        {
            var exists = await _context.UserProjectMaps
                .AnyAsync(x => x.UserId == userId && x.ProjId == projId);

            if (exists)
                return Conflict("User already assigned to project");

            _context.UserProjectMaps.Add(new UserProjectMap
            {
                UserId = userId,
                ProjId = projId
            });

            await _context.SaveChangesAsync();
            return Ok("User assigned to project");
        }

        // ---------------------------
        // Remove User from Project
        // ---------------------------
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveUserFromProject(
            [FromQuery] int userId,
            [FromQuery] string projId)
        {
            var map = await _context.UserProjectMaps
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProjId == projId);

            if (map == null)
                return NotFound("Mapping not found");

            _context.UserProjectMaps.Remove(map);
            await _context.SaveChangesAsync();
            return Ok("User removed from project");
        }

        // ---------------------------
        // Get Projects for User
        // ---------------------------
        [HttpGet("projects/{userId}")]
        public async Task<IActionResult> GetProjectsByUser(int userId)
        {
            var projects = await _context.UserProjectMaps
                .Where(x => x.UserId == userId)
                .Select(x => new
                {
                    x.Project.ProjId,
                    x.Project.ProjName,
                    x.Project.OrgId
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(projects);
        }

        // ---------------------------
        // Get Users for Project
        // ---------------------------
        [HttpGet("users/{projId}")]
        public async Task<IActionResult> GetUsersByProject(string projId)
        {
            var users = await _context.UserProjectMaps
                .Where(x => x.ProjId == projId)
                .Select(x => new
                {
                    x.User.UserId,
                    x.User.Username,
                    x.User.FullName,
                    x.User.Email
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("bulk-sync")]
        public async Task<IActionResult> BulkSyncProjectUsers(
            [FromBody] BulkProjectUserToggleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProjId))
                return BadRequest("ProjectId is required");

            request.UserIds ??= new List<int>();

            // 1️⃣ Current users assigned to project
            var existingUserIds = await _context.UserProjectMaps
                .Where(x => x.ProjId == request.ProjId)
                .Select(x => x.UserId)
                .ToListAsync();

            var existingSet = existingUserIds.ToHashSet();
            var requestSet = request.UserIds.ToHashSet();

            // 2️⃣ Identify NEW users to ADD
            var toAddUserIds = requestSet.Except(existingSet);

            // 3️⃣ Identify users to REMOVE
            var toRemoveUserIds = existingSet.Except(requestSet);

            // -------------------------
            // REMOVE
            // -------------------------
            if (toRemoveUserIds.Any())
            {
                var toRemove = await _context.UserProjectMaps
                    .Where(x => x.ProjId == request.ProjId &&
                                toRemoveUserIds.Contains(x.UserId))
                    .ToListAsync();

                _context.UserProjectMaps.RemoveRange(toRemove);
            }

            // -------------------------
            // ADD
            // -------------------------
            if (toAddUserIds.Any())
            {
                var toAdd = toAddUserIds
                    .Select(uid => new UserProjectMap
                    {
                        UserId = uid,
                        ProjId = request.ProjId
                    })
                    .ToList();

                _context.UserProjectMaps.AddRange(toAdd);
            }

            await _context.SaveChangesAsync();

            // 4️⃣ Return FINAL assigned users
            var finalUsers = await _context.UserProjectMaps
                .Where(x => x.ProjId == request.ProjId)
                .Select(x => new
                {
                    x.User.UserId,
                    x.User.Username,
                    x.User.FullName
                })
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return Ok(new
            {
                ProjectId = request.ProjId,
                AddedUserIds = toAddUserIds,
                RemovedUserIds = toRemoveUserIds,
                TotalUsers = finalUsers.Count,
                Users = finalUsers
            });
        }

        // ---------------------------
        // Get Users for Project
        // ---------------------------
        [HttpGet("Groups/{userId}")]
        public async Task<IActionResult> GetUsersGroups(int userId)
        {
            var orgGroups = await _context.OrgGroupUserMappings
                .Where(x => x.UserId == userId && x.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return Ok(orgGroups);
        }
        [HttpPost("BulkSyncUsersGroups")]
        public async Task<IActionResult> BulkSyncUsersGroups(
    [FromBody] BulkUserGroupsToggleRequest request)
        {

            request.GroupIds ??= new List<int>();

            // 1️⃣ Current users assigned to project
            var existingUserIds = await _context.OrgGroupUserMappings
                .Where(x => x.UserId == request.UserId)
                .Select(x => x.OrgGroupId)
                .ToListAsync();

            var existingSet = existingUserIds.ToHashSet();
            var requestSet = request.GroupIds.ToHashSet();

            // 2️⃣ Identify NEW users to ADD
            var toAddUserIds = requestSet.Except(existingSet);

            // 3️⃣ Identify users to REMOVE
            var toRemoveUserIds = existingSet.Except(requestSet);

            // -------------------------
            // REMOVE
            // -------------------------
            if (toRemoveUserIds.Any())
            {
                var toRemove = await _context.OrgGroupUserMappings
                    .Where(x => x.UserId == request.UserId &&
                                toRemoveUserIds.Contains(x.OrgGroupId))
                    .ToListAsync();

                _context.OrgGroupUserMappings.RemoveRange(toRemove);
            }

            // -------------------------
            // ADD
            // -------------------------
            if (toAddUserIds.Any())
            {
                var toAdd = toAddUserIds
                    .Select(groupId => new OrgGroupUserMapping
                    {
                        OrgGroupId = groupId,
                        UserId = request.UserId
                    })
                    .ToList();

                _context.OrgGroupUserMappings.AddRange(toAdd);
            }

            await _context.SaveChangesAsync();

            // 4️⃣ Return FINAL assigned Groups
            var finalGroups = await _context.OrgGroupUserMappings
                .Where(x => x.UserId == request.UserId)
                .Select(x => x.OrgGroup)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                UserId = request.UserId,
                AddedGroupIds = toAddUserIds,
                RemovedGroupIds = toRemoveUserIds,
                TotalGroups = finalGroups.Count,
                OrgGroups = finalGroups
            });
        }


        // ---------------------------
        // Get all Groups
        // ---------------------------
        [HttpGet("GetGroups")]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.OrgGroup
                .AsNoTracking()
                .ToListAsync();

            return Ok(groups);
        }


        // ---------------------------
        // Get Users for Project
        // ---------------------------
        [HttpGet("Orgs/{groupId}")]
        public async Task<IActionResult> GetOrgsByGroups(int groupId)
        {
            var users = await _context.OrgGroupOrgMappings
                .Where(x => x.OrgGroupId == groupId)
                .Select(x => x.Organization)
                .AsNoTracking()
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("BulkSyncGroupOrgs")]
        public async Task<IActionResult> BulkSyncGroupOrgs(
            [FromBody] BulkGroupOrgsToggleRequest request)
        {

            request.OrgIds ??= new List<string>();

            // 1️⃣ Current users assigned to project
            var existingOrgs = await _context.OrgGroupOrgMappings
                .Where(x => x.OrgGroupId == request.GroupId)
                .Select(x => x.OrgId)
                .ToListAsync();

            var existingSet = existingOrgs.ToHashSet();
            var requestSet = request.OrgIds.ToHashSet();

            // 2️⃣ Identify NEW users to ADD
            var toAddOrgIds = requestSet.Except(existingSet);

            // 3️⃣ Identify users to REMOVE
            var toRemoveOrgIds = existingSet.Except(requestSet);

            // -------------------------
            // REMOVE
            // -------------------------
            if (toRemoveOrgIds.Any())
            {
                var toRemove = await _context.OrgGroupOrgMappings
                    .Where(x => x.OrgGroupId == request.GroupId &&
                                toRemoveOrgIds.Contains(x.OrgId))
                    .ToListAsync();

                _context.OrgGroupOrgMappings.RemoveRange(toRemove);
            }

            // -------------------------
            // ADD
            // -------------------------
            if (toAddOrgIds.Any())
            {
                var toAdd = toAddOrgIds
                    .Select(OrgId => new OrgGroupOrgMapping
                    {
                        OrgId = OrgId,
                        OrgGroupId = request.GroupId
                    })
                    .ToList();

                _context.OrgGroupOrgMappings.AddRange(toAdd);
            }

            await _context.SaveChangesAsync();
            //return Ok();
            // 4️⃣ Return FINAL assigned users
            var finalOrgs = await _context.OrgGroupOrgMappings
                .Where(x => x.OrgGroupId == request.GroupId)
                .Select(x => x.Organization)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                GroupId = request.GroupId,
                AddeOrgIds = toAddOrgIds,
                RemovedOrgIds = toRemoveOrgIds,
                TotalOrgs = finalOrgs.Count,
                Orgs = finalOrgs
            });
        }


        //// ---------------------------
        //// Get all Groups
        //// ---------------------------
        //[HttpGet("GetUserOrgs")]
        //public async Task<IActionResult> GetUserOrgs()
        //{
        //    var orgs = await _context.UserOrgMappings
        //        .AsNoTracking()
        //        .ToListAsync();

        //    return Ok(orgs);
        //}


        [HttpPost("OrgGroups")]
        public async Task<IActionResult> Create(
       [FromBody] OrgGroupCreateUpdateDto dto)
        {
            // Optional: Unique Code validation
            var exists = await _context.OrgGroup
                .AnyAsync(x => x.OrgGroupCode == dto.OrgGroupCode);

            if (exists)
                return BadRequest("OrgGroupCode already exists.");

            var entity = new OrgGroup
            {
                OrgGroupCode = dto.OrgGroupCode,
                OrgGroupName = dto.OrgGroupName,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name
            };

            _context.OrgGroup.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = entity.OrgGroupId },
                entity);
        }

        // =====================================================
        // READ (By Id)
        // =====================================================
        [HttpGet("OrgGroups/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var entity = await _context.OrgGroup
                .AsNoTracking()
                .Include(x => x.OrgMappings)
                .Include(x => x.UserMappings)
                .FirstOrDefaultAsync(x => x.OrgGroupId == id);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [HttpPut("OrgGroups/{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] OrgGroupCreateUpdateDto dto)
        {
            var entity = await _context.OrgGroup
                .FirstOrDefaultAsync(x => x.OrgGroupId == id);

            if (entity == null)
                return NotFound();

            entity.OrgGroupCode = dto.OrgGroupCode;
            entity.OrgGroupName = dto.OrgGroupName;
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;
            entity.ModifiedAt = DateTime.UtcNow;
            entity.ModifiedBy = User?.Identity?.Name;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // =====================================================
        // DELETE
        // =====================================================
        [HttpDelete("OrgGroups/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.OrgGroup
                .Include(x => x.OrgMappings)
                .Include(x => x.UserMappings)
                .FirstOrDefaultAsync(x => x.OrgGroupId == id);

            if (entity == null)
                return NotFound();

            // Optional: prevent delete if mappings exist
            if (entity.OrgMappings.Any() || entity.UserMappings.Any())
                return BadRequest("Cannot delete OrgGroup with active mappings.");

            _context.OrgGroup.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("OrgGroups/BulkDelete")]
        public async Task<IActionResult> BulkDelete(
        [FromBody] List<int> request)
        {
            if (request == null || !request.Any())
                return BadRequest("OrgGroupIds cannot be empty.");

            var orgGroups = await _context.OrgGroup
                .Where(x => request.Contains(x.OrgGroupId))
                .Include(x => x.OrgMappings)
                .Include(x => x.UserMappings)
                .ToListAsync();

            if (!orgGroups.Any())
                return NotFound("No OrgGroups found.");

            var blockedIds = orgGroups
                .Where(x => x.OrgMappings.Any() || x.UserMappings.Any())
                .Select(x => x.OrgGroupId)
                .ToList();

            if (blockedIds.Any())
            {
                return BadRequest(new
                {
                    Message = "Some OrgGroups have active mappings and cannot be deleted.",
                    BlockedOrgGroupIds = blockedIds
                });
            }

            _context.OrgGroup.RemoveRange(orgGroups);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                DeletedCount = orgGroups.Count
            });
        }


        //    [HttpPost("BulkSyncUserOrgs")]
        //    public async Task<IActionResult> BulkSyncUserOrgs(
        //[FromBody] BulkUserOrgsToggleRequest request)
        //    {

        //        request.OrgIds ??= new List<string>();

        //        // 1️⃣ Current users assigned to project
        //        var existingOrgs = await _context.UserOrgMappings
        //            .Where(x => x.UserId == request.UserId)
        //            .Select(x => x.OrgId)
        //            .ToListAsync();

        //        var existingSet = existingOrgs.ToHashSet();
        //        var requestSet = request.OrgIds.ToHashSet();

        //        // 2️⃣ Identify NEW users to ADD
        //        var toAddOrgIds = requestSet.Except(existingSet);

        //        // 3️⃣ Identify users to REMOVE
        //        var toRemoveOrgIds = existingSet.Except(requestSet);

        //        // -------------------------
        //        // REMOVE
        //        // -------------------------
        //        if (toRemoveOrgIds.Any())
        //        {
        //            var toRemove = await _context.UserOrgMappings
        //                .Where(x => x.UserId == request.UserId &&
        //                            toRemoveOrgIds.Contains(x.OrgId))
        //                .ToListAsync();

        //            _context.UserOrgMappings.RemoveRange(toRemove);
        //        }

        //        // -------------------------
        //        // ADD
        //        // -------------------------
        //        if (toAddOrgIds.Any())
        //        {
        //            var toAdd = toAddOrgIds
        //                .Select(OrgId => new UserOrgMapping
        //                {
        //                    OrgId = OrgId,
        //                    UserId = request.UserId
        //                })
        //                .ToList();

        //            _context.UserOrgMappings.AddRange(toAdd);
        //        }

        //        await _context.SaveChangesAsync();
        //        //return Ok();
        //        // 4️⃣ Return FINAL assigned users
        //        var finalOrgs = await _context.UserOrgMappings
        //            .Where(x => x.UserId == request.UserId)
        //            .Select(x => x.Orgnization)
        //            .AsNoTracking()
        //            .ToListAsync();

        //        return Ok(new
        //        {
        //            GroupId = request.UserId,
        //            AddeOrgIds = toAddOrgIds,
        //            RemovedOrgIds = toRemoveOrgIds,
        //            TotalOrgs = finalOrgs.Count,
        //            Orgs = finalOrgs
        //        });
        //    }

    }

}
