using Api.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Base controller providing consistent response helpers.
/// All API controllers should inherit from this.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected OkObjectResult Ok<T>(T data, object? meta = null) =>
        base.Ok(ApiResponse<T>.Success(data, meta));

    protected ObjectResult Created<T>(string routeName, object routeValues, T data) =>
        base.CreatedAtRoute(routeName, routeValues, ApiResponse<T>.Success(data));

    protected ObjectResult Fail<T>(string message, int statusCode = 400) =>
        StatusCode(statusCode, ApiResponse<T>.Fail(message));
}
