using MediatR;
using Microsoft.AspNetCore.Mvc;
using Spider.ProjectManagement.Application.Commands;
using Spider.ProjectManagement.Application.DTOs;
using Spider.ProjectManagement.Application.Queries;

namespace Spider.ProjectManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IMediator mediator, ILogger<ProjectsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAllProjects()
    {
        try
        {
            var projects = await _mediator.Send(new GetAllProjectsQuery());
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all projects");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id)
    {
        try
        {
            var project = await _mediator.Send(new GetProjectByIdQuery(id));
            if (project == null)
                return NotFound($"Project with ID {id} not found");

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("parent/{parentId:guid?}")]
    public async Task<ActionResult<List<ProjectDto>>> GetProjectsByParent(Guid? parentId)
    {
        try
        {
            var projects = await _mediator.Send(new GetProjectsByParentQuery(parentId));
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by parent {ParentId}", parentId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<ProjectDto>>> GetProjectsByStatus(string status)
    {
        try
        {
            var projects = await _mediator.Send(new GetProjectsByStatusQuery(status));
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by status {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("root")]
    public async Task<ActionResult<List<ProjectDto>>> GetRootProjects()
    {
        try
        {
            var projects = await _mediator.Send(new GetRootProjectsQuery());
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting root projects");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ProjectStatisticsDto>> GetProjectStatistics()
    {
        try
        {
            var statistics = await _mediator.Send(new GetProjectStatisticsQuery());
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
    {
        try
        {
            var command = new CreateProjectCommand(
                dto.Name,
                dto.Description,
                dto.Configuration,
                dto.ParentProjectId,
                dto.CreatedBy);

            var project = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project {ProjectName}", dto.Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            var command = new UpdateProjectCommand(id, dto.Name, dto.Description, dto.ModifiedBy);
            var project = await _mediator.Send(command);
            return Ok(project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Project not found for update: {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ProjectDto>> ChangeProjectStatus(Guid id, [FromBody] ProjectStatusChangeDto dto)
    {
        try
        {
            var command = new ChangeProjectStatusCommand(id, dto.Status, dto.ModifiedBy, dto.Reason);
            var project = await _mediator.Send(command);
            return Ok(project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Project not found for status change: {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status for project {ProjectId}: {Status}", id, dto.Status);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing project status {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ProjectDto>> ActivateProject(Guid id, [FromBody] string modifiedBy)
    {
        try
        {
            var command = new ActivateProjectCommand(id, modifiedBy);
            var project = await _mediator.Send(command);
            return Ok(project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Project not found for activation: {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating project {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<ProjectDto>> DeactivateProject(Guid id, [FromBody] DeactivateProjectCommand command)
    {
        try
        {
            if (command.Id != id)
                return BadRequest("ID mismatch");

            var project = await _mediator.Send(command);
            return Ok(project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Project not found for deactivation: {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating project {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ProjectDto>> ArchiveProject(Guid id, [FromBody] ArchiveProjectCommand command)
    {
        try
        {
            if (command.Id != id)
                return BadRequest("ID mismatch");

            var project = await _mediator.Send(command);
            return Ok(project);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Project not found for archiving: {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving project {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteProject(Guid id, [FromQuery] string deletedBy)
    {
        try
        {
            var command = new DeleteProjectCommand(id, deletedBy);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Project not found for deletion: {ProjectId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("health")]
    public ActionResult<object> HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "ProjectManagement",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}